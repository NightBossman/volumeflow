import { app, BrowserWindow, ipcMain } from 'electron';
import path from 'path';
import { fileURLToPath } from 'url';
import { execFile } from 'child_process';
import fs from 'fs';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const isDev = process.env.NODE_ENV === 'development';

function createWindow() {
  const win = new BrowserWindow({
    width: 400,
    height: 350,
    frame: false,
    transparent: true,
    resizable: true,
    webPreferences: {
      nodeIntegration: true,
      contextIsolation: false,
      preload: path.join(__dirname, 'preload.js'),
    },
  });

  if (isDev) {
    win.loadURL('http://localhost:5173');
  } else {
    win.loadFile(path.join(__dirname, 'dist/index.html'));
  }
}

ipcMain.on('close-app', () => {
  app.quit();
});

ipcMain.on('set-window-size', (event, { width, height }) => {
  const win = BrowserWindow.fromWebContents(event.sender);
  if (win) {
    win.setSize(width, height, true);
  }
});

ipcMain.handle('get-audio-sessions', async () => {
  return new Promise((resolve) => {
    const svvPath = path.join(__dirname, 'SoundVolumeView.exe');
    const jsonPath = path.join(__dirname, 'sessions.json');
    
    execFile(svvPath, ['/sjson', jsonPath], (error) => {
      if (error) return resolve([]);

      try {
        let data = fs.readFileSync(jsonPath, 'utf16le');
        data = data.replace(/^\uFEFF/, '');
        const sessions = JSON.parse(data);
        
        const processes = sessions
          .filter(s => s.Type === 'Application' && s.Direction === 'Render')
          .map(s => ({
            pid: parseInt(s['Process ID']) || 0,
            name: s.Name || path.basename(s['Process Path'] || 'Unknown', '.exe'),
            volume: parseFloat(s['Volume Percent']) / 100,
            id: s['Command-Line Friendly ID']
          }))
          .filter(p => p.pid !== 0);

        fs.unlink(jsonPath, () => {});
        resolve(processes);
      } catch (err) {
        resolve([]);
      }
    });
  });
});

ipcMain.on('set-session-volume', (event, { id, volume }) => {
  if (!id) return;
  const svvPath = path.join(__dirname, 'SoundVolumeView.exe');
  const volPercent = Math.min(100, Math.max(0, volume * 100)).toFixed(1);
  execFile(svvPath, ['/SetVolume', id, volPercent]);
});

ipcMain.handle('get-master-info', async () => {
  return new Promise((resolve) => {
    const svvPath = path.join(__dirname, 'SoundVolumeView.exe');
    const jsonPath = path.join(__dirname, 'master.json');
    
    execFile(svvPath, ['/sjson', jsonPath], (error) => {
      if (error) return resolve({ volume: 50, id: '' });
      try {
        let data = fs.readFileSync(jsonPath, 'utf16le');
        data = data.replace(/^\uFEFF/, '');
        const sessions = JSON.parse(data);
        const master = sessions.find(s => s.Type === 'Device' && s.Direction === 'Render' && s.Default === 'Yes');
        if (master) {
          resolve({
            volume: parseFloat(master['Volume Percent']) || 50,
            id: master['Command-Line Friendly ID']
          });
        } else {
          resolve({ volume: 50, id: '' });
        }
        fs.unlink(jsonPath, () => {});
      } catch (err) {
        resolve({ volume: 50, id: '' });
      }
    });
  });
});

ipcMain.on('set-master-volume', (event, { id, volume }) => {
  const svvPath = path.join(__dirname, 'SoundVolumeView.exe');
  const volPercent = Math.min(100, Math.max(0, volume)).toFixed(1);
  const target = id || "DefaultPlaybackDevice";
  execFile(svvPath, ['/SetVolume', target, volPercent]);
});

app.whenReady().then(createWindow);

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') app.quit();
});