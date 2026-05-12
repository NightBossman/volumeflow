import { app, BrowserWindow, ipcMain, Tray, Menu, nativeImage, globalShortcut } from 'electron';
import path from 'path';
import { fileURLToPath } from 'url';
import { spawn } from 'child_process';
import fs from 'fs';
import { createRequire } from 'module';

const require = createRequire(import.meta.url);

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const isDev = process.env.NODE_ENV === 'development';

let mainWindow = null;
let osdWindow = null;
let tray = null;
let isQuitting = false;

// ============================================================
// Global Hotkeys
// ============================================================

// Default hotkey configuration — saved/loaded from config
let hotkeys = {
  toggleRecording: 'Ctrl+Alt+R',
  toggleMasterMute: 'Ctrl+Alt+M',
  toggleBoost: 'Ctrl+Alt+B',
};

// pid last used for recording via hotkey
let hotkeyRecordingPid = null;

// Shared boost state — synced between UI and hotkey
let hotkeyBoostState = false;

function registerHotkeys() {
  globalShortcut.unregisterAll();

  // Toggle master mute
  if (hotkeys.toggleMasterMute) {
    const ok = globalShortcut.register(hotkeys.toggleMasterMute, async () => {
      await sendBridgeCommand({ action: 'toggle_master_mute' });
      showOSD('Master Mute toggled', 'mute');
    });
    if (!ok) console.warn('[Hotkeys] Failed to register:', hotkeys.toggleMasterMute);
  }

  // Toggle Smart Overdrive
  if (hotkeys.toggleBoost) {
    const ok = globalShortcut.register(hotkeys.toggleBoost, async () => {
      hotkeyBoostState = !hotkeyBoostState;
      await sendBridgeCommand({ action: 'set_boost', active: hotkeyBoostState, factor: 0.6 });
      showOSD(hotkeyBoostState ? 'Smart Overdrive ON' : 'Smart Overdrive OFF', 'boost');
      // Sync state to renderer
      if (mainWindow) mainWindow.webContents.send('hotkey-boost-changed', hotkeyBoostState);
    });
    if (!ok) console.warn('[Hotkeys] Failed to register:', hotkeys.toggleBoost);
  }

  // Toggle recording — records the currently active audio session
  if (hotkeys.toggleRecording) {
    const ok = globalShortcut.register(hotkeys.toggleRecording, async () => {
      const result = await sendBridgeCommand({ action: 'get_sessions' });
      if (!result || !result.sessions || result.sessions.length === 0) {
        showOSD('No active audio sessions', 'info');
        return;
      }
      // Toggle recording for first active session (or previously chosen)
      const target = hotkeyRecordingPid
        ? result.sessions.find(s => s.pid === hotkeyRecordingPid)
        : result.sessions[0];
      if (!target) {
        // Session disappeared — reset tracking and fall back to first available
        hotkeyRecordingPid = null;
        const fallback = result.sessions[0];
        hotkeyRecordingPid = fallback.pid;
        await sendBridgeCommand({ action: 'start_recording', pid: fallback.pid });
        showOSD(`Recording: ${fallback.name}`, 'record');
        if (mainWindow) mainWindow.webContents.send('hotkey-recording-changed', { pid: fallback.pid, active: true });
        return;
      }
      if (hotkeyRecordingPid) {
        // Stop recording
        await sendBridgeCommand({ action: 'stop_recording', pid: target.pid });
        hotkeyRecordingPid = null;
        showOSD(`Stopped recording: ${target.name}`, 'stop');
        if (mainWindow) mainWindow.webContents.send('hotkey-recording-changed', { pid: target.pid, active: false });
      } else {
        // Start recording
        hotkeyRecordingPid = target.pid;
        await sendBridgeCommand({ action: 'start_recording', pid: target.pid });
        showOSD(`Recording: ${target.name}`, 'record');
        if (mainWindow) mainWindow.webContents.send('hotkey-recording-changed', { pid: target.pid, active: true });
      }
    });
    if (!ok) console.warn('[Hotkeys] Failed to register:', hotkeys.toggleRecording);
  }

  console.log('[Hotkeys] Registered:', Object.entries(hotkeys).map(([k,v]) => `${k}=${v}`).join(', '));
}

