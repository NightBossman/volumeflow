# Changelog - VolumeFlow

## [1.0.0] - 2026-04-27
### Added
- **Final V1.0 Release**: Pełna wersja z Trybem Scen i Autostartem.
- **AudioBridge.exe**: Wprowadzenie natywnego mostka audio v C# (Core Audio API).
- **Persistent Connection**: Electron utrzymuje stałe połączenie z mostkiem przez stdin/stdout.
- **JSON Protocol**: Nowy, szybki protokół komunikacji między procesami.
- **New Theme**: Dodano 5. motyw kolorystyczny: **Cyberpunk 2077**.
- **UI Polish**: Zmniejszono przeźroczystość tła i ulepszono izolację Eye Saver (nagłówek bez filtra).

### Fixed
- **Cursor Flickering**: Wyeliminowano problem migającego kursora systemowego przy odświeżaniu danych.
- **Process Spam**: Wyeliminowano ciągłe uruchamianie i zamykanie podprocesów v Menedżerze Zadań.
- **Parsing Errors**: Usunięto błędy "Error 32" i problemy z niekompletnym JSONem dzięki buforowaniu strumienia.

## [Alpha 0.6] - 2026-04-27
### Added
- **AudioBridge.exe**: Wprowadzenie natywnego mostka audio v C# (Core Audio API).
- **Persistent Connection**: Electron utrzymuje stałe połączenie z mostkiem przez stdin/stdout.
- **JSON Protocol**: Nowy, szybki protokół komunikacji między procesami.
- **New Theme**: Dodano 5. motyw kolorystyczny: **Cyberpunk 2077**.
- **UI Polish**: Zmniejszono przeźroczystość tła i ulepszono izolację Eye Saver (nagłówek bez filtra).

### Fixed
- **Cursor Flickering**: Wyeliminowano problem migającego kursora systemowego przy odświeżaniu danych.
- **Process Spam**: Wyeliminowano ciągłe uruchamianie i zamykanie podprocesów v Menedżerze Zadań.
- **Parsing Errors**: Usunięto błędy "Error 32" i problemy z niekompletnym JSONem dzięki buforowaniu strumienia.

## [Alpha 0.5] - 2026-04-26
### Added
- **System Tray**: Obsługa ikony v zasobniku systemowym.
- **Native Icons**: Pobieranie ikon `.exe` za pomocą `app.getFileIcon`.
- **Settings Persistence**: Zapisywanie motywu i trybu Eye Saver do `config.json`.

### Fixed
- **Tray Exit**: Naprawiono problem z brakiem reakcji na przycisk zamknięcia przy schowanej aplikacji.

## [Alpha 0.4] - 2026-04-25
### Added
- **Dual-Mode UI**: Tryb kompaktowy i rozszerzony.
- **Theme System**: 4 motywy kolorystyczne (Midnight, Solar, Matrix, Frost).
- **Eye Saver**: Filtr światła niebieskiego.

### [Alpha 0.1] - 2026-04-20
- Inicjalizacja projektu (Vite + Svelte + Electron).
- Podstawowa kontrola głośności przez SoundVolumeView CLI.
