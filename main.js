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

let bridgeProcess = null;

function startBridge() {
  const exePath = path.join(__dirname, 'AudioBridge.exe');
  if (!fs.existsSync(exePath)) {
    console.error('AudioBridge.exe not found at:', exePath);
    return;
  }

  bridgeProcess = spawn(exePath);

  bridgeProcess.stdout.on('data', (data) => {
    const lines = data.toString().split('\n');
    for (const line of lines) {
      if (!line.trim()) continue;
      try {
        const json = JSON.parse(line);
        if (json.type === 'peaks') {
          if (mainWindow && !mainWindow.isDestroyed()) {
            mainWindow.webContents.send('audio-peaks', json);
          }
        } else if (json.status === 'ready') {
          console.log('AudioBridge is ready');
        }
      } catch (e) {
        // console.log('Bridge Msg:', line);
      }
    }
  });

  bridgeProcess.stderr.on('data', (data) => {
    console.error('Bridge Error:', data.toString());
  });

  bridgeProcess.on('close', (code) => {
    console.log(`Bridge process exited with code ${code}`);
    if (!isQuitting) {
      setTimeout(startBridge, 2000);
    }
  });
}

function stopBridge() {
  if (bridgeProcess) {
    bridgeProcess.stdin.write(JSON.stringify({ action: 'exit' }) + '\n');
    bridgeProcess.kill();
  }
}

const createWindow = () => {
  mainWindow = new BrowserWindow({
    width: 400,
    height: 600,
    frame: false,
    transparent: true,
    backgroundColor: '#00000000',
    resizable: true,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      sandbox: false, // Required for some native APIs like getFileIcon if we want full integration, but safe here
      preload: path.join(__dirname, 'preload.cjs'),
      additionalArguments: [`--vf-dev=${isDev}`],
    },
  });

  if (isDev) {
    mainWindow.loadURL('http://localhost:5173');
    // mainWindow.webContents.openDevTools({ mode: 'detach' });
  } else {
    mainWindow.loadFile(path.join(__dirname, 'dist/index.html'));
  }

  mainWindow.on('close', (e) => {
    if (!isQuitting) {
      e.preventDefault();
      mainWindow.hide();
    }
  });
};

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
    // app.quit();
  }
});

app.on('before-quit', () => {
  isQuitting = true;
  stopBridge();
});

function createTray() {
  const iconPath = path.join(__dirname, 'public/tray_icon.png');
  const icon = nativeImage.createFromPath(iconPath);
  tray = new Tray(icon.resize({ width: 16, height: 16 }));

  const contextMenu = Menu.buildFromTemplate([
    { label: 'Otwórz VolumeFlow', click: () => mainWindow.show() },
    { type: 'separator' },
    { 
      label: 'Wyjście', 
      click: () => {
        isQuitting = true;
        stopBridge();
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

ipcMain.on('set-ducking', (event, data) => {
  if (bridgeProcess && !bridgeProcess.killed) {
    const cmd = JSON.stringify({
      action: 'set_ducking',
      ...data
    });
    bridgeProcess.stdin.write(cmd + '\n');
  }
});

// Settings persistence
const userDataPath = app.getPath('userData');
const configPath = path.join(userDataPath, 'config.json');

ipcMain.handle('load-settings', () => {
  if (fs.existsSync(configPath)) {
    try {
      return JSON.parse(fs.readFileSync(configPath, 'utf8'));
    } catch (e) {
      return null;
    }
  }
  return null;
});

ipcMain.on('save-settings', (event, settings) => {
  fs.writeFileSync(configPath, JSON.stringify(settings, null, 2));
});

ipcMain.handle('get-audio-sessions', async () => {
  return new Promise((resolve) => {
    if (!bridgeProcess || bridgeProcess.killed) return resolve([]);
    const requestId = Date.now().toString();
    
    const handler = (data) => {
      try {
        const json = JSON.parse(data.toString());
        if (json.requestId === requestId) {
          bridgeProcess.stdout.removeListener('data', handler);
          resolve(json.sessions || []);
        }
      } catch(e) {}
    };

    bridgeProcess.stdout.on('data', handler);
    bridgeProcess.stdin.write(JSON.stringify({ action: 'get_sessions', requestId }) + '\n');
    
    setTimeout(() => {
      bridgeProcess.stdout.removeListener('data', handler);
      resolve([]);
    }, 1000);
  });
});

ipcMain.on('set-session-volume', (event, { id, volume }) => {
  if (bridgeProcess && !bridgeProcess.killed) {
    bridgeProcess.stdin.write(JSON.stringify({ action: 'set_volume', pid: id, volume }) + '\n');
  }
});

ipcMain.on('set-master-volume', (event, { volume }) => {
  if (bridgeProcess && !bridgeProcess.killed) {
    bridgeProcess.stdin.write(JSON.stringify({ action: 'set_master_volume', volume }) + '\n');
  }
});

ipcMain.on('toggle-session-mute', (event, { id }) => {
  if (bridgeProcess && !bridgeProcess.killed) {
    bridgeProcess.stdin.write(JSON.stringify({ action: 'toggle_mute', pid: id }) + '\n');
  }
});

ipcMain.handle('apply-profile', async (event, profile) => {
  if (!bridgeProcess || bridgeProcess.killed) return;
  
  if (profile.masterVolume !== undefined) {
    bridgeProcess.stdin.write(JSON.stringify({ action: 'set_master_volume', volume: profile.masterVolume / 100 }) + '\n');
  }
  
  if (profile.sessions) {
    const result = await ipcRenderer.invoke('get-audio-sessions');
    if (!result || !result.sessions || isQuitting) return;
    
    for (const targetSession of profile.sessions) {
      const live = result.sessions.find(s => s.name === targetSession.name);
      if (live) {
        bridgeProcess.stdin.write(JSON.stringify({ action: 'set_volume', pid: live.pid, volume: targetSession.volume }) + '\n');
      }
    }
  }
});
