# Changelog - VolumeFlow

## [1.7.0] - 2026-05-11
### Added
- **OSD Notifications**: Graficzne powiadomienia na ekranie informujące o zmianach profilu, statusie nagrywania i trybie Boost.
- **Process Search**: Nowy pasek wyszukiwania pozwalający błyskawicznie odfiltrować aplikacje na liście.
- **AudioBridge V1.8.0**: Ulepszony, utwardzony silnik audio z asynchronicznym buforowaniem stdout (zapobiega blokowaniu procesu) i lepszą obsługą błędów COM.
- **Open Recordings Button**: Szybki dostęp do folderu nagrań bezpośrednio z panelu ustawień.
- **Health Monitoring**: Automatyczne monitorowanie wydajności mostka audio (telemetria get_stats).

### Fixed
- **Recording Stability**: Naprawiono błąd, przez który pliki nagrań nie zapisywały się poprawnie na niektórych systemach.
- **UI Interaction**: Poprawiono zachowanie przycisków w regionach "drag", przywracając pełną interaktywność suwaków.

## [1.6.0] - 2026-05-10

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
