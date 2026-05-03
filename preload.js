const { contextBridge, ipcRenderer } = require('electron');

// Maps original callback -> ipc wrapper, per channel, so removeListener can
// find and remove the exact wrapper that was registered.
const listenerMap = new Map(); // key: `${channel}::${func}` -> wrapper

contextBridge.exposeInMainWorld('electron', {
  ipcRenderer: {
    send: (channel, data) => ipcRenderer.send(channel, data),

    on: (channel, func) => {
      const wrapper = (_event, ...args) => func(...args);
      const key = `${channel}::${func}`;
      listenerMap.set(key, wrapper);
      ipcRenderer.on(channel, wrapper);
    },

    removeListener: (channel, func) => {
      const key = `${channel}::${func}`;
      const wrapper = listenerMap.get(key);
      if (wrapper) {
        ipcRenderer.removeListener(channel, wrapper);
        listenerMap.delete(key);
      }
    },

    invoke: (channel, ...args) => ipcRenderer.invoke(channel, ...args),
  }
});

window.addEventListener('DOMContentLoaded', () => {
  if (process.env.NODE_ENV === 'development') {
    console.log('VolumeFlow Preload Loaded');
  }
}, { once: true });
