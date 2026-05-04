# Changelog - VolumeFlow

## [1.5.0] - 2026-05-04
### Added
- **Reliability Update**: Kompleksowa stabilizacja komunikacji i zarządzania pamięcią.
- **IPC Protocol V2**: Wprowadzenie `requestId` dla asynchronicznego routingu żądań – eliminuje błędy kolejkowania.
- **Context Bridge Security**: Pełna migracja na `contextIsolation: true` i `contextBridge` w Electronie.
- **Auto-Cleanup**: Automatyczne zamykanie mostka przy wyjściu z aplikacji (`isQuitting` flag).

### Fixed
- **COM Resource Leaks**: Gwarantowane zwalnianie obiektów COM w pętli `PeakPollingLoop` (eliminacja crashy przy zmianie urządzeń).
- **Process Handle Leaks**: Bezpieczne zamykanie uchwytów procesów Windows dzięki `using`.
- **Fading Glitches**: Naprawiono nakładanie się animacji głośności (asynchroniczny fade z anulowaniem).
- **Preload Best Practices**: Zastosowano poprawki sugerowane przez Codex (once: true, dev-only logging).

## [1.1.0] - 2026-05-04
### Added
- **Stability Overhaul**: Całkowita przebudowa backendu AudioBridge.cs (V1.4.0).
- **Dynamic Device Tracking**: Mostek teraz automatycznie przełącza się między urządzeniami audio w czasie rzeczywistym.
- **Multithreaded Sync**: Wprowadzono blokady (stdoutLock) zapobiegające mieszaniu się danych JSON na strumieniu wyjściowym.
- **Improved JSON Parser**: Przejście na `JavaScriptSerializer` po stronie backendu – koniec z błędami parsowania tekstowego.

### Fixed
- **Memory Leaks**: Usunięto wycieki obiektów COM poprzez jawne zwalnianie wskaźników RCW.
- **Session Mismatch**: Naprawiono błędy GUID-ów sesji, co przywróciło działanie kontroli głośności procesów.
- **Missing Sessions**: Usunięto agresywny filtr pików – teraz sesje, które są chwilowo ciche, nie znikają z interfejsu.
- **SetVolume Implementation**: W pełni działająca funkcja ustawiania głośności dla poszczególnych procesów.

## [1.0.0] - 2026-04-27
### Added
- **Final V1.0 Release**: Pełna wersja z Trybem Scen i Autostartem.
- **AudioBridge.exe**: Wprowadzenie natywnego mostka audio w C# (Core Audio API).
- **Persistent Connection**: Electron utrzymuje stałe połączenie z mostkiem przez stdin/stdout.
- **JSON Protocol**: Nowy, szybki protokół komunikacji między procesami.
- **New Theme**: Dodano 5. motyw kolorystyczny: **Cyberpunk 2077**.
- **UI Polish**: Zmniejszono przeźroczystość tła i ulepszono izolację Eye Saver (nagłówek bez filtra).

### Fixed
- **Cursor Flickering**: Wyeliminowano problem migającego kursora systemowego przy odświeżaniu danych.
- **Process Spam**: Wyeliminowano ciągłe uruchamianie i zamykanie podprocesów w Menedżerze Zadań.
- **Parsing Errors**: Usunięto błędy "Error 32" i problemy z niekompletnym JSONem dzięki buforowaniu strumienia.

## [Alpha 0.5] - 2026-04-26
### Added
- **System Tray**: Obsługa ikony w zasobniku systemowym.
- **Native Icons**: Pobieranie ikon `.exe` za pomocą `app.getFileIcon`.
- **Settings Persistence**: Zapisywanie motywu i trybu Eye Saver do `config.json`.

### Fixed
- **Tray Exit**: Naprawiono problem z brakiem reakcji na przycisk zamknięcia przy schowanej aplikacji.

## [Alpha 0.4] - 2026-04-25
### Added
- **Dual-Mode UI**: Tryb kompaktowy i rozszerzony.
- **Theme System**: 4 motywy kolorystyczne (Midnight, Solar, Matrix, Frost).
- **Eye Saver**: Filtr światła niebieskiego.

## [Alpha 0.1] - 2026-04-20
- Inicjalizacja projektu (Vite + Svelte + Electron).
- Podstawowa kontrola głośności przez SoundVolumeView CLI.
