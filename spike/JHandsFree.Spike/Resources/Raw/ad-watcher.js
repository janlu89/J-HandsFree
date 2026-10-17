(() => {
  'use strict';
  if (window.__jhfAdWatcher || !window.__jhfBridge) {
    return;
  }
  window.__jhfAdWatcher = true;

  const selectors = {
    player: '#movie_player',
    adShowingClass: 'ad-showing',
    skipButtonCandidates: 'button[class*="skip"], [role="button"][class*="skip"]',
  };
  const watchedAttributes = ['class', 'style', 'hidden', 'aria-hidden', 'disabled'];
  const snapshotLimit = 300000;
  const skipConfirmationWindowMs = 1000;
  const bridge = window.__jhfBridge;

  let player = null;
  let adStartedAt = 0;
  let skippableAt = 0;
  let skipRequestedAt = 0;
  let skipButton = null;
  let offeredSkip = false;

  const isAdShowing = () => player.classList.contains(selectors.adShowingClass);

  const isVisible = (element) => {
    const rect = element.getBoundingClientRect();
    const style = getComputedStyle(element);
    return rect.width > 0 && rect.height > 0 && style.visibility !== 'hidden' && style.display !== 'none' && Number(style.opacity) > 0;
  };

  const findSkipButton = () =>
    [...player.querySelectorAll(selectors.skipButtonCandidates)].find((element) => isVisible(element) && !element.disabled) ?? null;

  const describe = (element) => ({
    tag: element.tagName.toLowerCase(),
    classes: [...element.classList],
    text: element.textContent.trim().slice(0, 40),
    ariaLabel: element.getAttribute('aria-label'),
  });

  const sendSnapshot = (reason) => {
    bridge.send('spike.snapshot', { reason, html: player.outerHTML.slice(0, snapshotLimit) });
  };

  const onAdStarted = () => {
    adStartedAt = performance.now();
    skippableAt = 0;
    skipRequestedAt = 0;
    skipButton = null;
    offeredSkip = false;
    bridge.send('ad.started', { kind: 'unknown' });
    sendSnapshot('ad-started');
  };

  // An ad counts as skipped only when it ends shortly after the skip request; a request the page ignored does not count.
  const onAdEnded = () => {
    const now = performance.now();
    const msFromSkipRequestToEnd = skipRequestedAt > 0 ? Math.round(now - skipRequestedAt) : null;
    bridge.send('ad.ended', {
      skipped: msFromSkipRequestToEnd !== null && msFromSkipRequestToEnd < skipConfirmationWindowMs,
      msFromSkipRequestToEnd,
      offeredSkip,
      durationMs: Math.round(now - adStartedAt),
    });
    adStartedAt = 0;
  };

  const onSkippable = (button) => {
    skipButton = button;
    offeredSkip = true;
    skippableAt = performance.now();
    bridge.send('ad.skippable', { msSinceAdStart: Math.round(skippableAt - adStartedAt), button: describe(button) });
    sendSnapshot('ad-skippable');
  };

  // An ad pod can show several ads in a row; once the button disappears, the next ad may offer a new one.
  const onSkipButtonGone = () => {
    bridge.send('spike.skipButtonGone', {
      msSinceSkippable: Math.round(performance.now() - skippableAt),
      msSinceSkipRequest: skipRequestedAt > 0 ? Math.round(performance.now() - skipRequestedAt) : null,
      adStillShowing: isAdShowing(),
    });
    skipButton = null;
    skippableAt = 0;
  };

  const evaluate = () => {
    const showing = isAdShowing();
    if (showing && adStartedAt === 0) {
      onAdStarted();
    }
    if (!showing && adStartedAt > 0) {
      if (skipButton) {
        onSkipButtonGone();
      }
      onAdEnded();
      return;
    }
    if (!showing) {
      return;
    }
    if (skipButton && (!skipButton.isConnected || !isVisible(skipButton))) {
      onSkipButtonGone();
      return;
    }
    if (!skipButton) {
      const button = findSkipButton();
      if (button) {
        onSkippable(button);
      }
    }
  };

  const playerObserver = new MutationObserver(() => {
    if (player.isConnected) {
      evaluate();
      return;
    }
    playerObserver.disconnect();
    waitForPlayer();
  });

  const pageObserver = new MutationObserver(() => {
    const found = document.querySelector(selectors.player);
    if (found) {
      pageObserver.disconnect();
      watchPlayer(found);
    }
  });

  const watchPlayer = (element) => {
    player = element;
    playerObserver.observe(player, { attributes: true, attributeFilter: watchedAttributes, childList: true, subtree: true });
    console.info('[jhf-ads] watching the player');
    evaluate();
  };

  const waitForPlayer = () => {
    const found = document.querySelector(selectors.player);
    if (found) {
      watchPlayer(found);
      return;
    }
    pageObserver.observe(document.documentElement, { childList: true, subtree: true });
  };

  bridge.on('ad.skip', (message) => {
    if (!skipButton) {
      console.info('[jhf-ads] ad.skip received but no skip button is visible');
      return;
    }
    skipRequestedAt = performance.now();
    if (message.payload.mode === 'locate') {
      const rect = skipButton.getBoundingClientRect();
      bridge.send('ad.skipTarget', { x: rect.x, y: rect.y, width: rect.width, height: rect.height, devicePixelRatio: window.devicePixelRatio });
      return;
    }
    skipButton.click();
    bridge.send('spike.skipClicked', { msSinceSkippable: Math.round(skipRequestedAt - skippableAt) });
  });

  waitForPlayer();
})();
