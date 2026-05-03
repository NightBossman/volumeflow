# Changelog - VolumeFlow

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
