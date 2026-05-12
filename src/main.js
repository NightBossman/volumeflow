// ============================================================
// Svelte renderer entry point.
//
// IMPORTANT — Regression fix by Claude (Anthropic) — model
// `claude-opus-4-7` (Opus 4.7), commit on branch
// `claude/audit-volumeflow-performance-p3a2V`.
//
// In the previous repository state this file had been overwritten
// with a stale copy of the Electron *main* process code (require('electron'),
// BrowserWindow, spawn, …). That meant Vite was being asked to bundle
// Node-only modules into the renderer, the Svelte App was never mounted
// and the entire UI was broken. We restore the original responsibility
// of `src/main.js`: bootstrap the Svelte component into <div id="app">.
//
// The mount logic is defensive — it works under both Svelte 4
// (the version pinned in package.json) and Svelte 5 (which removed
// the `new App({ target })` constructor in favour of `mount()`).
// ============================================================

import * as svelte from 'svelte';
import App from './App.svelte';

const target = document.getElementById('app');
if (!target) {
  throw new Error('VolumeFlow: root element #app not found in index.html');
}

const app = typeof svelte.mount === 'function'
  ? svelte.mount(App, { target })
  : new App({ target });

export default app;
