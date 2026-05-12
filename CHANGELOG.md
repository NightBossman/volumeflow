# Changelog - VolumeFlow

Wszystkie istotne zmiany w tym projekcie będą dokumentowane w tym pliku.

## [1.9.1] - 2026-05-12 — Hotfix audit (Claude Opus 4.7)
Wpis dodany przez asystenta Claude (Anthropic), model `claude-opus-4-7`,
działającego na branchu `claude/audit-volumeflow-performance-p3a2V`.
Ten release zawiera WYŁĄCZNIE poprawki istniejących regresji — bez nowych
funkcji. Skopiowane tu, żebyś (NightBosman / Antigravity) miał czytelny
ślad każdej zmiany.

### Naprawiono (krytyczne)
- **`src/main.js` przywrócony jako entrypoint renderera Svelte.**
  W commicie `e6855ee` (v1.7.0) plik został przypadkowo nadpisany
  zduplikowanym kodem procesu Electron *main* (require('electron'),
  BrowserWindow itd.). Wskutek tego Vite usiłował zbundlować Node-API
  do renderera, komponent `App.svelte` nigdy nie był montowany do
  `<div id="app">` i całe UI nie startowało. Plik ma znów jedno
  zadanie: zamontować `App.svelte` (defensywnie pod Svelte 4 i 5).
- **`App.svelte` — przepisana warstwa IPC.** Renderer wołał kanały,
  które po hardeningu IPC v1.5+ już nie istnieją (`bridge-command`,
  `bridge-response`, `bridge-peaks`, `minimize-app`) oraz API
  niewystawiane przez `preload.cjs` (`sendSync`, `removeAllListeners`).
  Efekt: lista sesji nigdy się nie wypełniała, peak meters były
  zamrożone, przycisk minimalizacji nie reagował, zapis hotkeyów
  rzucał wyjątek. Renderer korzysta teraz z `invoke('get-audio-sessions')`,
  `invoke('get-master-info')`, strumienia `audio-peaks`, oraz unsubscribe
  closures zwracanych przez `ipcRenderer.on(...)`.
- **`preload.cjs` — uzupełniona whitelista kanałów.** Brakowało
  `show-osd`, `open-recordings` (send) oraz `save-hotkeys` (invoke).
  Bez nich powyższe akcje były po cichu odrzucane.
- **`save-hotkeys` przeniesione na `ipcMain.handle` (async).** Stara
  wersja używała `event.returnValue` (sendSync), co blokowało wątek
  renderera podczas zapisu do dysku i wymagało nieistniejącego
  `sendSync` w preloadzie. Teraz: czysty async + await + invoke.
- **`master` poll dodany do interwału 1 Hz.** Wcześniej `refreshMaster`
  był wołany jednorazowo na mount, więc zewnętrzne zmiany (mixer
  Windows, scenariusze automation) nie odbijały się w UI.

### Naprawiono (UX / wydajność)
- **`showOSD` race condition**: kolejne wywołanie nie czyściło
  poprzedniego `setTimeout(hide)`, więc świeżo pokazane powiadomienie
  bywało zamykane przez stary timer. Timer jest teraz idempotentny.
- **`showOSD` early-IPC race**: jeśli okno OSD jest jeszcze w trakcie
  `loadFile`, `webContents.send('show-osd', …)` nie dochodziło do
  renderera. Komunikat jest teraz odraczany do `did-finish-load`.
- **`svelte.config.js` — usunięty `compilerOptions.runes: true`.**
  Pod Svelte 4 (wersja z `package.json`) flaga jest no-opem; pod
  Svelte 5 wymusiła by tryb runes na całym `App.svelte` (który używa
  legacy syntax `let`/`$:`) i wyłożyła reaktywność. Komponenty
  korzystające z runes (np. `src/lib/VolumeSlider.svelte`) mogą się
  opt-inować lokalnie przez `<svelte:options runes={true} />`.
- **Tray label** czytany z `app.getVersion()` zamiast wpisanego na
  sztywno `v1.6.0` (aktualnie i tak zostaje `v1.9.0+`).

### Nieruszone (świadomie)
- `AudioBridge.cs` przejrzany — COM cleanup, drop policy dla peak
  frames, zombie PID GC i obsługa stdout backpressure są zdrowe.
  Brak zmian.
