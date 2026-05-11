const { app, BrowserWindow, ipcMain, Tray, Menu, nativeImage, globalShortcut } = require('electron');
const path = require('path');
const { spawn } = require('child_process');
const fs = require('fs');

let mainWindow;
let osdWindow;
let tray = null;
let bridgeProcess;
let bridgeReady = false;
let pendingRequests = new Map();

// Polling stats interval
let healthCheckInterval;

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 480,
    height: 700,
    frame: false,
    transparent: true,
    backgroundColor: '#00000000',
    webPreferences: {
      nodeIntegration: true,
      contextIsolation: false
    },
    icon: path.join(__dirname, '../assets/icon.png'),
    show: false
  });

  mainWindow.loadFile(path.join(__dirname, '../index.html'));

  mainWindow.once('ready-to-show', () => {
    mainWindow.show();
  });

  mainWindow.on('closed', () => {
    mainWindow = null;
  });
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

  osdWindow.loadFile(path.join(__dirname, '../osd.html'));
  osdWindow.setIgnoreMouseEvents(true);
}

function startAudioBridge() {
  const bridgePath = path.join(__dirname, '../AudioBridge.exe');
  console.log('Starting AudioBridge from:', bridgePath);

  bridgeProcess = spawn(bridgePath);

  bridgeProcess.stdout.on('data', (data) => {
    const lines = data.toString().split('\n');
    for (let line of lines) {
      line = line.trim();
      if (!line) continue;

      try {
        const response = JSON.parse(line);

        if (response.status === 'ready') {
          bridgeReady = true;
          console.log('AudioBridge is ready');
          startHealthMonitoring();
          return;
        }

        if (response.type === 'peaks') {
          if (mainWindow) {
            mainWindow.webContents.send('bridge-peaks', response);
          }
          return;
        }

        if (response.requestId && pendingRequests.has(response.requestId)) {
          const { resolve } = pendingRequests.get(response.requestId);
          pendingRequests.delete(response.requestId);
          resolve(response);
        }

      } catch (e) {
        // Silently ignore non-JSON or malformed lines (logs)
      }
    }
  });

  bridgeProcess.stderr.on('data', (data) => {
    console.error(`AudioBridge Error: ${data}`);
  });

  bridgeProcess.on('close', (code) => {
    console.log(`AudioBridge exited with code ${code}`);
    bridgeReady = false;
    stopHealthMonitoring();
  });
}

function startHealthMonitoring() {
  if (healthCheckInterval) clearInterval(healthCheckInterval);
  
  healthCheckInterval = setInterval(async () => {
    if (!bridgeReady) return;

    try {
      const stats = await sendBridgeCommand({ action: 'get_stats' });
      if (stats.status === 'ok') {
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
    if (!bridgeReady && command.action !== 'ping') {
      reject(new Error('AudioBridge not ready'));
      return;
    }

    const requestId = Math.random().toString(36).substring(7);
    const cmdWithId = { ...command, requestId };

    pendingRequests.set(requestId, { resolve, reject });

    try {
      bridgeProcess.stdin.write(JSON.stringify(cmdWithId) + '\n');
    } catch (e) {
      pendingRequests.delete(requestId);
      reject(e);
    }

    // Timeout for safety
    setTimeout(() => {
      if (pendingRequests.has(requestId)) {
        pendingRequests.delete(requestId);
        reject(new Error(`Command ${command.action} timed out`));
      }
    }, 5000);
  });
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

app.whenReady().then(() => {
  createWindow();
  createOSDWindow();
  startAudioBridge();

  const iconPath = path.join(__dirname, '../assets/tray-icon.png');
  const icon = nativeImage.createFromPath(iconPath).resize({ width: 16, height: 16 });
  tray = new Tray(icon);

  const contextMenu = Menu.buildFromTemplate([
    { label: 'Show VolumeFlow', click: () => mainWindow.show() },
    { type: 'separator' },
    { label: 'Exit', click: () => app.quit() }
  ]);

  tray.setToolTip('VolumeFlow v1.7.0');
  tray.setContextMenu(contextMenu);

  tray.on('click', () => {
    mainWindow.isVisible() ? mainWindow.hide() : mainWindow.show();
  });
});

ipcMain.on('bridge-command', async (event, command) => {
  try {
    const response = await sendBridgeCommand(command);
    event.reply('bridge-response', response);
  } catch (err) {
    event.reply('bridge-response', { status: 'error', message: err.message });
  }
});

ipcMain.on('show-osd', (event, { message, icon }) => {
  showOSD(message, icon);
});

ipcMain.on('close-app', () => {
  app.quit();
});

ipcMain.on('minimize-app', () => {
  mainWindow.minimize();
});

ipcMain.on('open-recordings', () => {
  const recordingsPath = path.join(__dirname, '../Recordings');
  if (!fs.existsSync(recordingsPath)) {
    fs.mkdirSync(recordingsPath);
  }
  require('electron').shell.openPath(recordingsPath);
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});
