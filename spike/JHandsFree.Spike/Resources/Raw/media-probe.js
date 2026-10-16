(() => {
  'use strict';
  if (window.__jhfMediaProbe) {
    return;
  }
  window.__jhfMediaProbe = true;

  const playerStates = { playing: 'playing', pause: 'paused', ended: 'ended' };

  const reportPlayerState = (eventType, video) => {
    const state = playerStates[eventType];
    if (!state || location.pathname !== '/watch' || !window.__jhfBridge) {
      return;
    }
    window.__jhfBridge.send('player.state', {
      state,
      videoId: new URLSearchParams(location.search).get('v'),
      positionSeconds: video.currentTime,
      durationSeconds: Number.isFinite(video.duration) ? video.duration : null,
    });
  };

  const reportMediaEvent = (event) => {
    if (!(event.target instanceof HTMLMediaElement)) {
      return;
    }
    const position = event.target.currentTime.toFixed(1);
    console.info(`[jhf-probe] media ${event.type} at ${position}s, document.hidden=${document.hidden}`);
    reportPlayerState(event.type, event.target);
  };
  for (const type of ['play', 'playing', 'pause', 'ended']) {
    document.addEventListener(type, reportMediaEvent, true);
  }

  document.addEventListener('visibilitychange', () => {
    console.info(`[jhf-probe] visibilitychange reached the page, document.hidden=${document.hidden}`);
  }, true);

  console.info(`[jhf-probe] installed on ${location.origin}`);
})();