- `src/lib/VolumeSlider.svelte` — kod sierota (nigdzie nie importowany).
  Używa runes Svelte 5; Vite go nie kompiluje, więc zostaje
  nienaruszony jako "wishlist" gdybyś chciał refaktor na Svelte 5.

---
## [1.9.0] - 2026-05-12
### Dodano
- **Mini-Player Mode**: Kompaktowy, zawsze widoczny widget (240x350px) do szybkiego sterowania głośnością systemu i najaktywniejszych aplikacji.
- **Advanced Audio Settings**: Nowa sekcja w ustawieniach pozwalająca na precyzyjną konfigurację:
  - Próg czułości duckingu (Threshold).
  - Siła wyciszenia tła (Ducking Factor).
  - Czas trwania przejść głośności (Fade Duration).
- **Przełącznik trybu Mini**: Nowy przycisk w nagłówku aplikacji do błyskawicznego przełączania między pełnym oknem a widgetem.

### Ulepszono
- **Bezpieczeństwo IPC**: Uszczelnienie komunikacji w `preload.cjs` i przejście na pełną izolację kontekstu (Context Isolation).
- **Persystencja**: Wszystkie zaawansowane parametry audio są zapisywane w `config.json`.
- **Integracja Audio**: Ustawienia duckingu i fade'owania są teraz dynamicznie wstrzykiwane do komend mostka audio.

---
## [1.8.1] - 2026-05-12
### Dodano
- **Global Hotkeys**: System skrótów klawiszowych działających w całym systemie (niezależnie od fokusa okna).
  - `Ctrl+Alt+R` — Rozpocznij / Zatrzymaj nagrywanie aktywnej sesji audio.
  - `Ctrl+Alt+M` — Wycisz / Odcisz wyjście master systemu.
  - `Ctrl+Alt+B` — Włącz / Wyłącz Smart Overdrive.
- **Edytor hotkeys w UI**: Nowa sekcja "Global Hotkeys" w zakładce Settings pozwala na rebinding skrótów bez restartowania aplikacji.
- **OSD przy hotkey**: Każde naciśnięcie globalnego skrótu wyświetla powiadomienie OSD z informacją o akcji.
- **Synchronizacja stanu**: Zmiany wywołane hotkey (nagrywanie, boost) są natychmiast odzwierciedlane w interfejsie.

### Ulepszono
- Skróty są persystowane w `config.json` i ładowane przy starcie.
- Walidacja bezpieczeństwa nowo przypisanych skrótów po stronie procesu głównego.

---
## [1.7.0] - 2026-05-11
### Dodano
- **System OSD (On-Screen Display)**: Nowoczesne powiadomienia w rogu ekranu informujące o rozpoczęciu i zakończeniu nagrywania.
- **Procesy Search**: Dynamiczna wyszukiwarka w czasie rzeczywistym pozwalająca na błyskawiczne odnalezienie konkretnej aplikacji w mikserze.
- **Health Monitoring**: Automatyczne monitorowanie wydajności mostka audio (dropped peaks) z logowaniem ostrzeżeń w konsoli.
- **AudioBridge V1.8.0**: Gruntowna przebudowa backendu (audyt bezpieczeństwa COM, obsługa backpressure, event-driven recording).

### Ulepszono
- **Audio Logic**: Poprawiony mechanizm synchronizacji wolumenu przy włączonym Smart Overdrive.
- **UI Performance**: Optymalizacja list procesów przy dużej liczbie aktywnych sesji audio.
- **UX**: Dodano powiadomienie OSD przy zatrzymaniu nagrywania.

### Naprawiono
- Krytyczny błąd blokowania potoku stdout przy dużym natężeniu danych peak.
- Wycieki pamięci związane z nieprawidłowym zwalnianiem obiektów COM w backendzie C#.
- Problem z pustym folderem nagrań (poprawiona ścieżka zapisu i finalizacja nagłówka WAV).

---
## [1.6.0] - 2026-05-04
### Dodano
- **Smart Overdrive (Boost)**: Inteligentna kompresja dynamiczna pozwalająca na podbicie głośności bez przesterowań.
- **Recording Engine**: Możliwość nagrywania dźwięku bezpośrednio z wybranych procesów do plików .wav.
- **Integracja systemowa**: Dodanie przycisku otwierającego folder z nagraniami.

---
## [1.5.1] - 2026-05-03
### Naprawiono
- Poprawki stabilności Auto-Ducking w środowiskach wielomonitorowych.
- Hardening komunikacji IPC między Electronem a AudioBridge.
