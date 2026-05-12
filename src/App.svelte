<script>
  import { onMount } from 'svelte';
  const { ipcRenderer } = window.electron;

  let sessions = [];
  let masterVolume = 0;
  let masterMuted = false;
  let duckingEnabled = false;
  let duckingTriggerPid = -1;
  let activeTab = 'mixer'; // 'mixer', 'settings', 'about'
  let activeRecordings = new Set();
  let boostActive = false;
  let searchQuery = '';

  // Peak levels (updated via bridge-peaks event)
  let masterPeak = 0;
  let sessionPeaks = {};

  // Global Hotkeys
  let hotkeys = {
    toggleRecording: 'Ctrl+Alt+R',
    toggleMasterMute: 'Ctrl+Alt+M',
    toggleBoost: 'Ctrl+Alt+B',
  };
  let hotkeyEditing = null; // which key is being edited
  let hotkeyError = '';
  let hotkeySaved = false;

  const isMini = window.electron?.isMini || false;
  let advancedSettings = {
    duckingThreshold: 0.05,
    duckingFactor: 0.2,
    fadeDuration: 800,
    autoStart: false
  };
  let settingsLoading = true;

  $: filteredSessions = sessions.filter(s => 
    s.name.toLowerCase().includes(searchQuery.toLowerCase()) || 
    s.pid.toString().includes(searchQuery)
  );

  onMount(() => {
    refreshSessions();
    refreshMaster();

    // Load hotkeys from main process (fire-and-forget, no await)
    ipcRenderer.invoke('get-hotkeys').then(savedHotkeys => {
      if (savedHotkeys) hotkeys = { ...hotkeys, ...savedHotkeys };
    });

    // Load advanced settings
    ipcRenderer.invoke('load-settings').then(cfg => {
      if (cfg) {
        if (cfg.advancedSettings) advancedSettings = { ...advancedSettings, ...cfg.advancedSettings };
        advancedSettings.autoStart = cfg.autoStart || false;
      }
      settingsLoading = false;
    });

    const interval = setInterval(refreshSessions, 1000);

    ipcRenderer.on('bridge-peaks', (event, data) => {
      masterPeak = data.master;
      sessionPeaks = data.sessions;
    });

    // Sync boost/recording state triggered by global hotkeys
    ipcRenderer.on('hotkey-boost-changed', (event, active) => {
      boostActive = active;
    });
    ipcRenderer.on('hotkey-recording-changed', (event, { pid, active }) => {
      if (active) {
        activeRecordings.add(pid);
      } else {
        activeRecordings.delete(pid);
      }
      activeRecordings = activeRecordings;
    });

    return () => {
      clearInterval(interval);
      ipcRenderer.removeAllListeners('bridge-peaks');
      ipcRenderer.removeAllListeners('hotkey-boost-changed');
      ipcRenderer.removeAllListeners('hotkey-recording-changed');
    };
  });

  async function refreshSessions() {
    ipcRenderer.send('bridge-command', { action: 'get_sessions' });
  }

  async function refreshMaster() {
    ipcRenderer.send('bridge-command', { action: 'get_master' });
  }

  ipcRenderer.on('bridge-response', (event, response) => {
    if (response.status === 'ok') {
      if (response.sessions) {
        sessions = response.sessions;
      }
      if (response.master) {
        masterVolume = response.master.volume;
        masterMuted = response.master.muted;
      }
    }
  });

  function setVolume(pid, volume) {
    ipcRenderer.send('set-session-volume', { id: pid, volume });
  }

  function toggleMute(pid) {
    ipcRenderer.send('toggle-session-mute', { id: pid });
  }

  function setMasterVolume(volume) {
    ipcRenderer.send('set-master-volume', { id: 'master', volume });
  }

  function toggleMasterMute() {
    ipcRenderer.send('toggle-session-mute', { id: 'master' });
  }

  function toggleDucking() {
    duckingEnabled = !duckingEnabled;
    ipcRenderer.send('set-ducking', { 
      enabled: duckingEnabled,
      triggerPid: duckingTriggerPid,
      threshold: advancedSettings.duckingThreshold,
      factor: advancedSettings.duckingFactor
    });
  }

  function setDuckingTrigger(pid) {
    duckingTriggerPid = pid;
    if (duckingEnabled) {
      ipcRenderer.send('set-ducking', { 
        enabled: true,
        triggerPid: pid
      });
    }
  }

  function toggleRecording(pid) {
    if (activeRecordings.has(pid)) {
      activeRecordings.delete(pid);
      activeRecordings = activeRecordings; // trigger reactivity
      ipcRenderer.send('stop-recording', { pid });
      ipcRenderer.send('show-osd', { message: 'Recording stopped', icon: 'stop' });
    } else {
      activeRecordings.add(pid);
      activeRecordings = activeRecordings;
      ipcRenderer.send('start-recording', { pid });
      ipcRenderer.send('show-osd', { message: 'Recording started', icon: 'record' });
    }
  }

  function toggleBoost() {
    boostActive = !boostActive;
    ipcRenderer.send('set-boost', { active: boostActive, factor: 0.6 });
  }

  function openRecordingsFolder() {
    ipcRenderer.send('open-recordings');
  }

  function closeApp() {
    ipcRenderer.send('close-app');
  }

  function minimizeApp() {
    ipcRenderer.send('minimize-app');
  }

  // ---- Global Hotkeys ----

  const hotkeyLabels = {
    toggleRecording: 'Record Active Session',
    toggleMasterMute: 'Toggle Master Mute',
    toggleBoost: 'Toggle Smart Overdrive',
  };

  function startEditingHotkey(key) {
    hotkeyEditing = key;
    hotkeyError = '';
    hotkeySaved = false;
  }

  function handleHotkeyKeydown(event, key) {
    event.preventDefault();
    const parts = [];
    if (event.ctrlKey)  parts.push('Ctrl');
    if (event.altKey)   parts.push('Alt');
    if (event.shiftKey) parts.push('Shift');
    const code = event.key;
    // Ignore bare modifier presses
    if (['Control','Alt','Shift','Meta'].includes(code)) return;
    parts.push(code.length === 1 ? code.toUpperCase() : code);
    if (parts.length < 2) {
      hotkeyError = 'Use at least one modifier key (Ctrl, Alt, Shift).';
      return;
    }
    hotkeys = { ...hotkeys, [key]: parts.join('+') };
    hotkeyEditing = null;
    hotkeyError = '';
  }

  function cancelEditHotkey() {
    hotkeyEditing = null;
    hotkeyError = '';
  }

  async function saveHotkeys() {
    hotkeyError = '';
    const result = ipcRenderer.sendSync('save-hotkeys', hotkeys);
    if (result && result.ok) {
      hotkeySaved = true;
      setTimeout(() => hotkeySaved = false, 2000);
    } else {
      hotkeyError = (result && result.error) || 'Failed to save hotkeys.';
    }
  }

  function saveSettings() {
    ipcRenderer.send('save-settings', {
      autoStart: advancedSettings.autoStart,
      advancedSettings: {
        duckingThreshold: advancedSettings.duckingThreshold,
        duckingFactor: advancedSettings.duckingFactor,
        fadeDuration: advancedSettings.fadeDuration
      }
    });
    // We could add a "Saved" indicator here too
  }

  function toggleMiniPlayer() {
    ipcRenderer.send('toggle-mini-player');
  }
