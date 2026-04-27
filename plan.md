# Plan Pracy - VolumeFlow

## Status Projektu: Alpha → Pre-Release (Backend + UI gotowe, trwają szlify V1.0)

### 1. Przygotowanie Środowiska [x]
- [x] Inicjalizacja projektu (Vite + Svelte + Electron)
- [x] Konfiguracja struktury katalogów
- [x] Wybór technologii audio (Mostek C# Native COM)

### 2. Backend Audio [x]
- [x] Implementacja struktury IPC (UI <-> Main)
- [x] Integracja AudioBridge.exe do odczytu sesji (JSON Stream)
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
- [x] System 5 motywów premium (Midnight, Solar, Matrix, Frost, Cyberpunk)
- [x] Tryb **Eye Saver** (Filtr światła niebieskiego)
- [x] Płynne animacje przejść między motywami
- [x] **Settings Persistence** – zapamiętywanie motywu i Eye Saver v `config.json`

### 5. Dystrybucja i GitHub [x]
- [x] Przygotowanie repozytorium na GitHubie
- [x] Publikacja kodu źródłowego i dokumentacji
- [x] Pierwszy merge PR (Wyciszanie + About + Poprawka Codex)
- [x] Push dokumentacji technicznej i README.md

### 6. Szybki Szlif V1.0 (W toku) [/]
- [x] Settings Persistence (Motyw + Eye Saver)
- [x] **System Tray** – ikonka v zasobniku systemowym (praca v tle)
- [x] **Prawdziwe ikony aplikacji** – wyciąganie ikon z plików .exe
- [x] **Audio Bridge (Pro)** – natywny mostek C# eliminujący miganie kursora
- [/] **Tryb Scen (Profile)** – zapisywalne presety głośności
- [ ] **Autostart** – opcja uruchamiania aplikacji z systemem Windows
- [ ] **Volume Fading** – płynne przejścia głośności przy zmianie profilu

### 7. Przyszłe funkcje (Post V1.0) [ ]
- [ ] Grupowanie procesów (Multi-select)
- [ ] Globalne Skróty Klawiszowe (Hotkeys)
- [ ] Audio Peak Visualizer
- [ ] Smart Overdrive (Boost powyżej 100%)
- [ ] Wyszukiwarka Procesów
