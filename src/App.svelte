<script>
  import { onMount } from 'svelte';
  import { 
    Activity, 
    X, 
    Minimize2, 
    Maximize2, 
    Settings, 
    Volume2, 
    VolumeX, 
    Sun, 
    Moon, 
    Info, 
    Heart, 
    Users,
    Minus,
    Power
  } from 'lucide-svelte';
  import VolumeSlider from './lib/VolumeSlider.svelte';

  const { ipcRenderer } = window.require('electron');

  let isExpanded = false;
  let showAbout = false;
  let currentTheme = 'midnight';
  let eyeSaver = false;
  let autoStart = false;
  let masterVolume = 75;
  let masterMuted = false;
  let masterId = '';
  let processes = [];
  let profiles = []; // { id, name, sessions: [{ name, volume }], masterVolume }
  let iconCache = new Map();

  const themes = [
    { id: 'midnight', name: 'Midnight', color: '#0078d4' },
    { id: 'solar', name: 'Solar', color: '#ff4d00' },
    { id: 'matrix', name: 'Matrix', color: '#00ff41' },
    { id: 'frost', name: 'Frost', color: '#00f2ff' },
    { id: 'cyberpunk', name: 'Cyberpunk', color: '#fcee0a' }
  ];

  async function loadSessions() {
    const data = await ipcRenderer.invoke('get-audio-sessions');
    processes = data.map(p => ({
      ...p,
      volume: Math.round(p.volume * 100)
    }));

    // Fetch icons for new paths
    for (const p of processes) {
      if (p.path && !iconCache.has(p.path)) {
        iconCache.set(p.path, 'loading');
        ipcRenderer.invoke('get-app-icon', p.path).then(iconData => {
          if (iconData) {
            iconCache.set(p.path, iconData);
            iconCache = new Map(iconCache); // Trigger reactivity
          }
        });
      }
    }
  }

  async function loadMaster() {
    const data = await ipcRenderer.invoke('get-master-info');
    masterVolume = data.volume;
    masterMuted = data.muted;
    masterId = data.id;
  }

  function saveSettings() {
    ipcRenderer.send('save-settings', {
      theme: currentTheme,
      eyeSaver: eyeSaver,
      autoStart: autoStart,
      profiles: profiles
    });
  }

  function addProfile() {
    const name = prompt('Nazwa sceny:', `Scena ${profiles.length + 1}`);
    if (!name) return;

    const newProfile = {
      id: Date.now(),
      name: name,
      masterVolume: masterVolume,
      sessions: processes.map(p => ({ name: p.name, volume: p.volume / 100 }))
    };

    profiles = [...profiles, newProfile];
    saveSettings();
  }

  async function applyProfile(profile) {
    await ipcRenderer.invoke('apply-profile', profile);
  }

  function deleteProfile(id) {
    profiles = profiles.filter(p => p.id !== id);
    saveSettings();
  }

  function setTheme(themeId) {
    currentTheme = themeId;
    document.body.setAttribute('data-theme', themeId);
    saveSettings();
  }

  function toggleMode() {
    isExpanded = !isExpanded;
    const width = isExpanded ? 700 : 350;
    const height = 600;
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
  }

  function closeApp() {
    ipcRenderer.send('close-app');
  }

  function hideToTray() {
    ipcRenderer.send('minimize-to-tray');
  }

  onMount(async () => {
    const savedSettings = await ipcRenderer.invoke('load-settings');
    if (savedSettings) {
      if (savedSettings.theme) {
        setTheme(savedSettings.theme);
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

    loadSessions();
    loadMaster();
    const interval = setInterval(() => {
      loadSessions();
      loadMaster();
    }, 2500);

    return () => clearInterval(interval);
  });
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
            </div>
            <button class="add-profile-btn" onclick={addProfile}>
              <span>+ Zapisz obecną scenę</span>
            </button>
          </div>

          <div class="section-title" style="margin-top: 24px">Ustawienia Systemowe</div>
          <div class="advanced-options">
            <button class="advanced-btn" class:active={autoStart} onclick={() => { autoStart = !autoStart; saveSettings(); }}>
              <Power size={16} /> <span>{autoStart ? 'Autostart: ON' : 'Autostart: OFF'}</span>
            </button>
            <button class="advanced-btn" onclick={() => showAbout = true}>
              <Info size={16} /> <span>O programie</span>
            </button>
          </div>
          {:else}
            <div class="about-card">
              <div class="about-header">
                <Activity size={24} color="var(--primary-color)" />
                <h3>VolumeFlow v1.0.0</h3>
              </div>
              <p>Premium Windows Audio Mixer stworzony z myślą o estetyce i wydajności.</p>
              <div class="stats">
                <div class="stat-item">
                  <span class="stat-label">Technologia:</span>
                  <span class="stat-val">Svelte 5 + Electron</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">Status:</span>
                  <span class="stat-val">Stabilny (v1.0.0)</span>
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
  </div>
</main>

<style>
  main {
    display: flex;
    flex-direction: column;
    height: 100%;
    box-sizing: border-box;
  }

  .view-container {
    display: flex;
    flex-direction: column;
    flex: 1;
    overflow: hidden;
    transition: filter 0.5s ease;
  }

  header {
    height: 60px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0 16px;
    background: rgba(255, 255, 255, 0.05);
    border-bottom: 1px solid var(--glass-border);
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
    font-size: 0.9em;
    font-weight: 700;
    letter-spacing: 0.05em;
    text-transform: uppercase;
    color: var(--text-color);
    opacity: 0.8;
  }

  .controls {
    display: flex;
    gap: 8px;
  }

  .icon-btn {
    background: transparent;
    border: none;
    color: white;
    width: 32px;
    height: 32px;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: all 0.2s;
    opacity: 0.6;
  }

  .icon-btn:hover {
    background: var(--glass-border);
    opacity: 1;
  }

  .icon-btn.close:hover {
    background: var(--danger-color);
  }

  .master-section {
    padding: 24px 16px;
  }

  .separator {
    height: 1px;
    background: linear-gradient(90deg, transparent, var(--glass-border), transparent);
    margin: 0 16px;
  }

  .process-list {
    flex: 1;
    overflow-y: auto;
    padding: 16px;
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .section-title {
    font-size: 0.7em;
    font-weight: 800;
    text-transform: uppercase;
    color: var(--text-color);
    opacity: 0.4;
    margin-bottom: 4px;
    letter-spacing: 0.1em;
  }

  .icon-container {
    width: 24px;
    height: 24px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: rgba(255, 255, 255, 0.05);
    border-radius: 6px;
    color: var(--primary-color);
  }

  .app-icon {
    width: 18px;
    height: 18px;
    object-fit: contain;
  }

  .advanced-section {
    margin-top: 24px;
    padding-top: 24px;
    border-top: 1px solid var(--glass-border);
    display: flex;
    flex-direction: column;
    gap: 20px;
    animation: fadeIn 0.3s ease;
  }

  .theme-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
    background: rgba(255, 255, 255, 0.03);
    padding: 12px;
    border-radius: 12px;
    border: 1px solid var(--glass-border);
  }

  .theme-selector {
    display: flex;
    gap: 10px;
  }

  .theme-dot {
    width: 20px;
    height: 20px;
    border-radius: 50%;
    border: 2px solid transparent;
    cursor: pointer;
    transition: all 0.2s;
  }

  .theme-dot:hover {
    transform: scale(1.2);
  }

  .theme-dot.active {
    border-color: white;
    box-shadow: 0 0 10px var(--primary-color);
  }

  .eye-saver-toggle {
    display: flex;
    align-items: center;
    gap: 8px;
    background: rgba(255, 255, 255, 0.05);
    border: 1px solid var(--glass-border);
    color: white;
    padding: 6px 12px;
    border-radius: 20px;
    font-size: 0.7em;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s;
  }

  .eye-saver-toggle:hover {
    background: var(--glass-border);
  }

  .eye-saver-toggle.active {
    background: #ffcc00;
    color: #333;
    border-color: #ffcc00;
  }

  .advanced-options {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 12px;
  }

  .advanced-btn {
    display: flex;
    align-items: center;
    gap: 10px;
    background: rgba(255, 255, 255, 0.03);
    border: 1px solid var(--glass-border);
    color: white;
    padding: 12px;
    border-radius: 12px;
    font-size: 0.8em;
    cursor: pointer;
    transition: all 0.2s;
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
    border-style: solid;
    border-color: var(--primary-color);
    color: var(--primary-color);
    background: rgba(255, 255, 255, 0.02);
  }

  /* About Card Styles */
  .about-card {
    display: flex;
    flex-direction: column;
    gap: 20px;
    background: rgba(255, 255, 255, 0.05);
    padding: 24px;
    border-radius: 16px;
    border: 1px solid var(--glass-border);
  }

  .about-header {
    display: flex;
    align-items: center;
    gap: 15px;
  }

  .about-header h3 {
    margin: 0;
    font-size: 1.1em;
    letter-spacing: 0.05em;
  }

  .about-card p {
    margin: 0;
    font-size: 0.85em;
    line-height: 1.6;
    opacity: 0.7;
  }

  .stats {
    display: flex;
    flex-direction: column;
    gap: 10px;
  }

  .stat-item {
    display: flex;
    justify-content: space-between;
    font-size: 0.75em;
  }

  .stat-label {
    opacity: 0.5;
  }

  .stat-val {
    font-weight: 600;
    color: var(--primary-color);
  }

  .about-footer {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-top: 10px;
  }

  .back-btn {
    background: var(--primary-color);
    border: none;
    color: white;
    padding: 8px 20px;
    border-radius: 8px;
    font-size: 0.8em;
    font-weight: 600;
    cursor: pointer;
  }

  .made-with {
    font-size: 0.7em;
    opacity: 0.4;
    display: flex;
    align-items: center;
    gap: 4px;
  }

  footer {
    padding: 12px;
    text-align: center;
    font-size: 0.6em;
    font-weight: 700;
    letter-spacing: 0.1em;
    text-transform: uppercase;
    opacity: 0.3;
    border-top: 1px solid var(--glass-border);
  }

  @keyframes fadeIn {
    from { opacity: 0; transform: translateY(10px); }
    to { opacity: 1; transform: translateY(0); }
  }
</style>