</script>

<main class={isMini ? 'mini-mode' : ''}>
  <div class="glass-container {isMini ? 'mini-container' : ''}">
    {#if isMini}
      <!-- Mini Player View -->
      <header class="title-bar mini-drag">
        <div class="brand">
          <div class="logo small"></div>
          <h1>Flow</h1>
        </div>
        <div class="window-controls">
          <button class="control-btn mini-btn" on:click={toggleMiniPlayer} title="Exit Mini-Player">
            <svg viewBox="0 0 24 24" width="14" height="14"><path fill="currentColor" d="M19 19H5V5h7V3H5c-1.11 0-2 .9-2 2v14c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2v-7h-2v7zM14 3v2h3.59l-9.83 9.83 1.41 1.41L19 6.41V10h2V3h-7z"/></svg>
          </button>
          <button class="control-btn close" on:click={closeApp}>
            <svg viewBox="0 0 24 24" width="14" height="14"><path fill="currentColor" d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12 19 6.41z"/></svg>
          </button>
        </div>
      </header>

      <div class="mini-content">
        <!-- Master control -->
        <div class="mini-section">
          <div class="mini-header">Master Output</div>
          <div class="mini-control-row">
             <button class="mute-btn small {masterMuted ? 'muted' : ''}" on:click={toggleMasterMute}>
               <svg viewBox="0 0 24 24" width="14" height="14"><path fill="currentColor" d="M3 9v6h4l5 5V4L7 9H3zm13.5 3c0-1.77-1.02-3.29-2.5-4.03v8.05c1.48-.73 2.5-2.25 2.5-4.02z"/></svg>
             </button>
             <div class="mini-slider">
               <input type="range" min="0" max="1" step="0.01" value={masterVolume} on:input={(e) => setMasterVolume(e.target.value)} />
             </div>
             <span class="mini-val">{Math.round(masterVolume * 100)}%</span>
          </div>
        </div>

        <!-- Top Sessions -->
        <div class="mini-section">
          <div class="mini-header">Active Apps</div>
          {#each sessions.slice(0, 3) as session}
            <div class="mini-session-row">
              <span class="mini-app-name" title={session.name}>{session.name}</span>
              <div class="mini-slider-small">
                <input type="range" min="0" max="1" step="0.01" value={session.volume} on:input={(e) => setVolume(session.pid, e.target.value)} />
              </div>
            </div>
          {/each}
        </div>
      </div>
    {:else}
    <!-- Header / Title Bar -->
    <header class="title-bar">
      <div class="brand">
        <div class="logo">
          <div class="logo-inner"></div>
        </div>
        <h1>VolumeFlow</h1>
      </div>
      <div class="window-controls">
        <button class="control-btn mini-btn" on:click={toggleMiniPlayer} title="Switch to Mini-Player">
          <svg viewBox="0 0 24 24" width="14" height="14"><path fill="currentColor" d="M19 11h-8v6h8v-6zm4 8V4.98C23 3.88 22.1 3 21 3H3c-1.1 0-2 .88-2 1.98V19c0 1.1.9 2 2 2h18c1.1 0 2-.9 2-2zm-2 .02H3V4.97h18v14.05z"/></svg>
        </button>
        <button class="control-btn" on:click={minimizeApp}>
          <svg viewBox="0 0 24 24" width="14" height="14"><path fill="currentColor" d="M19 13H5v-2h14v2z"/></svg>
        </button>
        <button class="control-btn close" on:click={closeApp}>
          <svg viewBox="0 0 24 24" width="14" height="14"><path fill="currentColor" d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12 19 6.41z"/></svg>
        </button>
      </div>
    </header>

    <!-- Master Control Section -->
    <section class="master-control">
      <div class="master-info">
        <span class="label">System Master</span>
        <span class="value">{Math.round(masterVolume * 100)}%</span>
      </div>
      <div class="slider-group">
        <button class="mute-btn {masterMuted ? 'muted' : ''}" on:click={toggleMasterMute}>
          {#if masterMuted}
            <svg viewBox="0 0 24 24" width="18" height="18"><path fill="currentColor" d="M16.5 12c0-1.77-1.02-3.29-2.5-4.03v2.21l2.45 2.45c.03-.2.05-.41.05-.63zm2.5 0c0 .94-.2 1.82-.54 2.64l1.51 1.51C20.63 14.91 21 13.5 21 12c0-4.28-2.99-7.86-7-8.77v2.06c2.89.86 5 3.54 5 6.71zM4.27 3L3 4.27 7.73 9H3v6h4l5 5v-6.73l4.25 4.25c-.67.52-1.42.93-2.25 1.18v2.06c1.38-.31 2.63-.95 3.69-1.81L19.73 21 21 19.73l-9-9L4.27 3zM12 4L9.91 6.09 12 8.18V4z"/></svg>
          {:else}
            <svg viewBox="0 0 24 24" width="18" height="18"><path fill="currentColor" d="M3 9v6h4l5 5V4L7 9H3zm13.5 3c0-1.77-1.02-3.29-2.5-4.03v8.05c1.48-.73 2.5-2.25 2.5-4.02zM14 3.23v2.06c2.89.86 5 3.54 5 6.71s-2.11 5.85-5 6.71v2.06c4.01-.91 7-4.49 7-8.77s-2.99-7.86-7-8.77z"/></svg>
          {/if}
        </button>
        <div class="slider-container">
          <div class="peak-bg"></div>
          <div class="peak-bar master" style="width: {masterPeak * 100}%"></div>
          <input 
            type="range" 
            min="0" 
            max="1" 
            step="0.01" 
            value={masterVolume} 
            on:input={(e) => setMasterVolume(e.target.value)}
          />
        </div>
      </div>
    </section>

    <!-- Navigation Tabs -->
    <nav class="tabs">
      <button class="tab {activeTab === 'mixer' ? 'active' : ''}" on:click={() => activeTab = 'mixer'}>
        Mixer
      </button>
      <button class="tab {activeTab === 'settings' ? 'active' : ''}" on:click={() => activeTab = 'settings'}>
        Settings
      </button>
      <button class="tab {activeTab === 'about' ? 'active' : ''}" on:click={() => activeTab = 'about'}>
        About
      </button>
    </nav>

    <!-- Main Content Area -->
    <div class="content">
      {#if activeTab === 'mixer'}
        <div class="search-bar">
          <div class="search-icon">
            <svg viewBox="0 0 24 24" width="14" height="14"><path fill="currentColor" d="M15.5 14h-.79l-.28-.27A6.471 6.471 0 0 0 16 9.5 6.5 6.5 0 1 0 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z"/></svg>
          </div>
          <input 
            type="text" 
            placeholder="Search processes..." 
            bind:value={searchQuery}
          />
          {#if searchQuery}
            <button class="clear-search" on:click={() => searchQuery = ''}>&times;</button>
          {/if}
        </div>

        <div class="session-list">
          {#if filteredSessions.length === 0}
            <div class="empty-state">
              {#if searchQuery}
                No processes match your search.
              {:else}
                No active audio sessions found.
              {/if}
            </div>
          {/if}
          {#each filteredSessions as session (session.pid)}
            <div class="session-card {duckingTriggerPid === session.pid ? 'is-trigger' : ''}">
              <div class="session-header">
                <div class="session-info">
                  <span class="name">{session.name}</span>
                  <span class="pid">PID: {session.pid}</span>
                </div>
                <div class="session-actions">
                  <button 
                    class="action-btn {activeRecordings.has(session.pid) ? 'recording' : ''}" 
                    title="Record this app"
                    on:click={() => toggleRecording(session.pid)}
                  >
                    <div class="record-dot"></div>
                  </button>
                  <button 
                    class="action-btn {duckingTriggerPid === session.pid ? 'ducking' : ''}" 
                    title="Set as Ducking Trigger"
                    on:click={() => setDuckingTrigger(session.pid)}
                  >
                    <svg viewBox="0 0 24 24" width="14" height="14"><path fill="currentColor" d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-1 14H9v-2h2v2zm0-4H9V7h2v5z"/></svg>
                  </button>
                </div>
              </div>
              <div class="slider-group">
                <button class="mute-btn {session.muted ? 'muted' : ''}" on:click={() => toggleMute(session.pid)}>
                  <svg viewBox="0 0 24 24" width="16" height="16"><path fill="currentColor" d="M3 9v6h4l5 5V4L7 9H3zm13.5 3c0-1.77-1.02-3.29-2.5-4.03v8.05c1.48-.73 2.5-2.25 2.5-4.02z"/></svg>
                </button>
                <div class="slider-container">
                  <div class="peak-bg"></div>
                  <div class="peak-bar" style="width: {(sessionPeaks[session.pid] || 0) * 100}%"></div>
                  <input 
                    type="range" 
                    min="0" 
                    max="1" 
                    step="0.01" 
                    value={session.volume} 
                    on:input={(e) => setVolume(session.pid, e.target.value)}
                  />
                </div>
                <span class="vol-text">{Math.round(session.volume * 100)}%</span>
              </div>
            </div>
          {/each}
        </div>
      {:else if activeTab === 'settings'}
        <div class="settings-list">
          <div class="setting-item">
            <div class="setting-info">
              <span class="title">Smart Overdrive</span>
              <span class="desc">Podbij głośność powyżej 100% bez clippingu.</span>
            </div>
            <button class="toggle {boostActive ? 'on' : ''}" on:click={toggleBoost}>
              <div class="handle"></div>
            </button>
          </div>

          <div class="setting-item">
            <div class="setting-info">
              <span class="title">Auto-Ducking</span>
              <span class="desc">Wycisz tło gdy {duckingTriggerPid !== -1 ? 'wybrany proces' : 'proces'} emituje dźwięk.</span>
            </div>
            <button class="toggle {duckingEnabled ? 'on' : ''}" on:click={toggleDucking}>
              <div class="handle"></div>
            </button>
          </div>

          <div class="setting-item action-only">
            <div class="setting-info">
              <span class="title">Recordings Folder</span>
              <span class="desc">Otwórz folder z nagranymi plikami .wav</span>
            </div>
            <button class="action-btn primary" on:click={openRecordingsFolder}>
              Open Folder
            </button>
          </div>

          <!-- Advanced Audio Control -->
          <div class="hotkey-section">
            <div class="hotkey-section-header">
              <svg viewBox="0 0 24 24" width="14" height="14"><path fill="currentColor" d="M19.14 12.94c.04-.3.06-.61.06-.94 0-.32-.02-.64-.07-.94l2.03-1.58c.18-.14.23-.41.12-.61l-1.92-3.32c-.12-.22-.37-.29-.59-.22l-2.39.96c-.5-.38-1.03-.7-1.62-.94l-.36-2.54c-.04-.24-.24-.41-.48-.41h-3.84c-.24 0-.43.17-.47.41l-.36 2.54c-.59.24-1.13.57-1.62.94l-2.39-.96c-.22-.08-.47 0-.59.22L2.74 8.87c-.12.21-.08.47.12.61l2.03 1.58c-.05.3-.09.63-.09.94s.02.64.07.94l-2.03 1.58c-.18.14-.23.41-.12.61l1.92 3.32c.12.22.37.29.59.22l2.39-.96c.5.38 1.03.7 1.62.94l.36 2.54c.05.24.24.41.48.41h3.84c.24 0 .44-.17.47-.41l.36-2.54c.59-.24 1.13-.56 1.62-.94l2.39.96c.22.08.47 0 .59-.22l1.92-3.32c.12-.22.07-.47-.12-.61l-2.01-1.58zM12 15.6c-1.98 0-3.6-1.62-3.6-3.6s1.62-3.6 3.6-3.6 3.6 1.62 3.6 3.6-1.62 3.6-3.6 3.6z"/></svg>
              <span>Advanced Audio Control</span>
            </div>
            
            <div class="advanced-row">
              <span class="advanced-label">Ducking Threshold</span>
              <div class="advanced-input-group">
                <input type="range" min="0.01" max="0.5" step="0.01" bind:value={advancedSettings.duckingThreshold} on:change={saveSettings} />
                <span class="advanced-value">{advancedSettings.duckingThreshold}</span>
              </div>
            </div>

            <div class="advanced-row">
              <span class="advanced-label">Ducking Strength</span>
              <div class="advanced-input-group">
                <input type="range" min="0.05" max="0.8" step="0.05" bind:value={advancedSettings.duckingFactor} on:change={saveSettings} />
                <span class="advanced-value">{Math.round((1 - advancedSettings.duckingFactor) * 100)}%</span>
              </div>
            </div>

            <div class="advanced-row">
              <span class="advanced-label">Fade Duration (ms)</span>
              <div class="advanced-input-group">
                <input type="range" min="100" max="3000" step="50" bind:value={advancedSettings.fadeDuration} on:change={saveSettings} />
                <span class="advanced-value">{advancedSettings.fadeDuration}ms</span>
              </div>
            </div>
          </div>

          <!-- Global Hotkeys Section -->
          <div class="hotkey-section">
            <div class="hotkey-section-header">
              <svg viewBox="0 0 24 24" width="14" height="14"><path fill="currentColor" d="M20 5H4c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V7c0-1.1-.9-2-2-2zm-9 3h2v2h-2V8zm0 3h2v2h-2v-2zM8 8h2v2H8V8zm0 3h2v2H8v-2zm-1 5H5v-2h2v2zm0-3H5v-2h2v2zm0-3H5V8h2v2zm3 6H8v-2h2v2zm3 0h-2v-2h2v2zm3 0h-2v-2h2v2zm3-1h-2v-2h2v2zm0-3h-2v-2h2v2zm0-3h-2V8h2v2z"/></svg>
              <span>Global Hotkeys</span>
            </div>

            {#each Object.keys(hotkeys) as key}
              <div class="hotkey-row">
                <span class="hotkey-label">{hotkeyLabels[key]}</span>
                {#if hotkeyEditing === key}
                  <div
                    class="hotkey-capture"
                    tabindex="0"
                    on:keydown={(e) => handleHotkeyKeydown(e, key)}
                    on:blur={cancelEditHotkey}
                  >
                    Press keys…
                  </div>
                {:else}
                  <button class="hotkey-badge" on:click={() => startEditingHotkey(key)}>
                    {hotkeys[key]}
                  </button>
                {/if}
              </div>
            {/each}

            {#if hotkeyError}
              <p class="hotkey-error">{hotkeyError}</p>
            {/if}

            <div class="hotkey-actions">
              <span class="hotkey-hint">Click a shortcut to rebind it, then press the new key combo.</span>
              <button
                class="action-btn primary {hotkeySaved ? 'saved' : ''}"
                on:click={saveHotkeys}
              >
                {hotkeySaved ? '✓ Saved' : 'Apply Hotkeys'}
              </button>
            </div>
          </div>
        </div>
      {:else if activeTab === 'about'}
        <div class="about-card">
          <div class="about-header">
            <div class="icon-container">
               <img src="assets/tray-icon.png" alt="logo" class="app-icon" />
            </div>
            <h3>VolumeFlow v1.7.0</h3>
          </div>
          <p>
            Premium Windows Audio Control & Monitoring. Designed for professionals and power users.
          </p>
          <div class="stats">
            <div class="stat-item">
              <span class="stat-label">Audio Engine:</span>
              <span class="stat-value">V1.8.0 (Hardened)</span>
            </div>
            <div class="stat-item">
              <span class="stat-label">Platform:</span>
              <span class="stat-value">Electron + Svelte</span>
            </div>
          </div>
          <div class="about-footer">
            <button class="back-btn" on:click={() => activeTab = 'mixer'}>Back to Mixer</button>
            <div class="made-with">
              Made with <span style="color: #ff4444;">❤</span> for Sound
            </div>
          </div>
        </div>
      {/if}
    {/if}

    <!-- Footer Status Bar -->
    <footer>
      {#if duckingEnabled}
        <span class="status-badge ducking">Ducking Active</span>
      {/if}
      {#if boostActive}
        <span class="status-badge boost">Boost Active</span>
      {/if}
      {#if activeRecordings.size > 0}
        <span class="status-badge recording">Recording {activeRecordings.size} app(s)</span>
      {/if}
      <span class="spacer"></span>
      <span class="version">v1.9.0</span>
    </footer>
  </div>
</main>


<style>
  :root {
    --primary-color: #00f2ff;
    --primary-glow: rgba(0, 242, 255, 0.5);
    --bg-dark: #0a0a0c;
    --glass-bg: rgba(20, 20, 25, 0.7);
    --glass-border: rgba(255, 255, 255, 0.1);
    --card-bg: rgba(255, 255, 255, 0.03);
    --text-main: #ffffff;
    --text-dim: rgba(255, 255, 255, 0.5);
  }

  main {
    width: 100vw;
    height: 100vh;
    display: flex;
    justify-content: center;
    align-items: center;
    overflow: hidden;
    color: var(--text-main);
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
  }

  .glass-container {
    width: 100%;
    height: 100%;
    background: var(--glass-bg);
    backdrop-filter: blur(20px);
    border: 1px solid var(--glass-border);
    display: flex;
    flex-direction: column;
    box-shadow: 0 20px 50px rgba(0, 0, 0, 0.5);
  }

  .title-bar {
    height: 60px;
    padding: 0 20px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-bottom: 1px solid var(--glass-border);
    -webkit-app-region: drag;
  }

  .brand {
    display: flex;
    align-items: center;
    gap: 12px;
  }

  .logo {
    width: 24px;
    height: 24px;
    border: 2px solid var(--primary-color);
    border-radius: 6px;
    display: flex;
    justify-content: center;
    align-items: center;
    box-shadow: 0 0 10px var(--primary-glow);
  }

  .logo-inner {
    width: 10px;
    height: 10px;
    background: var(--primary-color);
    border-radius: 2px;
    animation: pulse 2s infinite;
  }

  @keyframes pulse {
    0% { transform: scale(1); opacity: 1; }
    50% { transform: scale(1.2); opacity: 0.5; }
    100% { transform: scale(1); opacity: 1; }
  }

  h1 {
    font-size: 1.1em;
    font-weight: 700;
    letter-spacing: 1px;
    margin: 0;
    background: linear-gradient(to right, #fff, var(--primary-color));
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
  }

  .window-controls {
    display: flex;
    gap: 8px;
    -webkit-app-region: no-drag;
  }

  .control-btn {
    background: transparent;
    border: none;
    color: var(--text-dim);
    width: 28px;
    height: 28px;
    border-radius: 6px;
    display: flex;
    justify-content: center;
    align-items: center;
    cursor: pointer;
    transition: all 0.2s;
  }

  .control-btn:hover {
    background: rgba(255, 255, 255, 0.1);
    color: white;
  }

  .control-btn.close:hover {
    background: #ff4444;
  }

  .master-control {
    padding: 25px 20px;
    background: linear-gradient(to bottom, rgba(0, 242, 255, 0.05), transparent);
  }

  .master-info {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 12px;
  }

  .master-info .label {
    font-size: 0.85em;
    font-weight: 600;
    color: var(--primary-color);
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }

  .master-info .value {
    font-size: 1.2em;
    font-weight: 700;
  }

  .tabs {
    display: flex;
    padding: 0 20px;
    gap: 20px;
    border-bottom: 1px solid var(--glass-border);
  }

  .tab {
    padding: 12px 0;
    background: transparent;
    border: none;
    color: var(--text-dim);
    font-size: 0.9em;
    font-weight: 600;
    cursor: pointer;
    position: relative;
    transition: color 0.3s;
  }

  .tab.active {
    color: var(--primary-color);
  }

  .tab.active::after {
    content: '';
    position: absolute;
    bottom: -1px;
    left: 0;
    width: 100%;
    height: 2px;
    background: var(--primary-color);
    box-shadow: 0 0 10px var(--primary-glow);
  }

  .content {
    flex: 1;
    overflow-y: auto;
    padding: 20px;
  }

  .search-bar {
    position: relative;
    margin-bottom: 15px;
    display: flex;
    align-items: center;
    background: rgba(255, 255, 255, 0.05);
    border-radius: 10px;
    padding: 0 12px;
    border: 1px solid var(--glass-border);
    transition: all 0.3s;
  }

  .search-bar:focus-within {
    border-color: var(--primary-color);
    box-shadow: 0 0 15px rgba(0, 242, 255, 0.1);
  }

  .search-icon {
    color: var(--text-dim);
    margin-right: 10px;
  }

  .search-bar input {
    flex: 1;
    background: transparent;
    border: none;
    color: white;
    height: 40px;
    font-size: 0.9em;
    outline: none;
  }

  .clear-search {
    background: transparent;
    border: none;
    color: var(--text-dim);
    font-size: 1.2em;
    cursor: pointer;
    padding: 0 5px;
  }

  .session-list {
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .empty-state {
    padding: 40px 20px;
    text-align: center;
    color: var(--text-dim);
    font-size: 0.9em;
    background: var(--card-bg);
    border-radius: 12px;
    border: 1px dashed var(--glass-border);
  }

  .session-card {
    background: var(--card-bg);
    border: 1px solid var(--glass-border);
    border-radius: 12px;
    padding: 15px;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  }

  .session-card:hover {
    background: rgba(255, 255, 255, 0.06);
    border-color: rgba(255, 255, 255, 0.2);
    transform: translateY(-2px);
  }

  .session-card.is-trigger {
    border-color: #ffaa00;
    background: rgba(255, 170, 0, 0.05);
  }

  .session-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 15px;
  }

  .session-info {
    display: flex;
    flex-direction: column;
    gap: 2px;
    overflow: hidden;
  }

  .session-info .name {
    font-weight: 600;
    font-size: 0.95em;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .session-info .pid {
    font-size: 0.7em;
    color: var(--text-dim);
    font-family: monospace;
  }

  .session-actions {
    display: flex;
    gap: 8px;
  }

  .action-btn {
    width: 32px;
    height: 32px;
    border-radius: 8px;
    background: rgba(255, 255, 255, 0.05);
    border: 1px solid var(--glass-border);
    color: var(--text-dim);
    display: flex;
    justify-content: center;
    align-items: center;
    cursor: pointer;
    transition: all 0.2s;
  }

  .action-btn:hover {
    background: rgba(255, 255, 255, 0.1);
    color: white;
  }

  .action-btn.recording {
    border-color: #ff4444;
    color: #ff4444;
    background: rgba(255, 68, 68, 0.1);
    box-shadow: 0 0 10px rgba(255, 68, 68, 0.3);
  }

  .record-dot {
    width: 10px;
    height: 10px;
    background: currentColor;
    border-radius: 50%;
  }

  .action-btn.recording .record-dot {
    animation: blink 1s infinite;
  }

  @keyframes blink {
    0% { opacity: 1; transform: scale(1); }
    50% { opacity: 0.5; transform: scale(0.8); }
    100% { opacity: 1; transform: scale(1); }
  }

  .action-btn.ducking {
    border-color: #ffaa00;
    color: #ffaa00;
    background: rgba(255, 170, 0, 0.1);
  }

  .slider-group {
    display: flex;
    align-items: center;
    gap: 12px;
  }

  .slider-container {
    flex: 1;
    height: 32px;
    position: relative;
    display: flex;
    align-items: center;
  }

  .peak-bg {
    position: absolute;
    top: 50%;
    left: 0;
    width: 100%;
    height: 4px;
    background: rgba(255, 255, 255, 0.05);
    border-radius: 2px;
    transform: translateY(-50%);
  }

  .peak-bar {
    position: absolute;
    top: 50%;
    left: 0;
    height: 4px;
    background: var(--primary-color);
    border-radius: 2px;
    transform: translateY(-50%);
    opacity: 0.3;
    transition: width 0.1s ease;
  }

  .peak-bar.master {
    background: var(--primary-color);
    opacity: 0.5;
  }

  input[type="range"] {
    position: relative;
    z-index: 2;
    -webkit-appearance: none;
    width: 100%;
    background: transparent;
    cursor: pointer;
  }

  input[type="range"]::-webkit-slider-runnable-track {
    width: 100%;
    height: 4px;
    background: transparent;
  }

  input[type="range"]::-webkit-slider-thumb {
    -webkit-appearance: none;
    height: 16px;
    width: 16px;
    border-radius: 50%;
    background: white;
    margin-top: -6px;
    box-shadow: 0 0 10px rgba(0,0,0,0.5), 0 0 5px var(--primary-glow);
    border: 2px solid var(--primary-color);
    transition: transform 0.2s;
  }

  input[type="range"]:active::-webkit-slider-thumb {
    transform: scale(1.2);
  }

  .mute-btn {
    background: transparent;
    border: none;
    color: var(--text-dim);
    cursor: pointer;
    display: flex;
    align-items: center;
    transition: color 0.2s;
  }

  .mute-btn:hover {
    color: white;
  }

  .mute-btn.muted {
    color: #ff4444;
  }

  .vol-text {
    width: 35px;
    font-size: 0.75em;
    font-weight: 700;
    text-align: right;
    color: var(--text-dim);
    font-family: monospace;
  }

  .settings-list {
    display: flex;
    flex-direction: column;
    gap: 15px;
  }

  .setting-item {
    background: var(--card-bg);
    border: 1px solid var(--glass-border);
    border-radius: 12px;
    padding: 15px;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .setting-info {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  .setting-info .title {
    font-weight: 600;
    font-size: 0.95em;
  }

  .setting-info .desc {
    font-size: 0.75em;
    color: var(--text-dim);
  }

  .toggle {
    width: 44px;
    height: 24px;
    background: rgba(255, 255, 255, 0.1);
    border-radius: 12px;
    border: 1px solid var(--glass-border);
    position: relative;
    cursor: pointer;
    transition: all 0.3s;
  }

  .toggle.on {
    background: var(--primary-color);
    border-color: var(--primary-color);
  }

  .handle {
    width: 18px;
    height: 18px;
    background: white;
    border-radius: 50%;
    position: absolute;
    top: 2px;
    left: 2px;
    transition: transform 0.3s;
    box-shadow: 0 2px 5px rgba(0,0,0,0.2);
  }

  .toggle.on .handle {
    transform: translateX(20px);
  }

  .action-btn.primary {
    background: var(--primary-color);
    color: #000;
    border: none;
    font-weight: 700;
    font-size: 0.8em;
    width: auto;
    padding: 0 16px;
    border-radius: 6px;
  }

  footer {
    height: 35px;
    background: rgba(0, 0, 0, 0.3);
    padding: 0 15px;
    display: flex;
    align-items: center;
    gap: 10px;
    font-size: 0.7em;
    border-top: 1px solid var(--glass-border);
  }

  .status-badge {
    padding: 2px 8px;
    border-radius: 4px;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }

  .status-badge.ducking { background: rgba(255, 170, 0, 0.2); color: #ffaa00; }
  .status-badge.boost { background: rgba(0, 242, 255, 0.2); color: var(--primary-color); }
  .status-badge.recording { background: rgba(255, 68, 68, 0.2); color: #ff4444; }

  .spacer { flex: 1; }
  .version { color: var(--text-dim); }

  /* About Styles */
  .about-card {
    background: var(--card-bg);
    border: 1px solid var(--glass-border);
    border-radius: 15px;
    padding: 20px;
    display: flex;
    flex-direction: column;
    gap: 15px;
  }

  .about-header {
    display: flex;
    align-items: center;
    gap: 12px;
  }

  .icon-container {
    width: 32px;
    height: 32px;
  }

  .app-icon {
    width: 100%;
    height: 100%;
    object-fit: contain;
  }

  .about-card h3 {
    margin: 0;
    font-size: 1.1em;
    color: var(--primary-color);
  }

  .about-card p {
    font-size: 0.85em;
    line-height: 1.6;
    color: var(--text-dim);
    margin: 0;
  }

  .stats {
    background: rgba(0, 0, 0, 0.2);
    border-radius: 10px;
    padding: 12px;
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  .stat-item {
    display: flex;
    justify-content: space-between;
    font-size: 0.75em;
  }

  .stat-label { color: var(--text-dim); }
  .stat-value { font-weight: 700; color: var(--primary-color); }

  .about-footer {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: 5px;
  }

  .back-btn {
    background: rgba(255, 255, 255, 0.05);
    border: 1px solid var(--glass-border);
    color: white;
    padding: 6px 15px;
    border-radius: 6px;
    font-size: 0.8em;
    cursor: pointer;
    transition: all 0.2s;
  }

  .back-btn:hover { background: rgba(255, 255, 255, 0.1); }

  .made-with { font-size: 0.75em; color: var(--text-dim); }

  /* ---- Global Hotkeys ---- */
  .hotkey-section {
    margin-top: 12px;
    background: var(--card-bg);
    border: 1px solid var(--glass-border);
    border-radius: 12px;
    padding: 14px;
    display: flex;
    flex-direction: column;
    gap: 10px;
  }

  .hotkey-section-header {
    display: flex;
    align-items: center;
    gap: 7px;
    font-size: 0.75em;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 1px;
    color: var(--primary-color);
    margin-bottom: 2px;
  }

  .hotkey-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
  }

  .hotkey-label {
    font-size: 0.82em;
    color: var(--text-main);
    flex: 1;
  }

  .hotkey-badge {
    background: rgba(0, 242, 255, 0.08);
    border: 1px solid rgba(0, 242, 255, 0.25);
    color: var(--primary-color);
    border-radius: 6px;
    padding: 4px 10px;
    font-size: 0.78em;
    font-weight: 700;
    font-family: monospace;
    cursor: pointer;
    white-space: nowrap;
    transition: background 0.2s, border-color 0.2s;
    letter-spacing: 0.5px;
  }

  .hotkey-badge:hover {
    background: rgba(0, 242, 255, 0.18);
    border-color: var(--primary-color);
  }

  .hotkey-capture {
    background: rgba(0, 242, 255, 0.05);
    border: 1px dashed var(--primary-color);
    border-radius: 6px;
    padding: 4px 12px;
    font-size: 0.78em;
    color: var(--text-dim);
    font-style: italic;
    cursor: wait;
    outline: none;
    animation: blink-border 1s infinite;
  }

  @keyframes blink-border {
    0%, 100% { border-color: var(--primary-color); }
    50%       { border-color: transparent; }
  }

  .hotkey-error {
    font-size: 0.75em;
    color: #ff4444;
    margin: 0;
  }

  .hotkey-actions {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
    margin-top: 4px;
  }

  .hotkey-hint {
    font-size: 0.72em;
    color: var(--text-dim);
    line-height: 1.4;
    flex: 1;
  }

  .action-btn.primary.saved {
    background: rgba(0, 200, 100, 0.8);
    color: #000;
  }

  /* ---- Advanced Audio Control ---- */
  .advanced-row {
    display: flex;
    flex-direction: column;
    gap: 5px;
    margin-bottom: 8px;
  }

  .advanced-label {
    font-size: 0.78em;
    color: var(--text-dim);
  }

  .advanced-input-group {
    display: flex;
    align-items: center;
    gap: 10px;
  }

  .advanced-input-group input[type="range"] {
    flex: 1;
    height: 4px;
  }

  .advanced-value {
    font-size: 0.78em;
    font-family: monospace;
    color: var(--primary-color);
    width: 45px;
    text-align: right;
  }

  /* ---- Mini-Player Styles ---- */
  .mini-mode {
    background: transparent !important;
  }

  .mini-container {
    width: 240px !important;
    height: 350px !important;
    overflow: hidden;
    padding: 0 !important;
    border-radius: 12px;
  }

  .mini-drag {
    -webkit-app-region: drag;
    padding: 10px 15px !important;
    border-bottom: 1px solid var(--glass-border);
  }

  .mini-drag * {
    -webkit-app-region: no-drag;
  }

  .mini-btn {
    opacity: 0.7;
  }
  .mini-btn:hover {
    opacity: 1;
    color: var(--primary-color);
  }

  .mini-content {
    padding: 15px;
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .mini-section {
    display: flex;
    flex-direction: column;
    gap: 6px;
  }

  .mini-header {
    font-size: 0.65em;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    color: var(--primary-color);
    font-weight: 700;
  }

  .mini-control-row {
    display: flex;
    align-items: center;
    gap: 8px;
  }

  .mini-slider {
    flex: 1;
  }

  .mini-slider input {
    height: 5px;
  }

  .mini-val {
    font-size: 0.75em;
    font-family: monospace;
    width: 30px;
    text-align: right;
  }

  .mini-session-row {
    display: flex;
    flex-direction: column;
    gap: 2px;
    background: rgba(255, 255, 255, 0.03);
    padding: 6px 8px;
    border-radius: 6px;
    border: 1px solid var(--glass-border);
  }

  .mini-app-name {
    font-size: 0.72em;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    color: var(--text-main);
  }

  .mini-slider-small input {
    height: 3px;
  }

  .mute-btn.small {
    width: 24px;
    height: 24px;
    padding: 0;
    display: flex;
    align-items: center;
    justify-content: center;
  }
</style>