function unregisterHotkeys() {
  globalShortcut.unregisterAll();
}

// Polling stats interval
let healthCheckInterval = null;

// Tracker for active fades to prevent overlaps
const activeFades = new Map(); // pid -> { cancel: () => void }

// ============================================================
// Audio Bridge - persistent child process
// ============================================================

let bridgeProcess = null;
let bridgeReady = false;
let pendingRequests = new Map(); // requestId -> { resolve, reject, timeout }
let requestCounter = 0;
let responseBuffer = '';

function startBridge() {
  let bridgePath = path.join(__dirname, 'AudioBridge.exe');
  
  if (!isDev) {
    bridgePath = bridgePath.replace('app.asar', 'app.asar.unpacked');
  }
  
  if (!fs.existsSync(bridgePath)) {
    console.error('AudioBridge.exe not found at:', bridgePath);
    return;
  }

  bridgeProcess = spawn(bridgePath, [], {
    stdio: ['pipe', 'pipe', 'pipe'],
    windowsHide: true,
  });

  bridgeProcess.stdin.on('error', (err) => {
    console.error('Bridge stdin error:', err.message);
  });

  bridgeProcess.stdout.setEncoding('utf8');
  bridgeProcess.stderr.setEncoding('utf8');

  bridgeProcess.stdout.on('data', (data) => {
    responseBuffer += data;
    
    let newlineIdx;
    while ((newlineIdx = responseBuffer.indexOf('\n')) !== -1) {
      const line = responseBuffer.substring(0, newlineIdx).trim();
      responseBuffer = responseBuffer.substring(newlineIdx + 1);
      
      if (!line) continue;
      try {
        const parsed = JSON.parse(line);
        
        if (parsed.status === 'ready') {
          bridgeReady = true;
          console.log('AudioBridge is ready');
          startHealthMonitoring();
          continue;
        }

        if (parsed.type === 'peaks') {
          if (mainWindow) {
            mainWindow.webContents.send('audio-peaks', parsed);
          }
          continue;
        }

        if (parsed.requestId && pendingRequests.has(parsed.requestId)) {
          const req = pendingRequests.get(parsed.requestId);
          pendingRequests.delete(parsed.requestId);
          clearTimeout(req.timeout);
          req.resolve(parsed);
        }
      } catch (err) {
        console.error('Bridge parse error:', err.message, 'line:', line);
      }
    }
  });

  bridgeProcess.stderr.on('data', (data) => {
    console.error('AudioBridge Error:', data);
  });

  bridgeProcess.on('close', (code) => {
    console.log(`AudioBridge process exited with code ${code}`);
    bridgeReady = false;
    
    if (!isQuitting) {
      setTimeout(() => startBridge(), 1000);
    }
  });

  bridgeProcess.on('error', (err) => {
    console.error('Bridge process error:', err);
    bridgeReady = false;
  });
}

function startHealthMonitoring() {
  if (healthCheckInterval) clearInterval(healthCheckInterval);
  
  healthCheckInterval = setInterval(async () => {
    if (!bridgeReady) return;

    try {
      const stats = await sendBridgeCommand({ action: 'get_stats' });
      if (stats && stats.status === 'ok') {
        if (stats.droppedPeaks > 100) {
          console.warn(`[Health] Performance Warning: AudioBridge dropped ${stats.droppedPeaks} peak frames due to stdout backpressure.`);
        }
      }
    } catch (err) {
      console.error('[Health] Failed to get bridge stats:', err);
    }
  }, 10000); // Check every 10 seconds
}

function stopHealthMonitoring() {
  if (healthCheckInterval) {
    clearInterval(healthCheckInterval);
    healthCheckInterval = null;
  }
}

function sendBridgeCommand(command) {
  return new Promise((resolve, reject) => {
    if (!bridgeProcess || !bridgeReady || !bridgeProcess.stdin.writable) {
      return resolve(null);
    }

    const requestId = String(++requestCounter);
    const timeoutHandle = setTimeout(() => {
      pendingRequests.delete(requestId);
      resolve(null); 
    }, 5000);

    pendingRequests.set(requestId, { resolve, reject, timeout: timeoutHandle });

    try {
      bridgeProcess.stdin.write(JSON.stringify({ ...command, requestId }) + '\n');
    } catch (err) {
      pendingRequests.delete(requestId);
      clearTimeout(timeoutHandle);
      resolve(null);
    }
  });
}

