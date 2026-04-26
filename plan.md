# Plan Pracy - VolumeFlow

## Status Projektu: Alpha → Pre-Release (Backend + UI gotowe, trwają szlify V1.0)

### 1. Przygotowanie Środowiska [x]
- [x] Inicjalizacja projektu (Vite + Svelte + Electron)
- [x] Konfiguracja struktury katalogów
- [x] Wybór technologii audio (Mostek SoundVolumeView CLI)

### 2. Backend Audio [x]
- [x] Implementacja struktury IPC (UI <-> Main)
- [x] Integracja SoundVolumeView.exe do odczytu sesji
- [x] Obsługa zmiany głośności (0-100%) dla procesów
- [x] Obsługa Master Volume (Głośność systemowa)
- [x] Implementacja Mute Toggle (SwitchMute)
- [x] Pobieranie stanu Muted dla Master i procesów

### 3. Interfejs Użytkownika (Svelte) [x]
- [x] Budowa komponentu `VolumeSlider` (Premium Look)
- [x] Dynamiczna lista procesów z pollingiem danych
- [x] **Dual-Mode Interface** (Kompaktowy vs Rozszerzony)
- [x] Implementacja ikon **Lucide Svelte**
- [x] Sekcja **O programie / About** (karta informacyjna)
- [x] Efekty wizualne (Acrylic/Mica, Glassmorphism)

### 4. Personalizacja [x]
- [x] System 4 motywów premium (Midnight, Solar, Matrix, Frost)
- [x] Tryb **Eye Saver** (Filtr światła niebieskiego)
- [x] Płynne animacje przejść między motywami
- [x] **Settings Persistence** – zapamiętywanie motywu i Eye Saver w `config.json`

### 5. Dystrybucja i GitHub [/]
- [x] Przygotowanie repozytorium na GitHubie
- [x] Publikacja kodu źródłowego i dokumentacji
- [x] Pierwszy merge PR (Wyciszanie + About + Poprawka Codex)
- [/] Konfiguracja **Electron Builder** do tworzenia `.exe`
- [ ] Instrukcja instalacji dla użytkownika

### 6. Szybki Szlif V1.0 (W toku) [/]
- [x] Settings Persistence (Motyw + Eye Saver)
- [ ] **System Tray** – ikonka w zasobniku systemowym (praca w tle)
- [ ] **Prawdziwe ikony aplikacji** – wyciąganie ikon z plików .exe
- [ ] **Tryb Scen (Profile)** – zapisywalne presety głośności

### 7. Przyszłe funkcje (Post V1.0) [ ]
- [ ] Grupowanie procesów (Multi-select)
- [ ] Globalne Skróty Klawiszowe (Hotkeys)
- [ ] Audio Peak Visualizer
- [ ] Smart Overdrive (Boost powyżej 100%)
- [ ] Wyszukiwarka Procesów
