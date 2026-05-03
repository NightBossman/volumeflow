const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('electron', {
  ipcRenderer: {
    send: (channel, data) => ipcRenderer.send(channel, data),
    on: (channel, func) => ipcRenderer.on(channel, (event, ...args) => func(...args)),
    invoke: (channel, ...args) => ipcRenderer.invoke(channel, ...args),
    removeListener: (channel, func) => ipcRenderer.removeListener(channel, func),
  }
});

window.addEventListener('DOMContentLoaded', () => {
  if (process.env.NODE_ENV === 'development') {
    console.log('VolumeFlow Preload Loaded');
  }
}, { once: true });
