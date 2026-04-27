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
  let peaks = {}; // PID -> value (0.0 to 1.0)
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

    ipcRenderer.on('audio-peaks', (event, data) => {
      peaks = data;
    });

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
      {/each}
    </section>

    <footer>
      Vibe: {themes.find(t => t.id === currentTheme).name} | 
      {eyeSaver ? 'Protection' : 'Standard'}
    </footer>
  </div>
</main>

<style>
  /* ... (styles remain same) */
</style>
