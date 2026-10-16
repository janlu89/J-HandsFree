(() => {
  'use strict';
  if (window.__jhfMediaProbe) {
    return;
  }
  window.__jhfMediaProbe = true;

  const reportMediaEvent = (event) => {
    if (!(event.target instanceof HTMLMediaElement)) {
      return;
    }
    const position = event.target.currentTime.toFixed(1);
    console.info(`[jhf-probe] media ${event.type} at ${position}s, document.hidden=${document.hidden}`);
  };
  for (const type of ['play', 'playing', 'pause', 'ended']) {
    document.addEventListener(type, reportMediaEvent, true);
  }

  document.addEventListener('visibilitychange', () => {
    console.info(`[jhf-probe] visibilitychange reached the page, document.hidden=${document.hidden}`);
  }, true);

  console.info(`[jhf-probe] installed on ${location.origin}`);
})();
