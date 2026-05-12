const { contextBridge, ipcRenderer } = require('electron');

// ============================================================
// Channel whitelists — defence-in-depth for contextBridge
// Prevents arbitrary IPC from reaching main process even if
// renderer is compromised (e.g. malicious dependency in bundle).
// ============================================================
// NOTE — Channel whitelists were missing several send/invoke channels that
// main.js actually registered (show-osd, open-recordings, save-hotkeys).
// Renderer calls to those channels were being silently dropped, breaking
// the OSD button, "Open Recordings" and the hotkey editor.
// Filled in by Claude (Anthropic) model `claude-opus-4-7`.
const VALID_SEND_CHANNELS = new Set([
  'close-app',
  'minimize-to-tray',
  'set-window-size',
  'save-settings',
  'toggle-session-mute',
  'set-session-volume',
  'set-master-volume',
  'set-ducking',
  'set-boost',
  'start-recording',
  'stop-recording',
  'toggle-mini-player',
  'show-osd',
  'open-recordings',
]);

const VALID_INVOKE_CHANNELS = new Set([
  'get-app-icon',
  'load-settings',
  'apply-profile',
  'get-audio-sessions',
  'get-master-info',
  'get-hotkeys',
  'get-advanced-settings',
  // save-hotkeys is now invoke-based (async) — was previously sendSync,
  // which blocked the renderer thread and was not exposed by this preload.
  'save-hotkeys',
]);

const VALID_ON_CHANNELS = new Set([
  // Main process emits 'audio-peaks'. The legacy 'bridge-peaks' name is
  // kept for backwards compatibility with any downstream listener but is
  // no longer produced — App.svelte now subscribes to 'audio-peaks'.
  'audio-peaks',
  'bridge-peaks',
  'hotkey-boost-changed',
  'hotkey-recording-changed',
  'save-settings-response',
]);

// ============================================================
// Listener map: Map<channel, Map<originalFunc, wrapper>>
// Uses function *reference* as Map key (not string interpolation),
// so two different callbacks on the same channel never collide.
// ============================================================
const listenerMap = new Map();

// ============================================================
// isDev flag — passed explicitly from main via additionalArguments.
// process.env.NODE_ENV is unreliable in preload in packaged builds.
// ============================================================
const isDev = process.argv.some(a => a.startsWith('--vf-dev=true'));
const isMini = process.argv.some(a => a.startsWith('--vf-mode=mini'));

// ============================================================
// Exposed API
// ============================================================
contextBridge.exposeInMainWorld('electron', {
  isDev,
  isMini,
  ipcRenderer: {
    send: (channel, data) => {
      if (!VALID_SEND_CHANNELS.has(channel)) return;
      ipcRenderer.send(channel, data);
    },

    /**
     * Subscribe to a channel. Returns an unsubscribe function (modern pattern).
     * Usage in Svelte:
     *   const unsub = window.electron.ipcRenderer.on('audio-peaks', handler);
     *   onDestroy(unsub);
     */
    on: (channel, func) => {
      if (!VALID_ON_CHANNELS.has(channel)) return () => {};

      const wrapper = (_event, ...args) => func(...args);

      if (!listenerMap.has(channel)) listenerMap.set(channel, new Map());
      listenerMap.get(channel).set(func, wrapper);
      ipcRenderer.on(channel, wrapper);

      // Return unsubscribe closure — no string matching needed
      return () => {
        ipcRenderer.removeListener(channel, wrapper);
        const channelMap = listenerMap.get(channel);
        if (channelMap) {
          channelMap.delete(func);
          if (channelMap.size === 0) listenerMap.delete(channel);
        }
      };
    },

    removeListener: (channel, func) => {
      const channelMap = listenerMap.get(channel);
      if (!channelMap) return;
      const wrapper = channelMap.get(func);
      if (wrapper) {
        ipcRenderer.removeListener(channel, wrapper);
        channelMap.delete(func);
        if (channelMap.size === 0) listenerMap.delete(channel);
      }
    },

    invoke: (channel, ...args) => {
      if (!VALID_INVOKE_CHANNELS.has(channel)) return Promise.resolve(null);
      return ipcRenderer.invoke(channel, ...args);
    },
  },
});

window.addEventListener('DOMContentLoaded', () => {
  if (isDev) {
    console.log('VolumeFlow Preload Loaded (dev)');
  }
}, { once: true });
