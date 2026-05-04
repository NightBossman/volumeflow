const { contextBridge, ipcRenderer } = require('electron');

// ============================================================
// Channel whitelists — defence-in-depth for contextBridge
// Prevents arbitrary IPC from reaching main process even if
// renderer is compromised (e.g. malicious dependency in bundle).
// ============================================================
const VALID_SEND_CHANNELS = new Set([
  'close-app',
  'minimize-to-tray',
  'set-window-size',
  'save-settings',
  'toggle-session-mute',
  'set-session-volume',
  'set-master-volume',
  'set-ducking',
]);

const VALID_INVOKE_CHANNELS = new Set([
  'get-app-icon',
  'load-settings',
  'apply-profile',
  'get-audio-sessions',
  'get-master-info',
]);

const VALID_ON_CHANNELS = new Set([
  'audio-peaks',
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
const isDev = process.argv.some(a => a === '--vf-dev=true');

// ============================================================
// Exposed API
// ============================================================
contextBridge.exposeInMainWorld('electron', {
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
