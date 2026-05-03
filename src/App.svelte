<script>
  import './app.css';
  import VolumeSlider from './lib/VolumeSlider.svelte';
  import { onMount, onDestroy } from 'svelte';
  import { 
    X, 
    Maximize2, 
    Minimize2, 
    Minus, 
    Settings, 
    Info, 
    Moon, 
    Sun,
    Activity,
    Power
  } from 'lucide-svelte';

  const { ipcRenderer } = window.electron;

  let isExpanded = $state(false);
  let showAbout = $state(false);
  let showSettings = $state(false);
  let masterVolume = $state(50);
  let masterMuted = $state(false);
  let masterId = $state('');
  let processes = $state([]);
  let peaks = $state({});
  let masterPeak = $state(0);
  let currentTheme = $state('midnight');
  let eyeSaver = $state(false);
  let autoStart = $state(false);
  let profiles = $state([]);
  let iconCache = new Map();

  const themes = [
    { id: 'midnight', name: 'Midnight Deep', color: '#1a1a2e' },
    { id: 'solar', name: 'Solar Flare', color: '#2b1b17' },
    { id: 'matrix', name: 'Matrix Digital', color: '#000d00' },
    { id: 'frost', name: 'Arctic Frost', color: '#001a1a' },
    { id: 'cyberpunk', name: 'Night City', color: '#0d0d0d' }
  ];

  async function loadSessions() {
    try {
      const result = await ipcRenderer.invoke('get-audio-sessions');
      processes = result;
      
      // Load icons
      for (const p of processes) {
        if (p.path && !iconCache.has(p.path)) {
          iconCache.set(p.path, 'loading');
          ipcRenderer.invoke('get-app-icon', p.path).then(icon => {
            if (icon) iconCache.set(p.path, icon);
          });
        }
      }

      const info = await ipcRenderer.invoke('get-master-info');
      if (info) {
        masterVolume = Math.round(info.volume * 100);
        masterMuted = info.muted;
        masterId = info.id;
      }
    } catch (e) {
      console.error(e);
    }
  }

  function handleVolumeChange(id, volume) {
    ipcRenderer.send('set-session-volume', { id, volume: volume / 100 });
  }

  function handleMasterChange() {
    ipcRenderer.send('set-master-volume', { id: 'master', volume: masterVolume / 100 });
  }

  function handleMute(id) {
    ipcRenderer.send('toggle-session-mute', { id });
    setTimeout(loadSessions, 100);
  }

  function toggleMode() {
    isExpanded = !isExpanded;
    const width = 400;
    const height = isExpanded ? 600 : 350;
    ipcRenderer.send('set-window-size', { width, height });
  }

  function hideToTray() {
    ipcRenderer.send('minimize-to-tray');
  }

  function closeApp() {
    ipcRenderer.send('close-app');
  }

  function handlePeaks(data) {
    if (!data) return;
    masterPeak = data.master || 0;
    if (data.sessions) {
      peaks = data.sessions;
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
        if (savedSettings.autoStart !== undefined) {
          autoStart = savedSettings.autoStart;
        }
        if (savedSettings.profiles) {
          profiles = savedSettings.profiles;
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
    
    ipcRenderer.on('audio-peaks', handlePeaks);

    pollSessions();
    ipcRenderer.send('set-window-size', { width: 400, height: 600 });
    
    return () => {
      isRunning = false;
    };
  });

  function setTheme(id) {
    currentTheme = id;
    document.body.setAttribute('data-theme', id);
    saveSettings();
  }

  function saveSettings() {
    ipcRenderer.send('save-settings', {
      theme: currentTheme,
      eyeSaver,
      autoStart,
      profiles
    });
  }

  function applyProfile(profile) {
    ipcRenderer.invoke('apply-profile', profile);
  }

  function deleteProfile(id) {
    profiles = profiles.filter(p => p.id !== id);
    saveSettings();
  }

  function addProfile() {
    const name = prompt("Nazwa profilu:");
    if (!name) return;
    
    const newProfile = {
      id: Date.now().toString(),
      name,
      masterVolume,
      sessions: processes.map(p => ({
        name: p.name,
        volume: p.volume
      }))
    };
    
    profiles = [...profiles, newProfile];
    saveSettings();
  }
</script>

<main>
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

  <div class="view-container" class:eye-saver={eyeSaver}>
    <section class="master-section">
      <VolumeSlider 
        label="Głośność Główna" 
        bind:value={masterVolume} 
        isMaster={true} 
        muted={masterMuted}
        peak={masterPeak}
        onchange={handleMasterChange}
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
          peak={peaks[process.pid] || 0}
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
                {#if eyeSaver}
                  <Moon size={14} /> <span>Eye Saver: ON</span>
                {:else}
                  <Sun size={14} /> <span>Eye Saver: OFF</span>
                {/if}
              </button>
            </div>

            <div class="section-title" style="margin-top: 24px">Tryb Scen (Profile)</div>
          <div class="profiles-container">
            <div class="profiles-list">
              {#each profiles as profile}
                <div class="profile-item">
                  <button class="profile-btn" onclick={() => applyProfile(profile)}>
                    {profile.name}
                  </button>
                  <button class="profile-delete" onclick={() => deleteProfile(profile.id)}>
                    <X size={12} />
                  </button>
                </div>
              {/each}
              <button class="add-profile-btn" onclick={addProfile}>
                + Nowy Profil
              </button>
            </div>
          </div>
          {/if}

          <div class="footer-nav">
            <button class="nav-btn" class:active={showAbout} onclick={() => { showAbout = !showAbout; showSettings = false; }}>
              <Info size={16} /> O programie
            </button>
            <button class="nav-btn" class:active={showSettings} onclick={() => { showSettings = !showSettings; showAbout = false; }}>
              <Settings size={16} /> Ustawienia
            </button>
          </div>

          {#if showAbout}
            <div class="about-card">
              <div class="about-logo">
                <Activity size={32} color="var(--primary-color)" />
              </div>
              <h3>VolumeFlow V1.5.0</h3>
              <p>Premium Audio Mixer for Windows</p>
              <div class="about-details">
                <span>Created by NightBosman</span>
                <span>Powered by Svelte \u0026 Electron</span>
              </div>
            </div>
          {/if}

          {#if showSettings}
            <div class="settings-card">
              <div class="setting-item">
                <div class="setting-info">
                  <div class="setting-label">Uruchamiaj przy starcie</div>
                  <div class="setting-desc">Włącz VolumeFlow przy logowaniu do Windows</div>
                </div>
                <button 
                  class="toggle-btn" 
                  class:active={autoStart}
                  onclick={() => { autoStart = !autoStart; saveSettings(); }}
                >
                  {autoStart ? 'ON' : 'OFF'}
                </button>
              </div>
            </div>
          {/if}
        </div>
      {/if}
    </section>
  </div>
</main>

<style>
  :global(body) {
    margin: 0;
    font-family: 'Inter', sans-serif;
    color: white;
    user-select: none;
    overflow: hidden;
    background: transparent;
  }

  :global([data-theme="midnight"]) {
    --primary-color: #4ecca3;
    --bg-color: rgba(26, 26, 46, 0.95);
    --glass-border: rgba(78, 204, 163, 0.2);
    --accent-glow: rgba(78, 204, 163, 0.1);
  }

  :global([data-theme="solar"]) {
    --primary-color: #ff9f43;
    --bg-color: rgba(43, 27, 23, 0.95);
    --glass-border: rgba(255, 159, 67, 0.2);
    --accent-glow: rgba(255, 159, 67, 0.1);
  }

  :global([data-theme="matrix"]) {
    --primary-color: #00ff41;
    --bg-color: rgba(0, 13, 0, 0.95);
    --glass-border: rgba(0, 255, 65, 0.2);
    --accent-glow: rgba(0, 255, 65, 0.1);
  }

  :global([data-theme="frost"]) {
    --primary-color: #00d2ff;
    --bg-color: rgba(0, 26, 26, 0.95);
    --glass-border: rgba(0, 210, 255, 0.2);
    --accent-glow: rgba(0, 210, 255, 0.1);
  }

  :global([data-theme="cyberpunk"]) {
    --primary-color: #f7f700;
    --bg-color: rgba(13, 13, 13, 0.98);
    --glass-border: rgba(247, 247, 0, 0.3);
    --accent-glow: rgba(247, 247, 0, 0.15);
  }

  main {
    width: 100vw;
    height: 100vh;
    background: var(--bg-color);
    border: 1px solid var(--glass-border);
    border-radius: 12px;
    display: flex;
    flex-direction: column;
    box-shadow: 0 8px 32px rgba(0,0,0,0.5);
    backdrop-filter: blur(10px);
  }

  .view-container {
    padding: 16px;
    flex: 1;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
    gap: 16px;
    transition: filter 0.3s;
  }

  .view-container.eye-saver {
    filter: sepia(0.5) brightness(0.9);
  }

  header {
    height: 48px;
    padding: 0 16px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    border-bottom: 1px solid var(--glass-border);
    background: rgba(255, 255, 255, 0.03);
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
    gap: 8px;
  }

  .title {
    font-size: 0.85em;
    font-weight: 700;
    letter-spacing: 1px;
    text-transform: uppercase;
    background: linear-gradient(90deg, white, var(--primary-color));
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
  }

  .controls {
    display: flex;
    gap: 4px;
  }

  .icon-btn {
    width: 32px;
    height: 32px;
    border-radius: 6px;
    border: none;
    background: transparent;
    color: rgba(255, 255, 255, 0.6);
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: all 0.2s;
  }

  .icon-btn:hover {
    background: rgba(255, 255, 255, 0.1);
    color: white;
  }

  .icon-btn.close:hover {
    background: #ff4757;
  }

  .separator {
    height: 1px;
    background: linear-gradient(90deg, transparent, var(--glass-border), transparent);
  }

  .section-title {
    font-size: 0.65em;
    font-weight: 800;
    text-transform: uppercase;
    color: var(--primary-color);
    letter-spacing: 1.5px;
    margin-bottom: 12px;
    opacity: 0.8;
  }

  .process-list {
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .icon-container {
    width: 32px;
    height: 32px;
    background: rgba(255, 255, 255, 0.05);
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    color: var(--primary-color);
    overflow: hidden;
  }

  .app-icon {
    width: 20px;
    height: 20px;
    object-fit: contain;
  }

  .footer-nav {
    display: flex;
    gap: 8px;
    margin-top: 24px;
    padding-top: 16px;
    border-top: 1px solid var(--glass-border);
  }

  .nav-btn {
    flex: 1;
    height: 36px;
    background: rgba(255, 255, 255, 0.05);
    border: 1px solid transparent;
    border-radius: 8px;
    color: rgba(255, 255, 255, 0.7);
    font-size: 0.75em;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    transition: all 0.2s;
  }

  .nav-btn:hover {
    background: rgba(255, 255, 255, 0.1);
    color: white;
  }

  .nav-btn.active {
    background: var(--accent-glow);
    border-color: var(--glass-border);
    color: var(--primary-color);
  }

  .about-card, .settings-card {
    margin-top: 16px;
    padding: 16px;
    background: rgba(255, 255, 255, 0.03);
    border-radius: 12px;
    border: 1px solid var(--glass-border);
    animation: fadeIn 0.3s ease-out;
  }

  @keyframes fadeIn {
    from { opacity: 0; transform: translateY(10px); }
    to { opacity: 1; transform: translateY(0); }
  }

  .about-logo {
    margin-bottom: 12px;
    display: flex;
    justify-content: center;
  }

  .about-card h3 {
    margin: 0 0 4px 0;
    text-align: center;
    font-size: 1.1em;
  }

  .about-card p {
    margin: 0 0 16px 0;
    text-align: center;
    font-size: 0.8em;
    opacity: 0.6;
  }

  .about-details {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
    font-size: 0.7em;
    opacity: 0.4;
  }

  .setting-item {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  .setting-label {
    font-size: 0.85em;
    font-weight: 600;
  }

  .setting-desc {
    font-size: 0.7em;
    opacity: 0.5;
  }

  .toggle-btn {
    padding: 6px 12px;
    border-radius: 20px;
    border: 1px solid var(--glass-border);
    background: rgba(255, 255, 255, 0.05);
    color: white;
    font-size: 0.7em;
    font-weight: 800;
    cursor: pointer;
    transition: all 0.2s;
  }

  .toggle-btn.active {
    background: var(--primary-color);
    color: black;
    border-color: var(--primary-color);
  }

  .theme-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
  }

  .theme-selector {
    display: flex;
    gap: 8px;
    background: rgba(255, 255, 255, 0.05);
    padding: 6px;
    border-radius: 20px;
  }

  .theme-dot {
    width: 20px;
    height: 20px;
    border-radius: 50%;
    border: 2px solid transparent;
    cursor: pointer;
    transition: all 0.2s;
  }

  .theme-dot.active {
    border-color: white;
    transform: scale(1.2);
  }

  .eye-saver-toggle {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 6px 12px;
    background: rgba(255, 255, 255, 0.05);
    border: 1px solid var(--glass-border);
    border-radius: 20px;
    color: white;
    font-size: 0.7em;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s;
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

  .advanced-btn.active {
    background: var(--primary-color);
    border-color: var(--primary-color);
  }

  /* Profiles UI */
  .profiles-container {
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .profiles-list {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 10px;
  }

  .profile-item {
    display: flex;
    background: rgba(255, 255, 255, 0.05);
    border: 1px solid var(--glass-border);
    border-radius: 8px;
    overflow: hidden;
    transition: all 0.2s;
  }

  .profile-item:hover {
    border-color: var(--primary-color);
    transform: translateY(-2px);
  }

  .profile-btn {
    flex: 1;
    background: transparent;
    border: none;
    color: white;
    padding: 10px;
    font-size: 0.75em;
    font-weight: 600;
    cursor: pointer;
    text-align: left;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .profile-delete {
    background: rgba(255, 0, 0, 0.1);
    border: none;
    border-left: 1px solid var(--glass-border);
    color: rgba(255, 255, 255, 0.5);
    padding: 0 10px;
    cursor: pointer;
    transition: all 0.2s;
  }

  .profile-delete:hover {
    background: #ff4444;
    color: white;
  }

  .add-profile-btn {
    background: transparent;
    border: 1px dashed var(--glass-border);
    color: rgba(255, 255, 255, 0.5);
    padding: 12px;
    border-radius: 8px;
    font-size: 0.75em;
    cursor: pointer;
    transition: all 0.2s;
  }

  .add-profile-btn:hover {
    background: rgba(255, 255, 255, 0.03);
    color: var(--primary-color);
    border-color: var(--primary-color);
  }
</style>
