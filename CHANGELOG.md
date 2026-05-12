# Changelog - VolumeFlow

Wszystkie istotne zmiany w tym projekcie będą dokumentowane w tym pliku.

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
