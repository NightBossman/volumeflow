# VolumeFlow v1.7.0
Premium Windows Audio Mixer created with Svelte 5 and Electron.

VolumeFlow to lekka, wydajna i estetyczna aplikacja do zarządzania głośnością procesów w systemie Windows. Zbudowana w oparciu o Svelte, Electron oraz dedykowany mostek w C#, oferuje płynność działania nieosiągalną dla systemowego miksera.

![Status](https://img.shields.io/badge/Status-v1.7.0--Stable-brightgreen)
![Platform](https://img.shields.io/badge/Platform-Windows-blue)

## 🚀 Kluczowe Funkcje (v1.7.0)

- **Hardened Audio Engine (V1.8.0):** Ultra-wydajny mostek C# z asynchronicznym buforowaniem, eliminujący opóźnienia i blokady.
- **OSD Notifications:** Powiadomienia na ekranie o zmianach stanu aplikacji (Boost, Recording, Profile).
- **Process Search:** Błyskawiczne filtrowanie listy aktywnych procesów.
- **Smart Overdrive & Ducking:** Zaawansowana kontrola dynamiki dźwięku.
- **Per-App Recording:** Nagrywanie dźwięku z konkretnych okien do formatu WAV.
- **Premium Glass UI:** Interfejs oparty na estetyce Glassmorphism z 5 motywami (Midnight, Solar, Matrix, Frost, Cyberpunk).

## 🛠 Technologia

- **Frontend:** Svelte + Vite
- **Shell:** Electron
- **Backend Audio:** C# (Core Audio API / WASAPI Loopback)
- **Komunikacja:** Strumieniowy mostek JSON z telemetrią `get_stats`.

## 📦 Instalacja (Development)

1. Sklonuj repozytorium:
   ```bash
   git clone https://github.com/NightBossman/volumeflow.git
   ```
2. Zainstaluj zależności:
   ```bash
   npm install
   ```
3. Uruchom w trybie deweloperskim:
   ```bash
   npm run dev
   ```

## 🛤 Plany Rozwoju (Roadmap)

- [ ] **Global Hotkeys:** Skróty klawiszowe dla całego systemu.
- [ ] **EQ for Apps:** Korektor graficzny dla każdej aplikacji.
- [ ] **Mini-Player Mode:** Skondensowany tryb sterowania.

---
*Created with ❤️ by NightBossman & Antigravity AI*
