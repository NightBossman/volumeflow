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

### Naprawiono (recording feature) — drugi commit hotfixa
- **Per-app Audio Recording: ujednolicona ścieżka `Recordings/`.**
  `AudioBridge.cs` zapisuje pliki .wav do
  `AppDomain.CurrentDomain.BaseDirectory + "Recordings/"` (obok
  AudioBridge.exe). Stary handler `open-recordings` w `main.js`
  otwierał `path.join(__dirname, 'Recordings')`. W trybie deweloperskim
  obie ścieżki się pokrywały (oba w `<project>/`), ale w buildzie
  packaged rozjeżdżały się: bridge pisał do
  `<install>/resources/app.asar.unpacked/Recordings/`, a przycisk
  "Open Folder" próbował otworzyć
  `<install>/resources/app.asar/Recordings/` — czyli ścieżkę
  **wewnątrz wirtualnego asar**, więc `shell.openPath` cicho zawodził.
  Z perspektywy użytkownika: nagrania były tworzone, ale folder był
  pusty / nie do otwarcia, więc cała funkcjonalność per-app recording
  sprawiała wrażenie zepsutej. Fix wylicza katalog z `path.dirname`
  bridge'a, więc obie strony zawsze patrzą w to samo miejsce — bez
  konieczności rekompilacji `AudioBridge.exe`. Dodatkowo `mkdirSync`
  używa teraz `recursive: true` i logujemy ewentualne błędy
  `shell.openPath`.

### AudioBridge.cs — perf pass + bug fix (trzeci commit hotfixa)
- **BUG: `toggle_master_mute` nigdy nie był obsłużony przez bridge.**
  Od v1.8.1 `main.js` wysyła `action: 'toggle_master_mute'` z hotkey'a
  `Ctrl+Alt+M` i z `toggle-session-mute` na id=`'master'`, ale w
  AudioBridge.cs nigdy nie było `case "toggle_master_mute"` — leciało
  do `default → error_unknown_action`. Wyciszenie mastera z UI lub
  hotkey'a po prostu nic nie robiło. Dodany `HandleToggleMasterMute`
  korzystający ze współdzielonego cached `IAudioEndpointVolume`.

- **Współdzielone cache COM dla handlerów.** Każdy `HandleSet*` /
  `HandleToggle*` / `HandleGetMaster` robił dotychczas pełne
  `new MMDeviceEnumeratorComObject() → GetDefaultAudioEndpoint →
  Activate(manager/endpoint vol)` na każdym wywołaniu. Podczas
  przeciągania suwaka (~60 IPC zdarzeń/sek) to było ~1-2 ms COM tax
  × 60 = 6-12% rdzenia tylko na re-aktywację. Teraz:
  - `sharedDeviceEnum` — singleton `MMDeviceEnumerator`
  - `sharedSessionManager` — refresh w `SwitchToDevice` (na zmianę
    domyślnego urządzenia, rzadkie)
  - `sharedEndpointVolume` — j.w. dla master vol/mute
  Wszystkie z lazy-init na pierwsze wywołanie (na wypadek gdy
  handler trafi do bridge'a przed pierwszym cyklem peak loopu).

- **"Published session list" dla handlerów per-PID.** `PeakPollingLoop`
  i tak co 100 ms enumeruje wszystkie sesje i pobiera ich
  `IAudioSessionControl`-e. Te RCW są teraz publikowane do
  `publishedSessions` na końcu cyklu. `HandleSetVolume`,
  `HandleToggleMute` i `HandleSetBoost` szukają PID-a w tej liście
  (O(N) memory lookup) zamiast robić własne COM enumeracje sesji.
  Lifecycle bezpieczny przez `publishedSessionsLock` — RCW
  z poprzedniego cyklu są zwalniane DOPIERO po swapie, więc czytający
  handler zawsze widzi spójną listę. Fallback "slow path" robi
  prawdziwą enumerację gdy PID jeszcze nie dotarł do listy (np.
  sesja właśnie się pojawiła).

- **`PeakPollingLoop`: throttle device-pollingu z 10 Hz na 1 Hz.**
  `GetDefaultAudioEndpoint` był wołany na każdy peak tick (10×/sek)
  do porównania `GetId` z poprzednim — czyli 10 COM round-tripów/sek
  dla atrybutu który zmienia się rzadko. Teraz check raz na sekundę
  (lub od razu gdy `bestManager == null` przy starcie).

- **`PeakPollingLoop`: jeden przebieg po sesjach zamiast dwóch.**
  Stara wersja przy włączonym duckingu robiła wstępny pre-scan
  (`for s < sessionCount: GetSession, GetProcessId, GetPeakValue`)
  szukając trigger PID-a, a potem drugi pełny scan. Teraz jeden
  scan zbiera (pid, peak, control) i przy okazji łapie peak trigger
  PID-a — eliminuje ~N COM calls per cykl gdy ducking jest on.

- **`PeakPollingLoop`: reuse `StringBuilder`-ów + scratch buffers.**
  Stara wersja alokowała `new StringBuilder(96)` per sesja per
  cykl + `List<string>` + `string.Join` + `new StringBuilder(128 +
  N*32)` per cykl. Teraz dwa `static readonly StringBuilder` (peak
  + sessions JSON) z `.Clear()` na początku cyklu, plus
  `int[]/float[]/IAudioSessionControl[]` scratch arrays z `Array.Resize`
  na żądanie. Mniej presji na GC.

- **`CleanupZombiePids` bez wyjątków per-PID.** Stara wersja wołała
  `Process.GetProcessById(pid)` dla każdego PID-a w cache — gdy
  proces nie istniał, leciał `ArgumentException` (~50-100 µs per
  wyjątek ze względu na stack-walk + alokację). Dla 20 zombie
  PID-ów to dziesiątki ms co cleanup tick. Zamiana na jeden
  `Process.GetProcesses()` snapshot + `HashSet.Contains` —
  O(P + N) bez wyjątków.

- **Mikro: `Dictionary.ContainsKey + indexer` → `TryGetValue`**
  w gorących ścieżkach (peak loop, HandleSet*). Dwie operacje hash
  → jedna.

### Nieruszone (świadomie)
- `RecordingSession.RecordLoop` w AudioBridge.cs — event-driven +
  timer-driven fallback, `WAVEFORMATEXTENSIBLE` przez `IntPtr`,
  poprawny WAV header dla PCM / IEEE float / extensible. Logika
  jest zdrowa, nie tykam.
- `src/lib/VolumeSlider.svelte` — kod sierota (nigdzie nie importowany).
  Używa runes Svelte 5; Vite go nie kompiluje, więc zostaje
  nienaruszony jako "wishlist" gdybyś chciał refaktor na Svelte 5.

> ⚠️ **Note for the maintainer**: `AudioBridge.cs` to kod natywny C#,
> więc żeby te zmiany weszły do działającego buildu trzeba
> zrecompilować `AudioBridge.exe` (np. `csc AudioBridge.cs
> /reference:System.Web.Extensions.dll` lub przez Visual Studio).
> Sam fix `toggle_master_mute` oraz wszystkie optymalizacje wymagają
> rekompilacji — Electron-side commity działają niezależnie.

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
