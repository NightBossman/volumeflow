# Plan Pracy - VolumeFlow

## Status Projektu: Alpha (W pełni funkcjonalny backend i UI)

### 1. Przygotowanie Środowiska [x]
- [x] Inicjalizacja projektu (Vite + Svelte + Electron)
- [x] Konfiguracja struktury katalogów
- [x] Wybór technologii audio (Mostek SoundVolumeView CLI)

### 2. Backend Audio [x]
- [x] Implementacja struktury IPC (UI <-> Main)
- [x] Integracja SoundVolumeView.exe do odczytu sesji
- [x] Obsługa zmiany głośności (0-100%) dla procesów
- [x] Obsługa Master Volume (Głośność systemowa)
- [x] **Implementacja Mute Toggle** (SwitchMute) [x]

### 3. Interfejs Użytkownika (Svelte) [x]
- [x] Budowa komponentu `VolumeSlider` (Premium Look)
- [x] Dynamiczna lista procesów z pollingiem danych
- [x] **Dual-Mode Interface** (Kompaktowy vs Rozszerzony)
- [x] Implementacja ikon **Lucide Svelte**
- [x] Sekcja **O programie / About** [x]
- [x] Efekty wizualne (Acrylic/Mica, Glassmorphism)

### 4. Personalizacja [x]
- [x] System 4 motywów premium (Midnight, Solar, Matrix, Frost)
- [x] Tryb **Eye Saver** (Filtr światła niebieskiego)
- [x] Płynne animacje przejść między motywami

### 5. Dystrybucja i GitHub [x]
- [x] Przygotowanie repozytorium na GitHubie
- [x] Publikacja kodu źródłowego i dokumentacji
- [x] Pierwszy udany Merge PR (Wyciszanie i About) [x]
- [/] Konfiguracja **Electron Builder** do tworzenia `.exe`

### 6. Przyszłe funkcje (Post 1.0) [ ]
- [ ] Opracowanie mechanizmu **Overdrive** (Wzmocnienie cyfrowe)
- [ ] System grupowania aplikacji (Multi-select)
- [ ] Ustawienia zapisu (Settings Persistence)
- [ ] Tray Icon (System Tray)
