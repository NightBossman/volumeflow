import { vitePreprocess } from '@sveltejs/vite-plugin-svelte';

// NOTE: previously this file set `compilerOptions.runes: true`. That is
// only meaningful under Svelte 5 (it forces every component into runes
// mode). The pinned svelte version in package.json is `^4.0.5`, so under
// the version actually installed the flag is silently ignored — and if
// the toolchain were ever bumped to Svelte 5 the flag would have
// instantly broken App.svelte (which uses legacy `let`/`$:` reactivity).
// Removed by Claude (Anthropic) model `claude-opus-4-7`. Components that
// want runes can opt in individually via `<svelte:options runes={true} />`.
export default {
  preprocess: vitePreprocess(),
};
