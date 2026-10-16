(() => {
  'use strict';
  if (window.__jhfVisibilityShield) {
    return;
  }
  window.__jhfVisibilityShield = true;

  const realHidden = Object.getOwnPropertyDescriptor(Document.prototype, 'hidden').get;
  const alwaysVisible = { get: () => 'visible', configurable: true };
  const neverHidden = { get: () => false, configurable: true };
  Object.defineProperty(document, 'visibilityState', alwaysVisible);
  Object.defineProperty(document, 'webkitVisibilityState', alwaysVisible);
  Object.defineProperty(document, 'hidden', neverHidden);
  Object.defineProperty(document, 'webkitHidden', neverHidden);
  document.hasFocus = () => true;

  // Element blur events also pass through the window in the capture phase; only page-level ones are swallowed.
  const swallowPageLevelEvent = (event) => {
    if (event.target !== window && event.target !== document) {
      return;
    }
    event.stopImmediatePropagation();
    console.info(`[jhf-shield] swallowed ${event.type}, real document.hidden=${realHidden.call(document)}`);
  };
  for (const type of ['visibilitychange', 'webkitvisibilitychange', 'blur']) {
    window.addEventListener(type, swallowPageLevelEvent, true);
  }

  console.info(`[jhf-shield] installed on ${location.origin}`);
})();