function stopBridge() {
  isQuitting = true;
  stopHealthMonitoring();
  if (bridgeProcess) {
    try {
      if (bridgeProcess.stdin.writable) {
        bridgeProcess.stdin.write('{"action":"exit"}\n');
      }
    } catch (e) { }
    
    setTimeout(() => {
      if (bridgeProcess) {
        try { bridgeProcess.kill(); } catch (e) { }
        bridgeProcess = null;
      }
    }, 800);
  }
}

// ============================================================
// Window & Tray
// ============================================================

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 400,
    height: 600,
    frame: false,
    transparent: true,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      sandbox: false,
      preload: path.join(__dirname, 'preload.cjs'),
      additionalArguments: [`--vf-dev=${isDev}`],
    },
  });

  mainWindow.on('close', (event) => {
    if (!isQuitting) {
      event.preventDefault();
      mainWindow.hide();
    }
  });

  if (isDev) {
    mainWindow.loadURL('http://localhost:5173');
  } else {
    mainWindow.loadFile(path.join(__dirname, 'dist/index.html'));
  }
}

function createOSDWindow() {
  osdWindow = new BrowserWindow({
    width: 300,
    height: 120,
    frame: false,
    transparent: true,
    alwaysOnTop: true,
    skipTaskbar: true,
    focusable: false,
    resizable: false,
    show: false,
    webPreferences: {
      nodeIntegration: true,
      contextIsolation: false
    }
  });

  osdWindow.loadFile(path.join(__dirname, 'osd.html'));
  osdWindow.setIgnoreMouseEvents(true);
}

function showOSD(message, icon = 'info') {
  if (!osdWindow) createOSDWindow();

  osdWindow.webContents.send('show-osd', { message, icon });
  osdWindow.show();

  // Position at bottom right
  const { screen } = require('electron');
  const primaryDisplay = screen.getPrimaryDisplay();
  const { width, height } = primaryDisplay.workAreaSize;
  osdWindow.setPosition(width - 320, height - 140);

  setTimeout(() => {
    if (osdWindow) osdWindow.hide();
  }, 3000);
}

function createTray() {
  const size = 16;
  const canvas = Buffer.alloc(size * size * 4); // RGBA

  const bars = [
    { x: 3, h: 8 },
    { x: 7, h: 12 },
    { x: 11, h: 6 },
  ];

  for (const bar of bars) {
    const startY = size - bar.h;
    for (let y = startY; y < size; y++) {
      for (let dx = 0; dx < 2; dx++) {
        const x = bar.x + dx;
        const idx = (y * size + x) * 4;
        canvas[idx] = 255;     // R
        canvas[idx + 1] = 255; // G
        canvas[idx + 2] = 255; // B
        canvas[idx + 3] = 220; // A
      }
    }
  }

  const icon = nativeImage.createFromBuffer(canvas, { width: size, height: size });
  tray = new Tray(icon);
  tray.setToolTip('VolumeFlow - Audio Mixer');

  const contextMenu = Menu.buildFromTemplate([
    { label: 'VolumeFlow v1.6.0', enabled: false },
    { type: 'separator' },
    {
      label: 'Pokaż VolumeFlow',
      click: () => {
        if (mainWindow) {
          mainWindow.show();
          mainWindow.focus();
        }
      }
    },
    { type: 'separator' },
    {
      label: 'Zamknij',
      click: () => {
        isQuitting = true;
        stopBridge();
        if (tray) {
          tray.destroy();
          tray = null;
        }
        if (mainWindow) {
          mainWindow.destroy();
        }
        app.quit();
      }
    }
  ]);

  tray.setContextMenu(contextMenu);

  tray.on('click', () => {
    if (mainWindow) {
      if (mainWindow.isVisible()) {
        mainWindow.hide();
      } else {
        mainWindow.show();
        mainWindow.focus();
      }
    }
  });
}

// ============================================================
// IPC Handlers
// ============================================================

ipcMain.on('close-app', () => {
  isQuitting = true;
  stopBridge();
  if (tray) {
    tray.destroy();
    tray = null;
  }
  if (mainWindow) {
    mainWindow.destroy();
  }
  app.quit();
});

