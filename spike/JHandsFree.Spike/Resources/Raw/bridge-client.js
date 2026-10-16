(() => {
  'use strict';
  if (window.__jhfBridge || typeof jhf === 'undefined') {
    return;
  }

  const protocol = 1;
  const pingCount = 20;
  const pingTimes = [];
  const handlers = new Map();

  const send = (type, payload = {}) => {
    jhf.postMessage(JSON.stringify({ protocol, type, sentAtMs: Date.now(), payload }));
  };

  const findVideo = () => document.querySelector('video');
  const sendPing = () => send('bridge.ping', { sequence: pingTimes.length });

  const reportPingTimes = () => {
    const sorted = [...pingTimes].sort((first, second) => first - second);
    const median = sorted[Math.floor(sorted.length / 2)];
    console.info(`[jhf-bridge] ${pingCount} pings: min ${sorted[0]} ms, median ${median} ms, max ${sorted[sorted.length - 1]} ms`);
  };

  const handleEcho = (message) => {
    const roundTripMs = Date.now() - message.payload.echoOfSentAtMs;
    if (message.payload.echoOf === 'script.ready') {
      console.info(`[jhf-bridge] script.ready round trip ${roundTripMs} ms`);
      sendPing();
      return;
    }
    pingTimes.push(roundTripMs);
    if (pingTimes.length < pingCount) {
      sendPing();
      return;
    }
    reportPingTimes();
  };

  handlers.set('player.play', () => findVideo()?.play());
  handlers.set('player.pause', () => findVideo()?.pause());
  handlers.set('bridge.echo', handleEcho);

  jhf.addEventListener('message', (event) => {
    const message = JSON.parse(event.data);
    if (message.protocol !== protocol) {
      console.warn(`[jhf-bridge] ignored protocol ${message.protocol}`);
      return;
    }
    const handle = handlers.get(message.type);
    if (!handle) {
      console.info(`[jhf-bridge] ignored ${message.type}`);
      return;
    }
    handle(message);
  });

  const on = (type, handler) => handlers.set(type, handler);

  window.__jhfBridge = { send, on };
  send('script.ready', { bundleVersion: 'spike', selectorsVersion: 'spike' });
  console.info(`[jhf-bridge] installed on ${location.origin}`);
})();
