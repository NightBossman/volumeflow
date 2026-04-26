<script>
  import './app.css';
  import VolumeSlider from './lib/VolumeSlider.svelte';
  import { onMount } from 'svelte';
  import { 
    X, 
    Maximize2, 
    Minimize2, 
    Minus,
    Sun, 
    Moon, 
    Users, 
    Settings,
    Activity,
    Info,
    Heart,
    Power
  } from 'lucide-svelte';

  const { ipcRenderer } = window.require('electron');

  let isExpanded = false;
  let showAbout = false;
  let currentTheme = 'midnight';
  let eyeSaver = false;
  let masterVolume = 75;
  let masterMuted = false;
  let masterId = '';
  let processes = [];
  let iconCache = new Map();

  const themes = [
    { id: 'midnight', name: 'Midnight', color: '#0078d4' },
    { id: 'solar', name: 'Solar', color: '#ff4d00' },
    { id: 'matrix', name: 'Matrix', color: '#00ff41' },
    { id: 'frost', name: 'Frost', color: '#00f2ff' }
  ];

  async function loadSessions() {
    try {
      const liveProcesses = await ipcRenderer.invoke('get-audio-sessions');
      if (liveProcesses && liveProcesses.length > 0) {
        processes = liveProcesses.map(p => ({ ...p, volume: Math.round(p.volume * 100) }));
      }

      for (const process of processes) {
        if (process.path && !iconCache.has(process.path)) {
          iconCache.set(process.path, 'loading'); 
          ipcRenderer.invoke('get-app-icon', process.path).then(iconData => {
            if (iconData) {
              iconCache.set(process.path, iconData);
              processes = [...processes];
            } else {
              iconCache.set(process.path, null);
            }
          });
        }
      }

      const master = await ipcRenderer.invoke('get-master-info');
      if (master) {
        masterVolume = Math.round(master.volume);
        masterMuted = master.muted;
        masterId = master.id;
      }
    } catch (e) {
      console.error('Failed to load sessions:', e);
    }
  }

  function handleVolumeChange(id, volume) {
    if (id === 'master') {
      ipcRenderer.send('set-master-volume', { id: masterId, volume });
    } else {
      ipcRenderer.send('set-session-volume', { id, volume: volume / 100 });
    }
  }

  function handleMute(id) {
    ipcRenderer.send('toggle-session-mute', { id });
    loadSessions();
  }

  function hideToTray() {
    ipcRenderer.send('minimize-to-tray');
  }

  function closeApp() {
    ipcRenderer.send('close-app');
  }

  function setTheme(theme) {
    currentTheme = theme;
    document.body.setAttribute('data-theme', theme);
    saveSettings();
  }

  function toggleEyeSaver() {
    eyeSaver = !eyeSaver;
    saveSettings();
  }

  function saveSettings() {
    ipcRenderer.send('save-settings', {
      theme: currentTheme,
      eyeSaver: eyeSaver
    });
  }

  function toggleMode() {
    isExpanded = !isExpanded;
    if (isExpanded) {
      ipcRenderer.send('set-window-size', { width: 400, height: 600 });
    } else {
      ipcRenderer.send('set-window-size', { width: 400, height: 350 });
    }
  }

  onMount(async () => {
    try {
      const savedSettings = await ipcRenderer.invoke('load-settings');
      if (savedSettings) {
        if (savedSettings.theme) {
          currentTheme = savedSettings.theme;
          document.body.setAttribute('data-theme', currentTheme);
        }
        if (savedSettings.eyeSaver !== undefined) {
          eyeSaver = savedSettings.eyeSaver;
        }
      } else {
        document.body.setAttribute('data-theme', currentTheme);
      }
    } catch (e) {
      console.error('Failed to load settings:', e);
    }

    let isRunning = true;
    
    async function pollSessions() {
      if (!isRunning) return;
      await loadSessions();
      if (isRunning) {
        setTimeout(pollSessions, 2500);
      }
    }
    
    pollSessions();
    ipcRenderer.send('set-window-size', { width: 400, height: 350 });
    
    return () => {
      isRunning = false;
    };
  });
</script>