ipcMain.on('minimize-to-tray', () => {
  if (mainWindow) {
    mainWindow.hide();
  }
});

ipcMain.handle('get-app-icon', async (event, filePath) => {
  try {
    if (!filePath || !fs.existsSync(filePath)) return null;
    
    return await Promise.race([
      app.getFileIcon(filePath, { size: 'normal' }).then(icon => icon.toDataURL()),
      new Promise((_, reject) => setTimeout(() => reject(new Error('Icon timeout')), 3000))
    ]);
  } catch (err) {
    console.error('Error fetching icon:', err);
    return null;
  }
});

ipcMain.on('set-window-size', (event, { width, height }) => {
  const win = BrowserWindow.fromWebContents(event.sender);
  if (win && Number.isFinite(width) && Number.isFinite(height)) {
    const safeWidth = Math.max(200, Math.min(1920, width));
    const safeHeight = Math.max(200, Math.min(1080, height));
    win.setSize(safeWidth, safeHeight, true);
    win.center();
  }
});
ipcMain.on('set-ducking', (event, data) => {
  sendBridgeCommand({ action: 'set_ducking', ...data });
});

ipcMain.on('set-boost', (event, data) => {
  // Sync shared state so hotkey toggle stays in sync with UI
  if (data.active !== undefined) hotkeyBoostState = data.active;
  sendBridgeCommand({ action: 'set_boost', ...data });
});

ipcMain.on('start-recording', (event, { pid }) => {
  sendBridgeCommand({ action: 'start_recording', pid });
});

ipcMain.on('stop-recording', (event, { pid }) => {
  sendBridgeCommand({ action: 'stop_recording', pid });
});

ipcMain.on('show-osd', (event, { message, icon }) => {
  showOSD(message, icon);
});

ipcMain.on('open-recordings', () => {
  const recordingsPath = path.join(__dirname, 'Recordings');
  if (!fs.existsSync(recordingsPath)) {
    fs.mkdirSync(recordingsPath);
  }
  require('electron').shell.openPath(recordingsPath);
});

const userDataPath = app.getPath('userData');
const configPath = path.join(userDataPath, 'config.json');

ipcMain.handle('load-settings', () => {
  try {
    if (fs.existsSync(configPath)) {
      const data = fs.readFileSync(configPath, 'utf8');
      const cfg = JSON.parse(data);
      // Restore hotkeys from config
      if (cfg.hotkeys) {
        hotkeys = { ...hotkeys, ...cfg.hotkeys };
        registerHotkeys();
      }
      return cfg;
    }
  } catch (err) {
    console.error('Error loading settings:', err);
  }
  return null;
});

ipcMain.handle('get-hotkeys', () => {
  return hotkeys;
});

ipcMain.on('save-hotkeys', (event, newHotkeys) => {
  try {
    // Validate — only allow safe Electron accelerator strings
    const allowed = /^((Ctrl|Alt|Shift|Super)\+)+(F[1-9]|F1[0-2]|[A-Z0-9]|Space|Tab|Escape|Insert|Delete|Home|End|PageUp|PageDown)$/i;
    for (const [key, val] of Object.entries(newHotkeys)) {
      if (val && !allowed.test(val)) {
        console.warn(`[Hotkeys] Rejected invalid accelerator for ${key}: ${val}`);
        event.returnValue = { ok: false, error: `Invalid accelerator: ${val}` };
        return;
      }
    }
    hotkeys = { ...hotkeys, ...newHotkeys };
    registerHotkeys();
    // Persist alongside other settings
    let cfg = {};
    if (fs.existsSync(configPath)) {
      try { cfg = JSON.parse(fs.readFileSync(configPath, 'utf8')); } catch {}
    }
    cfg.hotkeys = hotkeys;
    fs.writeFileSync(configPath, JSON.stringify(cfg, null, 2), 'utf8');
    console.log('[Hotkeys] Saved:', hotkeys);
    event.returnValue = { ok: true };
  } catch (err) {
    console.error('[Hotkeys] Save error:', err);
    event.returnValue = { ok: false, error: err.message };
  }
});

