<script>
  import { Volume2, AppWindow } from 'lucide-svelte';
  let { value = $bindable(50), label = "Aplikacja", isMaster = false, onchange } = $props();
  function handleInput(e) { value = parseInt(e.target.value); if (onchange) onchange(); }
  let isOverdrive = $derived(value > 100);
</script>
<div class="slider-container">
  <div class="header">
    <div class="label-group">
      {#if isMaster}<Volume2 size={16} strokeWidth={2.5} color={isOverdrive ? '#ff4d00' : 'var(--primary-color)'} />{:else}<AppWindow size={16} strokeWidth={2} color="var(--text-secondary)" />{/if}
      <span class="label">{label}</span>
    </div>
    <span class="value" class:overdrive={isOverdrive}>{value}%</span>
  </div>
  <div class="input-wrapper">
    <input type="range" min="0" max="150" {value} oninput={handleInput} class:master={isMaster} class:overdrive={isOverdrive} />
    <div class="track-fill" style="width: {(value / 150) * 100}%" class:overdrive={isOverdrive}></div>
  </div>
</div>
<style>
  .slider-container { padding: 12px; background: rgba(255, 255, 255, 0.03); border-radius: 8px; margin-bottom: 8px; transition: all 0.2s ease; }
  .slider-container:hover { background: rgba(255, 255, 255, 0.06); }
  .header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px; font-size: 0.9em; }
  .label-group { display: flex; align-items: center; gap: 8px; }
  .label { color: rgba(255, 255, 255, 0.8); font-weight: 500; }
  .value { color: var(--primary-color); font-weight: bold; min-width: 40px; text-align: right; }
  .value.overdrive { color: #ff4d00; }
  .input-wrapper { position: relative; height: 6px; display: flex; align-items: center; }
  input[type="range"] { -webkit-appearance: none; width: 100%; height: 6px; background: rgba(255, 255, 255, 0.1); border-radius: 3px; outline: none; position: relative; z-index: 2; }
  input[type="range"]::-webkit-slider-thumb { -webkit-appearance: none; width: 14px; height: 14px; background: #fff; border-radius: 50%; cursor: pointer; box-shadow: 0 0 8px rgba(0, 0, 0, 0.5); border: 2px solid var(--primary-color); transition: transform 0.1s ease; }
  input[type="range"]:active::-webkit-slider-thumb { transform: scale(1.3); }
  input[type="range"].overdrive::-webkit-slider-thumb { border-color: #ff4d00; }
  .track-fill { position: absolute; top: 0; left: 0; height: 100%; background: linear-gradient(90deg, var(--primary-color), var(--accent-color)); border-radius: 3px; z-index: 1; pointer-events: none; }
  .track-fill.overdrive { background: linear-gradient(90deg, var(--primary-color), #ff4d00); }
</style>