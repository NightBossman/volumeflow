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

      const master = await ipcRenderer.invoke('get-master-info');
      if (master) {
        masterVolume = Math.round(master.volume);
        masterMuted = master.muted;
        masterId = master.id;
      }
    } catch (e) {
      console.error(e);
    }
  }

  function saveSettings() {
    ipcRenderer.send('save-settings', {
      theme: currentTheme,
      eyeSaver: eyeSaver
    });
  }

  function setTheme(themeId) {
    currentTheme = themeId;
    document.body.setAttribute('data-theme', themeId);
    saveSettings();
  }

  function toggleMode() {
    isExpanded = !isExpanded;
    showAbout = false;
    const width = 400;
    const height = isExpanded ? 700 : 350;
    ipcRenderer.send('set-window-size', { width, height });
  }

  function handleVolumeChange(id, volume) {
    ipcRenderer.send('set-session-volume', { id, volume: volume / 100 });
  }

  function handleMasterChange(volume) {
    ipcRenderer.send('set-master-volume', { id: masterId, volume });
  }

  function handleMute(id) {
    ipcRenderer.send('toggle-session-mute', { id });
    loadSessions(); // Natychmiastowe odświeżenie
  }

  function hideToTray() {
    ipcRenderer.send('minimize-to-tray');
  }

  function closeApp() {
    ipcRenderer.send('close-app');
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

    loadSessions();
    const interval = setInterval(loadSessions, 2500);
    ipcRenderer.send('set-window-size', { width: 400, height: 350 });
    return () => clearInterval(interval);
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

  <section class="master-section">
    <VolumeSlider 
      label="Głośność Główna" 
      bind:value={masterVolume} 
      isMaster={true} 
      muted={masterMuted}
      onchange={() => handleMasterChange(masterVolume)}
      onmute={() => handleMute(masterId)}
    />
  </section>

  <div class="separator"></div>

  <section class="process-list">
    <div class="section-title">
      {isExpanded ? 'Aktywne Procesy' : 'Najczęstsze'}
    </div>
    
    {#each isExpanded ? processes : processes.slice(0, 2) as process}
      <VolumeSlider 
        label={process.name} 
        bind:value={process.volume} 
        muted={process.muted}
        onchange={() => handleVolumeChange(process.id, process.volume)}
        onmute={() => handleMute(process.id)}
      />
    {/each}

    {#if isExpanded}
      <div class="advanced-section">
        {#if !showAbout}
          <div class="section-title">Personalizacja</div>
          <div class="theme-row">
            <div class="theme-selector">
              {#each themes as theme}
                <button 
                  class="theme-dot" 
                  class:active={currentTheme === theme.id}
                  style="background: {theme.color}"
                  onclick={() => setTheme(theme.id)}
                ></button>
              {/each}
            </div>
            
            <button 
              class="eye-saver-toggle" 
              class:active={eyeSaver}
              onclick={() => { eyeSaver = !eyeSaver; saveSettings(); }}
            >
              {#if eyeSaver}
                <Moon size={14} /> <span>Eye Saver: ON</span>
              {:else}
                <Sun size={14} /> <span>Eye Saver: OFF</span>
              {/if}
            </button>
          </div>

          <div class="section-title" style="margin-top: 24px">Zarządzanie</div>
          <div class="advanced-options">
            <button class="advanced-btn">
              <Users size={16} /> <span>Zarządzaj Grupami</span>
            </button>
            <button class="advanced-btn" onclick={() => showAbout = true}>
              <Info size={16} /> <span>O programie</span>
            </button>
          </div>
        {:else}
          <div class="about-card">
            <div class="about-header">
              <Activity size={24} color="var(--primary-color)" />
              <h3>VolumeFlow v1.0 Alpha</h3>
            </div>
            <p>Premium Windows Audio Mixer stworzony z myślą o estetyce i wydajności.</p>
            <div class="stats">
              <div class="stat-item">
                <span class="stat-label">Technologia:</span>
                <span class="stat-val">Svelte 5 + Electron</span>
              </div>
              <div class="stat-item">
                <span class="stat-label">Status:</span>
                <span class="stat-val">Stabilny (Real Backend)</span>
              </div>
            </div>
            <div class="about-footer">
              <button class="back-btn" onclick={() => showAbout = false}>Wróć</button>
              <div class="made-with">
                Made with <Heart size={10} color="#ff4444" fill="#ff4444" /> for Users
              </div>
            </div>
          </div>
        {/if}
      </div>
    {/if}
  </section>

  <footer>
    Vibe: {themes.find(t => t.id === currentTheme).name} | 
    {eyeSaver ? 'Protection' : 'Standard'}
  </footer>
</main>

<style>
  main {
    display: flex;
    flex-direction: column;
    height: 100%;
    box-sizing: border-box;
    transition: filter 0.5s ease;
  }

  header {
    height: 44px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0 16px;
    background: rgba(255, 255, 255, 0.05);
  }

  .title-group {
    display: flex;
    align-items: center;
    gap: 8px;
  }

  .title {
    font-weight: 700;
    font-size: 0.8em;
    letter-spacing: 1px;
    opacity: 0.9;
    text-transform: uppercase;
  }

  .controls {
    display: flex;
    gap: 12px;
    align-items: center;
  }

  .icon-btn {
    background: transparent;
    border: none;
    color: white;
    padding: 4px;
    cursor: pointer;
    opacity: 0.6;
    transition: all 0.2s;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .icon-btn:hover {
    opacity: 1;
    transform: scale(1.1);
  }

  .icon-btn.close:hover {
    color: #ff4444;
  }

  .master-section {
    padding: 20px 16px;
  }

  .separator {
    height: 1px;
    background: var(--glass-border);
    margin: 0 16px;
  }

  .section-title {
    font-size: 0.65em;
    text-transform: uppercase;
    letter-spacing: 1.5px;
    color: rgba(255, 255, 255, 0.4);
    margin-bottom: 14px;
    padding-left: 4px;
    font-weight: 700;
  }

  .process-list {
    flex: 1;
    overflow-y: auto;
    padding: 16px;
  }

  .advanced-section {
    margin-top: 24px;
    padding: 20px 16px;
    background: rgba(255, 255, 255, 0.02);
    border-radius: 12px;
    border: 1px solid var(--glass-border);
    min-height: 200px;
  }

  .theme-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: 8px;
  }

  .theme-selector {
    display: flex;
    gap: 12px;
  }

  .theme-dot {
    width: 20px;
    height: 20px;
    border-radius: 50%;
    border: 2px solid transparent;
    cursor: pointer;
    transition: transform 0.2s;
  }

  .theme-dot.active {
    border-color: white;
    box-shadow: 0 0 10px rgba(255, 255, 255, 0.3);
  }

  .eye-saver-toggle {
    background: rgba(255, 255, 255, 0.05);
    border: 1px solid var(--glass-border);
    color: white;
    padding: 8px 14px;
    border-radius: 20px;
    font-size: 0.7em;
    cursor: pointer;
    transition: all 0.2s;
    font-weight: 600;
    display: flex;
    align-items: center;
    gap: 6px;
  }

  .eye-saver-toggle.active {
    background: #ffcc00;
    color: black;
    border-color: #ffcc00;
  }

  .advanced-options {
    margin-top: 12px;
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  .advanced-btn {
    background: rgba(255, 255, 255, 0.05);
    border: 1px solid var(--glass-border);
    color: white;
    padding: 12px;
    border-radius: 8px;
    font-size: 0.8em;
    cursor: pointer;
    transition: all 0.2s;
    display: flex;
    align-items: center;
    gap: 10px;
  }

  .advanced-btn:hover {
    background: var(--glass-border);
    padding-left: 16px;
  }

  /* About Card Styles */
  .about-card {
    display: flex;
    flex-direction: column;
    gap: 12px;
    animation: fadeIn 0.3s ease;
  }

  @keyframes fadeIn {
    from { opacity: 0; transform: translateY(10px); }
    to { opacity: 1; transform: translateY(0); }
  }

  .about-header {
    display: flex;
    align-items: center;
    gap: 12px;
  }

  .about-header h3 {
    margin: 0;
    font-size: 1em;
    color: var(--primary-color);
  }

  .about-card p {
    font-size: 0.8em;
    color: rgba(255, 255, 255, 0.6);
    line-height: 1.5;
    margin: 0;
  }

  .stats {
    display: flex;
    flex-direction: column;
    gap: 6px;
    background: rgba(255, 255, 255, 0.03);
    padding: 10px;
    border-radius: 8px;
  }

  .stat-item {
    display: flex;
    justify-content: space-between;
    font-size: 0.75em;
  }

  .stat-label { color: rgba(255, 255, 255, 0.4); }

  .about-footer {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: 12px;
  }

  .back-btn {
    background: var(--primary-color);
    border: none;
    color: white;
    padding: 6px 16px;
    border-radius: 4px;
    font-size: 0.8em;
    cursor: pointer;
  }

  .made-with {
    font-size: 0.7em;
    color: rgba(255, 255, 255, 0.3);
    display: flex;
    align-items: center;
    gap: 4px;
  }

  footer {
    height: 30px;
    background: rgba(0, 0, 0, 0.3);
    display: flex;
    align-items: center;
    padding: 0 16px;
    font-size: 0.7em;
    color: rgba(255, 255, 255, 0.3);
    letter-spacing: 0.5px;
  }
</style>