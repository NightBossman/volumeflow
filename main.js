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
    console.error('AudioBridge Error:', data);
  });

  bridgeProcess.on('close', (code) => {
    console.log(`AudioBridge process exited with code ${code}`);
    bridgeReady = false;
    if (!isQuitting) {
      console.log('Attempting to restart bridge...');
      setTimeout(startBridge, 2000);
    }
  });
}

function sendBridgeCommand(command) {
  return new Promise((resolve, reject) => {
    if (!bridgeReady || !bridgeProcess) {
      return reject(new Error('Bridge not ready'));
    }

    const requestId = `req_${++requestCounter}`;
    const payload = JSON.stringify({ ...command, requestId }) + '\n';

    const timeout = setTimeout(() => {
      if (pendingRequests.has(requestId)) {
        pendingRequests.delete(requestId);
        reject(new Error(`Command timeout: ${command.action}`));
      }
    }, 5000);

    pendingRequests.set(requestId, { resolve, reject, timeout });
    bridgeProcess.stdin.write(payload);
  });
}

// ============================================================
// App Lifecycle
// ============================================================

function createTray() {
  const iconPath = path.join(__dirname, 'icon.png');
  const icon = nativeImage.createFromPath(iconPath);
  tray = new Tray(icon.resize({ width: 16, height: 16 }));
  
  const contextMenu = Menu.buildFromTemplate([
    { label: 'VolumeFlow v1.5.1', enabled: false },
    { type: 'separator' },
    { label: 'Pokaż Mixer', click: () => mainWindow.show() },
    { label: 'Ustawienia', click: () => {
      mainWindow.show();
      // Można dodać event do frontendu żeby otworzył zakładkę ustawień
    }},
    { type: 'separator' },
    { label: 'Zamknij', click: () => {
      isQuitting = true;
      app.quit();
    }}
  ]);

  tray.setToolTip('VolumeFlow - Premium Audio Mixer');
  tray.setContextMenu(contextMenu);
  
  tray.on('double-click', () => {
    mainWindow.show();
  });
}

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 400,
    height: 350,
    frame: false,
    transparent: true,
    resizable: false,
    backgroundColor: '#00000000',
    webPreferences: {
      preload: path.join(__dirname, 'preload.cjs'),
      nodeIntegration: false,
      contextIsolation: true,
      devTools: isDev,
    },
    show: false,
  });

  if (isDev) {
    mainWindow.loadURL('http://localhost:5173');
    // mainWindow.webContents.openDevTools({ mode: 'detach' });
  } else {
    mainWindow.loadFile(path.join(__dirname, 'dist/index.html'));
  }

  mainWindow.once('ready-to-show', () => {
    mainWindow.show();
  });

  mainWindow.on('close', (e) => {
    if (!isQuitting) {
      e.preventDefault();
      mainWindow.hide();
    }
  });
}

app.whenReady().then(() => {
  createWindow();
  createTray();
  startBridge();

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) createWindow();
  });
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    // We keep app running in tray by default
  }
});

app.on('before-quit', () => {
  isQuitting = true;
  if (bridgeProcess) {
    bridgeProcess.kill();
  }
});

// ============================================================
// IPC Handlers
// ============================================================

ipcMain.on('close-app', () => {
  isQuitting = true;
  app.quit();
});

ipcMain.on('minimize-to-tray', () => {
  if (mainWindow) mainWindow.hide();
});

ipcMain.on('toggle-session-mute', (event, { id }) => {
  sendBridgeCommand({ action: 'toggle_mute', pid: parseInt(id) });
});

ipcMain.on('set-session-volume', (event, { id, volume }) => {
  sendBridgeCommand({ action: 'set_volume', pid: parseInt(id), volume });
});

ipcMain.on('set-master-volume', (event, { volume }) => {
  sendBridgeCommand({ action: 'set_master_volume', volume });
});

ipcMain.handle('get-audio-sessions', async () => {
  const result = await sendBridgeCommand({ action: 'get_sessions' });
  return result ? result.sessions : [];
});

ipcMain.handle('get-master-info', async () => {
  const result = await sendBridgeCommand({ action: 'get_master' });
  if (result && result.master) {
    return { ...result.master, id: 'master' };
  }
  return null;
});

ipcMain.handle('apply-profile', async (event, profile) => {
  if (!profile || !profile.sessions) return;
  
  // Apply Master
  await sendBridgeCommand({ action: 'set_master_volume', volume: profile.masterVolume / 100 });

  // Apply sessions with a small delay between them to prevent bridge congestion
  for (const session of profile.sessions) {
    const result = await sendBridgeCommand({ action: 'get_sessions' });
    const live = result.sessions.find(s => s.name === session.name);
    if (live) {
      await sendBridgeCommand({ action: 'set_volume', pid: live.pid, volume: session.volume });
    }
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

ipcMain.on('set-ducking', (event, data) => {
  sendBridgeCommand({ action: 'set_ducking', ...data });
});

ipcMain.on('set-boost', (event, data) => {
  sendBridgeCommand({ action: 'set_boost', ...data });
});

ipcMain.on('start-recording', (event, { pid }) => {
  sendBridgeCommand({ action: 'start_recording', pid });
});

ipcMain.on('stop-recording', (event, { pid }) => {
  sendBridgeCommand({ action: 'stop_recording', pid });
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
