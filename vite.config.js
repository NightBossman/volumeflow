import { defineConfig } from 'vite';
import { svelte } from '@sveltejs/vite-plugin-svelte';

export default defineConfig(({ mode }) => ({
  plugins: [svelte()],
  base: './',
  clearScreen: false,
  build: {
    outDir: 'dist',
    emptyOutDir: true,
    target: 'chrome146',
    minify: 'esbuild',
    sourcemap: mode === 'development',
  },
  server: {
    port: 5173,
    strictPort: true,
  },
}));
