import { mount } from 'svelte';
import App from './App.svelte';

const target = document.getElementById('app');

if (!target) {
  throw new Error('Root element #app not found. Check if index.html has id="app" and script is loaded correctly.');
}

const app = mount(App, { target });

export default app;
