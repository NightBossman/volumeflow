<script>
  import './app.css';
  import VolumeSlider from './lib/VolumeSlider.svelte';
  import { onMount, onDestroy } from 'svelte';
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
    Power,
    Zap,
    Mic
  } from '@lucide/svelte';

  const { ipcRenderer } = window.electron;

  let isExpanded = $state(false);
  let showAbout = $state(false);
  let currentTheme = $state('midnight');
  let eyeSaver = $state(false);
  let autoStart = $state(false);

  // Auto-Duck States
  let duckingEnabled = $state(false);
  let duckingTriggerPid = $state(-1);
  let duckingThreshold = $state(0.05);
  let duckingFactor = $state(0.2);
  let masterVolume = $state(75);
  let masterMuted = $state(false);
  let masterId = $state('');
  let processes = $state([]);
  let peaks = $state({}); // PID -> value (0.0 to 1.0)
  let masterPeak = $state(0);
  let profiles = $state([]); // { id, name, sessions: [{ name, volume }], masterVolume }
  let iconCache = $state(new Map());

  // Recording & Boost States
  let recordingPids = $state(new Set());
  let isBoostActive = $state(false);

  function toggleRecording(pid) {
    if (recordingPids.has(pid)) {
      recordingPids.delete(pid);
      ipcRenderer.send('stop-recording', { pid });
    } else {
      recordingPids.add(pid);
      ipcRenderer.send('start-recording', { pid });
    }
  }

  function toggleBoost() {
    isBoostActive = !isBoostActive;
    ipcRenderer.send('set-boost', { active: isBoostActive });
  }

  const themes = [
    { id: 'midnight', name: 'Midnight', color: '#0078d4' },
    { id: 'solar', name: 'Solar', color: '#ff4d00' },
    { id: 'matrix', name: 'Matrix', color: '#00ff41' },
    { id: 'frost', name: 'Frost', color: '#00f2ff' },
    { id: 'cyberpunk', name: 'Cyberpunk', color: '#fcee0a' }
  ];

  async function loadSessions() {
    try {
      const liveProcesses = await ipcRenderer.invoke('get-audio-sessions');
      if (liveProcesses) {
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

  function saveSettings() {
    ipcRenderer.send('save-settings', $state.snapshot({
      theme: currentTheme,
      eyeSaver: eyeSaver,
      autoStart: autoStart,
      profiles: profiles,
      ducking: {
        enabled: duckingEnabled,
        triggerPid: duckingTriggerPid,
        threshold: duckingThreshold,
        factor: duckingFactor
      }
    }));
  }

  function updateDucking() {
    ipcRenderer.send('set-ducking', {
      enabled: duckingEnabled,
      triggerPid: duckingTriggerPid,
      threshold: duckingThreshold,
      factor: duckingFactor
    });
    saveSettings();
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
    await ipcRenderer.invoke('apply-profile', $state.snapshot(profile));
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
    showAbout = false;
    const width = 400;
    const height = isExpanded ? 700 : 350;
    ipcRenderer.send('set-window-size', { width, height });
  }

  function handleVolumeChange(id, volume) {
    ipcRenderer.send('set-session-volume', { id, volume: volume / 100 });
  }

  function handleMasterChange() {
    ipcRenderer.send('set-master-volume', { id: 'master', volume: masterVolume / 100 });
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
        if (savedSettings.ducking) {
          duckingEnabled = savedSettings.ducking.enabled ?? false;
          duckingTriggerPid = savedSettings.ducking.triggerPid ?? -1;
          duckingThreshold = savedSettings.ducking.threshold ?? 0.05;
          duckingFactor = savedSettings.ducking.factor ?? 0.2;
          // Notify bridge immediately after load
          setTimeout(updateDucking, 1000);
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
    
    // on() returns an unsubscribe closure — no removeListener matching needed
    const unsubPeaks = ipcRenderer.on('audio-peaks', handlePeaks);

    pollSessions();
    ipcRenderer.send('set-window-size', { width: 400, height: 600 });
    
    return () => {
      isRunning = false;
      if (typeof unsubPeaks === 'function') unsubPeaks();
    };
  });
</script>

<main class:eye-saver-active={eyeSaver}>
  <div class="eye-saver-overlay"></div>
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

  <div class="view-container">
    <section class="master-section">
      <VolumeSlider 
        label="Głośność Główna" 
        bind:value={masterVolume} 
        isMaster={true} 
        muted={masterMuted}
        peak={masterPeak}
        isBoost={isBoostActive}
        onboost={toggleBoost}
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
          isRecording={recordingPids.has(process.pid)}
          onrecord={() => toggleRecording(process.pid)}
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
                class="eye-saver-toggle no-drag" 
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

            <div class="section-title" style="margin-top: 24px">Auto-Duck (Inteligentne Wyciszanie)</div>
            <div class="ducking-card">
              <div class="ducking-row">
                <div class="ducking-info">
                  <Zap size={14} color="var(--primary-color)" />
                  <span>Aktywuj Auto-Duck</span>
                </div>
                <label class="switch">
                  <input type="checkbox" class="no-drag" bind:checked={duckingEnabled} onchange={updateDucking}>
                  <span class="slider round"></span>
                </label>
              </div>
              
              {#if duckingEnabled}
                <div class="ducking-settings">
                  <div class="duck-setting-item">
                    <label>Proces wyzwalający (Trigger):</label>
                    <select class="no-drag" bind:value={duckingTriggerPid} onchange={updateDucking}>
                      <option value={-1}>Wybierz proces...</option>
                      {#each processes as p}
                        <option value={p.pid}>{p.name} (PID: {p.pid})</option>
                      {/each}
                    </select>
                  </div>
                  
                  <div class="duck-setting-item">
                    <div class="label-row">
                      <label>Czułość (Threshold):</label>
                      <span>{(duckingThreshold * 100).toFixed(0)}%</span>
                    </div>
                    <input type="range" class="no-drag" min="0.01" max="0.5" step="0.01" bind:value={duckingThreshold} oninput={updateDucking}>
                  </div>
                  
                  <div class="duck-setting-item">
                    <div class="label-row">
                      <label>Siła wyciszenia (Duck Factor):</label>
                      <span>{(duckingFactor * 100).toFixed(0)}%</span>
                    </div>
                    <input type="range" class="no-drag" min="0.05" max="0.8" step="0.05" bind:value={duckingFactor} oninput={updateDucking}>
                  </div>
                </div>
              {/if}
            </div>

            <div class="section-title" style="margin-top: 24px">Tryb Scen (Profile)</div>
          <div class="profiles-container">
            <div class="profiles-list">
              {#each profiles as profile}
                <div class="profile-item">
                  <button class="profile-btn no-drag" onclick={() => applyProfile(profile)}>
                    {profile.name}
                  </button>
                  <button class="profile-delete no-drag" onclick={() => deleteProfile(profile.id)}>
                    <X size={12} />
                  </button>
                </div>
              {/each}
            </div>
            <button class="add-profile-btn no-drag" onclick={addProfile}>
              <span>+ Zapisz obecną scenę</span>
            </button>
          </div>

          <div class="section-title" style="margin-top: 24px">Ustawienia Systemowe</div>
          <div class="advanced-options">
            <button class="advanced-btn no-drag" class:active={autoStart} onclick={() => { autoStart = !autoStart; saveSettings(); }}>
              <Power size={16} /> <span>{autoStart ? 'Autostart: ON' : 'Autostart: OFF'}</span>
            </button>
            <button class="advanced-btn no-drag" onclick={() => showAbout = true}>
              <Info size={16} /> <span>O programie</span>
            </button>
          </div>
          {:else}
            <div class="about-card">
              <div class="about-header">
                <Activity size={24} color="var(--primary-color)" />
                <h3>VolumeFlow v1.5.1</h3>
              </div>
              <p>Premium Windows Audio Mixer stworzony z myślą o estetyce i wydajności.</p>
              <div class="stats">
                <div class="stat-item">
                  <span class="stat-label">Technologia:</span>
                  <span class="stat-val">Svelte 5 + Electron</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">Status:</span>
                  <span class="stat-val">Hardened (v1.5.1)</span>
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

  .advanced-btn.active {
    background: var(--primary-color);
    border-color: var(--primary-color);
  }

  /* Ducking UI */
  .ducking-card {
    background: rgba(255, 255, 255, 0.04);
    border: 1px solid var(--glass-border);
    border-radius: 12px;
    padding: 14px;
    margin-bottom: 20px;
    transition: all 0.3s ease;
  }

  .ducking-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .ducking-info {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 0.85em;
    font-weight: 600;
  }

  .ducking-settings {
    margin-top: 16px;
    display: flex;
    flex-direction: column;
    gap: 14px;
    padding-top: 14px;
    border-top: 1px solid var(--glass-border);
    animation: slideDown 0.3s ease;
  }

  @keyframes slideDown {
    from { opacity: 0; transform: translateY(-10px); }
    to { opacity: 1; transform: translateY(0); }
  }

  .duck-setting-item {
    display: flex;
    flex-direction: column;
    gap: 6px;
  }

  .duck-setting-item label {
    font-size: 0.7em;
    color: rgba(255, 255, 255, 0.5);
    font-weight: 600;
  }

  .duck-setting-item select {
    background: rgba(0, 0, 0, 0.3);
    border: 1px solid var(--glass-border);
    color: white;
    padding: 8px;
    border-radius: 6px;
    font-size: 0.85em;
    outline: none;
  }

  .label-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .label-row span {
    font-size: 0.75em;
    color: var(--primary-color);
    font-weight: 700;
  }

  /* Switch Style */
  .switch {
    position: relative;
    display: inline-block;
    width: 36px;
    height: 20px;
  }

  .switch input { opacity: 0; width: 0; height: 0; }

  .slider {
    position: absolute;
    cursor: pointer;
    top: 0; left: 0; right: 0; bottom: 0;
    background-color: rgba(255,255,255,0.1);
    transition: .4s;
  }

  .slider:before {
    position: absolute;
    content: "";
    height: 14px; width: 14px;
    left: 3px; bottom: 3px;
    background-color: white;
    transition: .4s;
  }

  input:checked + .slider { background-color: var(--primary-color); }
  input:checked + .slider:before { transform: translateX(16px); }
  .slider.round { border-radius: 20px; }
  .slider.round:before { border-radius: 50%; }

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
    gap: 12px;
    animation: fadeIn 0.3s ease;
  }

  @keyframes fadeIn {
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
