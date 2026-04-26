<script>
  import './app.css';
  import VolumeSlider from './lib/VolumeSlider.svelte';
  import { onMount } from 'svelte';
  import { X, Maximize2, Minimize2, Sun, Moon, Users, Settings, Activity } from 'lucide-svelte';

  const { ipcRenderer } = window.require('electron');

  let isExpanded = false;
  let currentTheme = 'midnight';
  let eyeSaver = false;
  let masterVolume = 75;
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
        masterId = master.id;
      }
    } catch (e) { console.error(e); }
  }

  function setTheme(themeId) {
    currentTheme = themeId;
    document.body.setAttribute('data-theme', themeId);
  }

  function toggleMode() {
    isExpanded = !isExpanded;
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

  function closeApp() { ipcRenderer.send('close-app'); }

  onMount(() => {
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
        {#if isExpanded}<Minimize2 size={18} />{:else}<Maximize2 size={18} />{/if}
      </button>
      <button onclick={closeApp} class="icon-btn close" title="Zamknij"><X size={20} /></button>
    </div>
  </header>

  <section class="master-section">
    <VolumeSlider label="Głośność Główna" bind:value={masterVolume} isMaster={true} onchange={() => handleMasterChange(masterVolume)} />
  </section>

  <div class="separator"></div>

  <section class="process-list">
    <div class="section-title">{isExpanded ? 'Aktywne Procesy' : 'Najczęstsze'}</div>
    {#each isExpanded ? processes : processes.slice(0, 2) as process}
      <VolumeSlider label={process.name} bind:value={process.volume} onchange={() => handleVolumeChange(process.id, process.volume)} />
    {/each}

    {#if isExpanded}
      <div class="advanced-section">
        <div class="section-title">Personalizacja</div>
        <div class="theme-row">
          <div class="theme-selector">
            {#each themes as theme}
              <button class="theme-dot" class:active={currentTheme === theme.id} style="background: {theme.color}" onclick={() => setTheme(theme.id)}></button>
            {/each}
          </div>
          <button class="eye-saver-toggle" class:active={eyeSaver} onclick={() => eyeSaver = !eyeSaver}>
            {#if eyeSaver}<Moon size={14} /> <span>Eye Saver: ON</span>{:else}<Sun size={14} /> <span>Eye Saver: OFF</span>{/if}
          </button>
        </div>
        <div class="section-title" style="margin-top: 24px">Zarządzanie</div>
        <div class="advanced-options">
          <button class="advanced-btn"><Users size={16} /> <span>Zarządzaj Grupami</span></button>
          <button class="advanced-btn"><Settings size={16} /> <span>Ustawienia Audio</span></button>
        </div>
      </div>
    {/if}
  </section>
  <footer>Vibe: {themes.find(t => t.id === currentTheme).name} | {eyeSaver ? 'Protection' : 'Standard'}</footer>
</main>

<style>
  main { display: flex; flex-direction: column; height: 100%; box-sizing: border-box; transition: filter 0.5s ease; }
  header { height: 44px; display: flex; justify-content: space-between; align-items: center; padding: 0 16px; background: rgba(255, 255, 255, 0.05); }
  .title-group { display: flex; align-items: center; gap: 8px; }
  .title { font-weight: 700; font-size: 0.8em; letter-spacing: 1px; opacity: 0.9; text-transform: uppercase; }
  .controls { display: flex; gap: 12px; align-items: center; }
  .icon-btn { background: transparent; border: none; color: white; padding: 4px; cursor: pointer; opacity: 0.6; transition: all 0.2s; display: flex; align-items: center; justify-content: center; }
  .icon-btn:hover { opacity: 1; transform: scale(1.1); }
  .icon-btn.close:hover { color: #ff4444; }
  .master-section { padding: 20px 16px; }
  .separator { height: 1px; background: var(--glass-border); margin: 0 16px; }
  .section-title { font-size: 0.65em; text-transform: uppercase; letter-spacing: 1.5px; color: rgba(255, 255, 255, 0.4); margin-bottom: 14px; padding-left: 4px; font-weight: 700; }
  .process-list { flex: 1; overflow-y: auto; padding: 16px; }
  .advanced-section { margin-top: 24px; padding: 20px 16px; background: rgba(255, 255, 255, 0.02); border-radius: 12px; border: 1px solid var(--glass-border); }
  .theme-row { display: flex; justify-content: space-between; align-items: center; margin-top: 8px; }
  .theme-selector { display: flex; gap: 12px; }
  .theme-dot { width: 20px; height: 20px; border-radius: 50%; border: 2px solid transparent; cursor: pointer; transition: transform 0.2s; }
  .theme-dot.active { border-color: white; box-shadow: 0 0 10px rgba(255, 255, 255, 0.3); }
  .eye-saver-toggle { background: rgba(255, 255, 255, 0.05); border: 1px solid var(--glass-border); color: white; padding: 8px 14px; border-radius: 20px; font-size: 0.7em; cursor: pointer; transition: all 0.2s; font-weight: 600; display: flex; align-items: center; gap: 6px; }
  .eye-saver-toggle.active { background: #ffcc00; color: black; border-color: #ffcc00; }
  .advanced-options { margin-top: 12px; display: flex; flex-direction: column; gap: 8px; }
  .advanced-btn { background: rgba(255, 255, 255, 0.05); border: 1px solid var(--glass-border); color: white; padding: 12px; border-radius: 8px; font-size: 0.8em; cursor: pointer; transition: all 0.2s; display: flex; align-items: center; gap: 10px; }
  .advanced-btn:hover { background: var(--glass-border); padding-left: 16px; }
  footer { height: 30px; background: rgba(0, 0, 0, 0.3); display: flex; align-items: center; padding: 0 16px; font-size: 0.7em; color: rgba(255, 255, 255, 0.3); letter-spacing: 0.5px; }
</style>