ipcMain.on('save-settings', async (event, settings) => {
  try {
    if (settings.autoStart !== undefined) {
      app.setLoginItemSettings({
        openAtLogin: settings.autoStart,
        path: app.getPath('exe')
      });
    }
    await fs.promises.writeFile(configPath, JSON.stringify(settings, null, 2), 'utf8');
  } catch (err) {
    console.error('Error saving settings:', err);
  }
});

async function fadeToVolume(pid, targetVolume, duration = 800) {
  if (isQuitting) return;

  if (activeFades.has(pid)) {
    activeFades.get(pid).cancel();
  }

  const steps = 12;
  const interval = duration / steps;
  
  const result = await sendBridgeCommand({ action: 'get_sessions' });
  if (!result || !result.sessions || isQuitting) return;
  const session = result.sessions.find(s => s.pid === pid);
  if (!session) return;


  const startVol = session.volume;
  const diff = targetVolume - startVol;

  let isCancelled = false;
  const cancel = () => { isCancelled = true; };
  activeFades.set(pid, { cancel });

  for (let i = 1; i <= steps; i++) {
    if (isCancelled || isQuitting) break;
    
    await new Promise(r => setTimeout(r, interval));
    
    if (isCancelled || isQuitting) break;
    const current = startVol + (diff * (i / steps));
    await sendBridgeCommand({ action: 'set_volume', pid, volume: current });
  }

  if (!isCancelled) {
    activeFades.delete(pid);
  }
}

ipcMain.handle('apply-profile', async (event, profile) => {
  if (!profile || !profile.sessions || isQuitting) return false;
  
  const result = await sendBridgeCommand({ action: 'get_sessions' });
  if (!result || !result.sessions) return false;

  const fadePromises = profile.sessions.map(target => {
    const live = result.sessions.find(s => s.name.toLowerCase() === target.name.toLowerCase());
    if (live) {
      return fadeToVolume(live.pid, target.volume);
    }
    return Promise.resolve();
  });

  Promise.all(fadePromises).catch(e => console.error('Fade error:', e));

  if (profile.masterVolume !== undefined) {
    const vol = Math.min(1.0, Math.max(0.0, profile.masterVolume / 100));
    await sendBridgeCommand({ action: 'set_master_volume', volume: vol });
  }

  return true;
});

// ============================================================
// Audio IPC
// ============================================================

ipcMain.handle('get-audio-sessions', async () => {
  const result = await sendBridgeCommand({ action: 'get_sessions' });
  if (!result || !result.sessions) return [];
  
  return result.sessions.map(s => ({
    pid: s.pid,
    name: s.name || 'Unknown',
    path: s.path || '',
    volume: s.volume,
    muted: s.muted,
    id: String(s.pid)
  }));
});

ipcMain.on('toggle-session-mute', async (event, { id }) => {
  if (!id) return;
  if (id === 'master') {
    await sendBridgeCommand({ action: 'toggle_master_mute' });
    return;
  }
  const pid = parseInt(id);
  if (isNaN(pid)) return;
  await sendBridgeCommand({ action: 'toggle_mute', pid: pid });
});

ipcMain.on('set-session-volume', async (event, { id, volume }) => {
  if (!id) return;
  const pid = parseInt(id);
  if (isNaN(pid)) return;
  const vol = Math.min(1.0, Math.max(0.0, volume));
  await sendBridgeCommand({ action: 'set_volume', pid: pid, volume: vol });
});

ipcMain.handle('get-master-info', async () => {
  const result = await sendBridgeCommand({ action: 'get_master' });
  if (!result || !result.master) return { volume: 0.5, muted: false, id: '' };
  
  return {
    volume: result.master.volume,
    muted: result.master.muted,
    id: 'master'
  };
});

ipcMain.on('set-master-volume', async (event, { id, volume }) => {
  const vol = Math.min(1.0, Math.max(0.0, volume));
  await sendBridgeCommand({ action: 'set_master_volume', volume: vol });
});

// ============================================================
// App Lifecycle
// ============================================================

app.whenReady().then(() => {
  startBridge();
  createWindow();
  createOSDWindow();
  createTray();
  registerHotkeys();

  app.on('activate', () => {
    if (mainWindow) {
      mainWindow.show();
    } else {
      createWindow();
    }
  });
});

app.on('before-quit', () => {
  isQuitting = true;
  unregisterHotkeys();
  stopBridge();
});

app.on('window-all-closed', () => {
});