<main class:eye-saver={eyeSaver}>
  <header class="draggable">
    <div class="title-group">
      <Activity size={14} color="var(--primary-color)" />
      <div class="title">VolumeFlow</div>
    </div>
    <div class="controls no-drag">
      <button onclick={toggleMode} class="icon-btn" title="Tryb">
        {#if isExpanded}
          <Minimize2 size={18} />
        {:else}
          <Maximize2 size={18} />
        {/if}
      </button>
      <button onclick={hideToTray} class="icon-btn" title="Schowaj do zasobnika">
        <Minus size={18} />
      </button>
      <button onclick={closeApp} class="icon-btn close" title="Zamknij">
        <X size={20} />
      </button>
    </div>
  </header>

  <div class="content">
    <VolumeSlider 
      label="Głośność Ogólna" 
      bind:value={masterVolume} 
      isMaster={true} 
      muted={masterMuted}
      onchange={() => handleVolumeChange('master', masterVolume)}
      onmute={() => handleMute(masterId)}
    />

    <div class="separator">
      <span>Aplikacje</span>
    </div>

    {#each processes as process}
      <VolumeSlider
        label={process.name}
        bind:value={process.volume}
        muted={process.muted}
        onchange={() => handleVolumeChange(process.id, process.volume)}
        onmute={() => handleMute(process.id)}
      >
        {#snippet icon()}
          <div class="icon-container">
            {#if process.path && iconCache.has(process.path) && iconCache.get(process.path) !== 'loading' && iconCache.get(process.path) !== null}
              <img src={iconCache.get(process.path)} alt="" class="app-icon" />
            {:else}
              <Activity size={16} />
            {/if}
          </div>
        {/snippet}
      </VolumeSlider>
    {/each}

    {#if isExpanded}
      <div class="expandable-section" in:fade>
        <div class="section-title">
          <Settings size={16} />
          <span>Personalizacja</span>
        </div>
        
        <div class="theme-grid">
          {#each themes as theme}
            <button 
              class="theme-card" 
              class:active={currentTheme === theme.id}
              onclick={() => setTheme(theme.id)}
            >
              <div class="color-preview" style="background: {theme.color}"></div>
              <span>{theme.name}</span>
            </button>
          {/each}
        </div>

        <div class="settings-list">
          <div class="setting-item">
            <div class="setting-info">
              <Sun size={18} />
              <span>Eye Saver Mode</span>
            </div>
            <button 
              class="toggle" 
              class:active={eyeSaver}
              onclick={toggleEyeSaver}
            ></button>
          </div>
        </div>

        <div class="about-card">
          <div class="about-header" onclick={() => showAbout = !showAbout}>
            <div class="about-title">
              <Info size={16} />
              <span>O programie VolumeFlow</span>
            </div>
            <div class="chevron" class:open={showAbout}></div>
          </div>
          
          {#if showAbout}
            <div class="about-content" in:fade>
              <p>VolumeFlow to zaawansowany mikser audio zaprojektowany dla użytkowników ceniących estetykę i precyzję.</p>
              <div class="stats">
                <div class="stat">
                  <span class="stat-label">Wersja</span>
                  <span class="stat-value">1.0.0 Alpha</span>
                </div>
                <div class="stat">
                  <span class="stat-label">Status</span>
                  <span class="stat-value pulse">Pre-Release</span>
                </div>
              </div>
              <div class="footer-note">
                Wykonano z <Heart size={12} color="#ff4444" /> dla audiofili
              </div>
            </div>
          {/if}
        </div>
      </div>
    {/if}
  </div>
</main>

<style>
  :root {
    --primary-color: #0078d4;
    --accent-color: #00bcf2;
    --bg-main: rgba(20, 20, 20, 0.85);
    --bg-card: rgba(40, 40, 40, 0.5);
    --text-main: #ffffff;
    --text-secondary: #cccccc;
    --transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  }

  :global(body[data-theme='midnight']) {
    --primary-color: #0078d4;
    --accent-color: #2b88d8;
  }

  :global(body[data-theme='solar']) {
    --primary-color: #ff4d00;
    --accent-color: #ff8c00;
  }

  :global(body[data-theme='matrix']) {
    --primary-color: #00ff41;
    --accent-color: #008f11;
  }

  :global(body[data-theme='frost']) {
    --primary-color: #00f2ff;
    --accent-color: #70faff;
  }

  :global(body) {
    margin: 0;
    font-family: 'Inter', system-ui, -apple-system, sans-serif;
    color: var(--text-main);
    overflow: hidden;
    background: transparent;
  }

  main {
    width: 100vw;
    height: 100vh;
    background: var(--bg-main);
    backdrop-filter: blur(25px);
    border: 1px solid rgba(255, 255, 255, 0.1);
    display: flex;
    flex-direction: column;
    transition: var(--transition);
  }

  main.eye-saver {
    filter: sepia(0.5) contrast(0.9);
  }

  header {
    height: 45px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0 15px;
    background: rgba(0, 0, 0, 0.2);
    border-bottom: 1px solid rgba(255, 255, 255, 0.05);
  }

  .draggable {
    -webkit-app-region: drag;
  }

  .no-drag {
    -webkit-app-region: no-drag;
  }

  .title-group {
    display: flex;
    align-items: center;
    gap: 10px;
  }

  .title {
    font-size: 0.85em;
    font-weight: 600;
    letter-spacing: 0.5px;
    text-transform: uppercase;
    color: var(--text-secondary);
  }

  .controls {
    display: flex;
    gap: 12px;
    align-items: center;
  }

  .icon-btn {
    background: transparent;
    border: none;
    color: var(--text-secondary);
    cursor: pointer;
    padding: 5px;
    border-radius: 4px;
    transition: all 0.2s;
    display: flex;
    align-items: center;
  }

  .icon-btn:hover {
    background: rgba(255, 255, 255, 0.1);
    color: #fff;
  }

  .icon-btn.close:hover {
    background: #ff4444;
  }

  .content {
    flex: 1;
    padding: 20px;
    overflow-y: auto;
  }

  .separator {
    margin: 25px 0 15px;
    display: flex;
    align-items: center;
    gap: 10px;
  }

  .separator::after {
    content: '';
    flex: 1;
    height: 1px;
    background: rgba(255, 255, 255, 0.1);
  }

  .separator span {
    font-size: 0.75em;
    font-weight: 700;
    text-transform: uppercase;
    color: var(--text-secondary);
    letter-spacing: 1px;
  }

  .expandable-section {
    margin-top: 30px;
    padding-top: 20px;
    border-top: 1px solid rgba(255, 255, 255, 0.05);
  }

  .section-title {
    display: flex;
    align-items: center;
    gap: 10px;
    margin-bottom: 15px;
    color: var(--text-secondary);
    font-size: 0.9em;
    font-weight: 600;
  }

  .theme-grid {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 10px;
    margin-bottom: 25px;
  }

  .theme-card {
    background: var(--bg-card);
    border: 1px solid rgba(255, 255, 255, 0.05);
    border-radius: 8px;
    padding: 10px;
    display: flex;
    align-items: center;
    gap: 12px;
    cursor: pointer;
    transition: all 0.2s;
    color: var(--text-main);
    text-align: left;
  }

  .theme-card:hover {
    background: rgba(255, 255, 255, 0.08);
    transform: translateY(-2px);
  }

  .theme-card.active {
    border-color: var(--primary-color);
    background: rgba(var(--primary-color), 0.1);
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.3);
  }

  .color-preview {
    width: 14px;
    height: 14px;
    border-radius: 50%;
    box-shadow: 0 0 10px rgba(0, 0, 0, 0.5);
  }

  .settings-list {
    display: flex;
    flex-direction: column;
    gap: 10px;
    margin-bottom: 25px;
  }

  .setting-item {
    background: var(--bg-card);
    padding: 12px 15px;
    border-radius: 8px;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .setting-info {
    display: flex;
    align-items: center;
    gap: 15px;
    font-size: 0.9em;
  }

  .toggle {
    width: 40px;
    height: 20px;
    background: #333;
    border-radius: 10px;
    position: relative;
    cursor: pointer;
    border: none;
    transition: background 0.3s;
  }

  .toggle::after {
    content: '';
    position: absolute;
    width: 16px;
    height: 16px;
    background: #fff;
    border-radius: 50%;
    top: 2px;
    left: 2px;
    transition: transform 0.3s;
  }

  .toggle.active {
    background: var(--primary-color);
  }

  .toggle.active::after {
    transform: translateX(20px);
  }

  .about-card {
    background: linear-gradient(135deg, rgba(255,255,255,0.05) 0%, rgba(255,255,255,0.01) 100%);
    border: 1px solid rgba(255, 255, 255, 0.05);
    border-radius: 12px;
    overflow: hidden;
  }

  .about-header {
    padding: 15px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    cursor: pointer;
    transition: background 0.2s;
  }

  .about-header:hover {
    background: rgba(255, 255, 255, 0.03);
  }

  .about-title {
    display: flex;
    align-items: center;
    gap: 10px;
    font-weight: 600;
    font-size: 0.85em;
    color: var(--text-secondary);
  }

  .chevron {
    width: 8px;
    height: 8px;
    border-right: 2px solid var(--text-secondary);
    border-bottom: 2px solid var(--text-secondary);
    transform: rotate(45deg);
    transition: transform 0.3s;
  }

  .chevron.open {
    transform: rotate(-135deg);
  }

  .about-content {
    padding: 0 15px 15px;
    font-size: 0.85em;
    color: var(--text-secondary);
    line-height: 1.6;
  }

  .stats {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 10px;
    margin: 15px 0;
  }

  .stat {
    background: rgba(0, 0, 0, 0.2);
    padding: 8px;
    border-radius: 6px;
    display: flex;
    flex-direction: column;
  }

  .stat-label {
    font-size: 0.7em;
    text-transform: uppercase;
    opacity: 0.5;
  }

  .stat-value {
    font-weight: bold;
    color: var(--primary-color);
  }

  .stat-value.pulse {
    animation: pulse 2s infinite;
  }

  .footer-note {
    text-align: center;
    font-size: 0.8em;
    opacity: 0.4;
    margin-top: 10px;
  }

  @keyframes pulse {
    0% { opacity: 0.6; }
    50% { opacity: 1; }
    100% { opacity: 0.6; }
  }

  @keyframes fade {
    from { opacity: 0; transform: translateY(10px); }
    to { opacity: 1; transform: translateY(0); }
  }

  .icon-container {
    width: 18px;
    height: 18px;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
  }

  .app-icon {
    width: 100%;
    height: 100%;
    object-fit: contain;
    filter: drop-shadow(0 0 2px rgba(0,0,0,0.3));
  }

  .about-header {
    display: flex;
    align-items: center;
    gap: 10px;
  }
</style>
