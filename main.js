import { app, BrowserWindow, ipcMain } from 'electron';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const isDev = process.env.NODE_ENV === 'development';

function createWindow() {
  const win = new BrowserWindow({
    width: 400,
    height: 600,
    frame: false,
    transparent: true,
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

// IPC Handlers
ipcMain.on('close-app', () => {
  app.quit();
});

ipcMain.on('set-window-size', (event, { width, height }) => {
  const win = BrowserWindow.fromWebContents(event.sender);
  if (win) {
    win.setSize(width, height, true);
    win.center(); // Optional: keep it centered
  }
});

const { execFile } = require('child_process');
const fs = require('fs');

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

ipcMain.on('save-settings', (event, settings) => {
  try {
    fs.writeFileSync(configPath, JSON.stringify(settings, null, 2), 'utf8');
  } catch (err) {
    console.error('Error saving settings:', err);
  }
});

ipcMain.handle('get-audio-sessions', async () => {
  return new Promise((resolve, reject) => {
    const svvPath = path.join(__dirname, 'SoundVolumeView.exe');
    const jsonPath = path.join(__dirname, 'sessions.json');
    
    execFile(svvPath, ['/sjson', jsonPath], (error) => {
      if (error) {
        console.error('Error executing SoundVolumeView:', error);
        return resolve([]); // W przypadku błędu zwróć pustą listę zamiast wywalać appkę
      }

      try {
        let data = fs.readFileSync(jsonPath, 'utf16le');
        data = data.replace(/^\uFEFF/, ''); // Usunięcie BOM
        const sessions = JSON.parse(data);
        
        const processes = sessions
          .filter(s => s.Type === 'Application' && s.Direction === 'Render') // Tylko aplikacje odtwarzające dźwięk
          .map(s => {
            let name = s.Name || "Unknown";
            // Jeśli nazwa jest pusta, użyj nazwy pliku wykonywalnego
            if (!name && s['Process Path']) {
               name = path.basename(s['Process Path'], '.exe');
            }
            // Zbudowanie obiektu
            return {
              pid: parseInt(s['Process ID']) || 0,
              name: name,
              volume: parseFloat(s['Volume Percent']) / 100,
              muted: s.Muted === 'Yes',
              id: s['Command-Line Friendly ID']
            };
          })
          .filter(p => p.pid !== 0); // Odrzucenie błędnych wpisów

        // Usunięcie pliku tymczasowego
        fs.unlink(jsonPath, () => {});
        
        resolve(processes);
      } catch (err) {
        console.error('Error reading/parsing sessions.json:', err);
        resolve([]);
      }
    });
  });
});

ipcMain.on('toggle-session-mute', (event, { id }) => {
  if (!id) return;
  const svvPath = path.join(__dirname, 'SoundVolumeView.exe');
  execFile(svvPath, ['/SwitchMute', id], (error) => {
    if (error) console.error('Error toggling mute:', error);
  });
});

ipcMain.on('set-session-volume', (event, { id, volume }) => {
  if (!id) return;
  const svvPath = path.join(__dirname, 'SoundVolumeView.exe');
  // Obliczenie procentów: np. 0.5 -> 50
  const volPercent = Math.min(100, Math.max(0, volume * 100)).toFixed(1);
  console.log(`Setting volume for ${id} to ${volPercent}`);
  
  execFile(svvPath, ['/SetVolume', id, volPercent], (error) => {
    if (error) console.error('Error setting volume:', error);
  });
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
        
        // Szukamy domyślnego urządzenia Render
        const master = sessions.find(s => s.Type === 'Device' && s.Direction === 'Render' && s.Default === 'Yes');
        
        if (master) {
          resolve({
            volume: parseFloat(master['Volume Percent']) || 50,
            muted: master.Muted === 'Yes',
            id: master['Command-Line Friendly ID']
          });
        } else {
          resolve({ volume: 50, muted: false, id: '' });
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
  
  // Jeśli mamy ID urządzenia, używamy go, w przeciwnym razie domyślne
  const target = id || "DefaultPlaybackDevice";
  execFile(svvPath, ['/SetVolume', target, volPercent], (error) => {
    if (error) console.error('Error setting master volume:', error);
  });
});

app.whenReady().then(() => {
  createWindow();

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) createWindow();
  });
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') app.quit();
});