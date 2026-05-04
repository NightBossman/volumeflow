<script>
  import { Volume2, VolumeX, AppWindow } from '@lucide/svelte';
  let { 
    value = $bindable(50), 
    label = "Aplikacja", 
    isMaster = false, 
    muted = false,
    peak = 0,
    onchange,
    onmute,
    icon
  } = $props();

  function handleInput(e) {
    value = parseInt(e.target.value);
    if (onchange) onchange();
  }

  function toggleMute() {
    if (onmute) onmute();
  }

  let isOverdrive = $derived(value > 100);
</script>

<div class="slider-container" class:muted>
  <div class="header">
    <div class="label-group">
      <button class="mute-btn" onclick={toggleMute} title={muted ? "Odwycisz" : "Wycisz"}>
        {#if muted}
          <VolumeX size={16} strokeWidth={2.5} color="#ff4444" />
        {:else if icon}
          {@render icon()}
        {:else if isMaster}
          <Volume2 size={16} strokeWidth={2.5} color={isOverdrive ? '#ff4d00' : 'var(--primary-color)'} />
        {:else}
          <AppWindow size={16} strokeWidth={2} color="var(--text-secondary)" />
        {/if}
      </button>
      <span class="label">{label}</span>
    </div>
    <span class="value" class:overdrive={isOverdrive}>{muted ? 'Muted' : value + '%'}</span>
  </div>
  
  <div class="input-wrapper">
    <div class="peak-glow" style="width: {peak * 100}%" class:is-muted={muted}></div>
    <input 
      type="range" 
      min="0" 
      max="150" 
      {value} 
      oninput={handleInput}
      disabled={muted}
      class:master={isMaster}
      class:overdrive={isOverdrive}
    />
    <div class="track-fill" style="width: {(value / 150) * 100}%" class:overdrive={isOverdrive} class:is-muted={muted}></div>
    <div class="peak-bar" style="width: {peak * 100}%" class:is-muted={muted}></div>
  </div>
</div>

<style>
  .slider-container {
    padding: 12px;
    background: rgba(255, 255, 255, 0.03);
    border-radius: 8px;
    margin-bottom: 8px;
    transition: all 0.2s ease;
  }

  .slider-container:hover {
    background: rgba(255, 255, 255, 0.06);
  }

  .slider-container.muted {
    opacity: 0.6;
  }

  .header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 10px;
    font-size: 0.9em;
  }

  .label-group {
    display: flex;
    align-items: center;
    gap: 8px;
  }

  .mute-btn {
    background: transparent;
    border: none;
    padding: 0;
    cursor: pointer;
    display: flex;
    align-items: center;
    transition: transform 0.1s;
  }

  .mute-btn:hover {
    transform: scale(1.2);
  }

  .label {
    color: rgba(255, 255, 255, 0.8);
    font-weight: 500;
  }

  .value {
    color: var(--primary-color);
    font-weight: bold;
    min-width: 40px;
    text-align: right;
  }

  .value.overdrive {
    color: #ff4d00;
  }

  .input-wrapper {
    position: relative;
    height: 6px;
    display: flex;
    align-items: center;
  }

  input[type="range"] {
    -webkit-appearance: none;
    width: 100%;
    height: 6px;
    background: rgba(255, 255, 255, 0.1);
    border-radius: 3px;
    outline: none;
    position: relative;
    z-index: 2;
  }

  input[type="range"]:disabled {
    cursor: not-allowed;
  }

  input[type="range"]::-webkit-slider-thumb {
    -webkit-appearance: none;
    width: 14px;
    height: 14px;
    background: #fff;
    border-radius: 50%;
    cursor: pointer;
    box-shadow: 0 0 8px rgba(0, 0, 0, 0.5);
    border: 2px solid var(--primary-color);
    transition: transform 0.1s ease;
  }

  input[type="range"]:disabled::-webkit-slider-thumb {
    background: #444;
    border-color: #666;
  }

  .track-fill {
    position: absolute;
    top: 0;
    left: 0;
    height: 100%;
    background: linear-gradient(90deg, var(--primary-color), var(--accent-color));
    border-radius: 3px;
    z-index: 1;
    pointer-events: none;
  }

  .track-fill.is-muted {
    background: #444;
  }

  .track-fill.overdrive {
    background: linear-gradient(90deg, var(--primary-color), #ff4d00);
  }

  .peak-bar {
    position: absolute;
    bottom: -6px;
    left: 0;
    height: 2px;
    background: var(--primary-color);
    box-shadow: 0 0 8px var(--primary-color);
    border-radius: 2px;
    pointer-events: none;
    transition: width 0.05s ease-out;
    opacity: 0.8;
    z-index: 3;
  }

  .peak-glow {
    position: absolute;
    top: -50%;
    left: 0;
    height: 200%;
    background: var(--primary-color);
    opacity: 0.15;
    filter: blur(12px);
    border-radius: 50%;
    pointer-events: none;
    transition: width 0.05s ease-out;
    z-index: 0;
  }

  .peak-bar.is-muted, .peak-glow.is-muted {
    display: none;
  }
</style>
