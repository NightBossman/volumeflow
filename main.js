import { app, BrowserWindow, ipcMain, Tray, Menu, nativeImage } from 'electron';
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
let tray = null;
let isQuitting = false;

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
  
  // W wersji spakowanej (ASAR) plik będzie w app.asar.unpacked
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
    
    // Process complete lines
    let newlineIdx;
    while ((newlineIdx = responseBuffer.indexOf('\n')) !== -1) {
      const line = responseBuffer.substring(0, newlineIdx).trim();
      responseBuffer = responseBuffer.substring(newlineIdx + 1);
      
      if (!line) continue;
      // console.log('RAW LINE:', line); // Debug raw data
      try {
        const parsed = JSON.parse(line);
        
        // Handle "ready" signal
        if (parsed.status === 'ready') {
          bridgeReady = true;
          console.log('AudioBridge is ready');
          continue;
        }

        // Handle peak updates
        if (parsed.type === 'peaks') {
          if (mainWindow) {
            mainWindow.webContents.send('audio-peaks', parsed);
          }
          continue;
        }

        // Route response by requestId
        if (parsed.requestId && pendingRequests.has(parsed.requestId)) {
          const req = pendingRequests.get(parsed.requestId);
          pendingRequests.delete(parsed.requestId);
          clearTimeout(req.timeout);
          req.resolve(parsed);
        }
      } catch (err) {
        console.error('Bridge parse error:', err.message, 'line:', line);
        // If we can't parse a line that was a response, it will eventually timeout.
        // We could also try to resolve the oldest pending if we had no requestId,
        // but with requestId, it's safer to just let it timeout or handle explicitly.
      }
    }
  });

  bridgeProcess.stderr.on('data', (data) => {
    console.error('Bridge stderr:', data);
  });

  bridgeProcess.on('exit', (code) => {
    console.log('AudioBridge exited with code:', code);
    bridgeReady = false;
    bridgeProcess = null;
    
    // Auto-restart after 1 second (unless quitting)
    if (!isQuitting) {
      setTimeout(() => startBridge(), 1000);
    }
  });

  bridgeProcess.on('error', (err) => {
    console.error('Bridge process error:', err);
    bridgeReady = false;
  });
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
  if (bridgeProcess) {
    try {
      if (bridgeProcess.stdin.writable) {
        bridgeProcess.stdin.write('{"action":"exit"}\n');
      }
    } catch (e) { }
    
    // Fallback kill if bridge doesn't exit gracefully
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
      sandbox: false, // Required for some native APIs like getFileIcon if we want full integration, but safe here
      preload: path.join(__dirname, 'preload.js'),
      additionalArguments: [`--vf-dev=${isDev}`],
    },
  });

  // Zamiast zamykać okno, chowamy je do tray
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

function createTray() {
  // Programowe tworzenie ikony tray 16x16 (3 paski equalizera)
  const size = 16;
  const canvas = Buffer.alloc(size * size * 4); // RGBA

  // Rysowanie 3 pasków (kolumn) equalizera w kolorze białym
  const bars = [
    { x: 3, h: 8 },   // lewy pasek
    { x: 7, h: 12 },  // środkowy (najwyższy)
    { x: 11, h: 6 },  // prawy pasek
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
        canvas[idx + 3] = 220; // A (lekko przezroczysty)
      }
    }
  }

  const icon = nativeImage.createFromBuffer(canvas, { width: size, height: size });
  tray = new Tray(icon);
  tray.setToolTip('VolumeFlow - Audio Mixer');

  const contextMenu = Menu.buildFromTemplate([
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

  // Kliknięcie na ikonkę tray przywraca okno
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
    
    // Wrap with timeout to prevent hanging on slow/network drives
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

// Settings persistence
const userDataPath = app.getPath('userData');
const configPath = path.join(userDataPath, 'config.json');

ipcMain.handle('load-settings', () => {
  try {
    if (fs.existsSync(configPath)) {
      const data = fs.readFileSync(configPath, 'utf8');
      return JSON.parse(data);
    }
  } catch (err) {
    console.error('Error loading settings:', err);
  }
  return null;
});

ipcMain.on('save-settings', async (event, settings) => {
  try {
    // Handle Auto-start
    if (settings.autoStart !== undefined) {
      app.setLoginItemSettings({
        openAtLogin: settings.autoStart,
        path: app.getPath('exe')
      });
    }
    // Use async writeFile to prevent blocking main thread
    await fs.promises.writeFile(configPath, JSON.stringify(settings, null, 2), 'utf8');
  } catch (err) {
    console.error('Error saving settings:', err);
  }
});



async function fadeToVolume(pid, targetVolume, duration = 800) {
  if (isQuitting) return;

  // Cancel any existing fade for this PID
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

  // Start all fades in parallel but wait for them
  const fadePromises = profile.sessions.map(target => {
    const live = result.sessions.find(s => s.name.toLowerCase() === target.name.toLowerCase());
    if (live) {
      return fadeToVolume(live.pid, target.volume);
    }
    return Promise.resolve();
  });

  Promise.all(fadePromises).catch(e => console.error('Fade error:', e));

  // Apply master
  if (profile.masterVolume !== undefined) {
    const vol = Math.min(1.0, Math.max(0.0, profile.masterVolume / 100));
    await sendBridgeCommand({ action: 'set_master_volume', volume: vol });
  }

  return true;
});


// ============================================================
// Audio IPC - now via AudioBridge
// ============================================================

ipcMain.handle('get-audio-sessions', async () => {
  const result = await sendBridgeCommand({ action: 'get_sessions' });
  if (!result || !result.sessions) return [];
  
  return result.sessions.map(s => ({
    pid: s.pid,
    name: s.name || 'Unknown',
    path: s.path || '',
    volume: s.volume,  // Already 0.0 - 1.0 from bridge
    muted: s.muted,
    id: String(s.pid)  // Use PID as identifier for bridge commands
  }));
});

ipcMain.on('toggle-session-mute', async (event, { id }) => {
  if (!id) return;
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
    volume: result.master.volume, // Raw 0-1 for consistent internal API
    muted: result.master.muted,
    id: 'master'
  };
});

ipcMain.on('set-master-volume', async (event, { id, volume }) => {
  const vol = Math.min(1.0, Math.max(0.0, volume)); // Now expects 0-1
  await sendBridgeCommand({ action: 'set_master_volume', volume: vol });
});

// ============================================================
// App Lifecycle
// ============================================================

app.whenReady().then(() => {
  startBridge();
  createWindow();
  createTray();

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
  stopBridge();
});

app.on('window-all-closed', () => {
  // Nie zamykamy, bo aplikacja działa w tray
});
