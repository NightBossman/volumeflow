# VolumeFlow - Premium Audio Control

VolumeFlow to zaawansowany mikser dźwięku dla systemu Windows, oferujący precyzyjną kontrolę, wizualizację w czasie rzeczywistym oraz unikalne funkcje automatyzacji.

![VolumeFlow UI](assets/screenshot.png)

## 🚀 Główne Funkcje (v1.7.0)

- **Vibrant Audio Visualizers**: Płynne mierniki szczytowe (peak meters) dla każdego procesu.
- **Smart Overdrive**: Bezpieczne podbijanie głośności ponad standardowe 100%.
- **Audio Recording**: Nagrywanie dźwięku z konkretnych aplikacji bez przechwytywania całego systemu.
- **Auto-Ducking**: Automatyczne ściszanie muzyki/tła, gdy wybrana aplikacja (np. komunikator) emituje dźwięk.
- **OSD Notifications**: Powiadomienia na ekranie o stanie aplikacji.
- **Instant Search**: Szybkie filtrowanie procesów audio.
- **Modern UI**: Interfejs oparty na efektach szklanego połysku (glassmorphism).

## 🛠 Technologia

- **Frontend**: Svelte + Vite (Premium UI Design)
- **Backend**: C# (WASAPI / Core Audio API)
- **Runtime**: Electron
- **Inter-Process**: JSON-based Stdin/Stdout Stream with Backpressure Management

## 📦 Instalacja i Uruchomienie

1. Sklonuj repozytorium.
2. Zainstaluj zależności: `npm install`
3. Uruchom w trybie deweloperskim: `npm run start`

## 🗺 Roadmap

- [x] v1.7.0: OSD, Search, Health Monitoring, AudioBridge Hardening
- [ ] v1.8.0: Global Hotkeys (Ctrl+Shift+R dla nagrywania)
- [ ] v1.9.0: Mini-Player Mode & Compact View
- [ ] v2.0.0: Profile Audio & Cloud Sync

## 📄 Licencja

MIT - Zobacz plik [LICENSE](LICENSE) po więcej szczegółów.
