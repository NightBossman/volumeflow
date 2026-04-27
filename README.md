# 🌊 VolumeFlow
**Nowoczesny Mikser Audio dla Windows z Ekosystemem Przyszłości**

VolumeFlow to lekka, wydajna i estetyczna aplikacja do zarządzania głośnością procesów v systemie Windows. Zbudowana v oparciu o Svelte, Electron oraz dedykowany mostek v C#, oferuje płynność działania nieosiągalną dla systemowego miksera.

![Status](https://img.shields.io/badge/Status-Alpha--Pre--Release-orange)
![Platform](https://img.shields.io/badge/Platform-Windows-blue)

## 🚀 Kluczowe Funkcje (V1.0)

- **Persistent Audio Bridge (Tryb Pro):** Dedykowany mostek `AudioBridge.exe` v C# wykorzystujący natywne API COM. Eliminuje migotanie kursora i zapewnia natychmiastową reakcję suwaków.
- **Premium Glass UI:** Interfejs oparty na estetyce Glassmorphism (Acrylic/Mica) z płynnymi animacjami.
- **System Tray:** Aplikacja działa v tle, minimalizuje się do zasobnika systemowego i pozwala na błyskawiczne przywołanie okna.
- **System Motywów:** 4 starannie dobrane palety (Midnight, Solar, Matrix, Frost) oraz tryb **Eye Saver**.
- **Native Icons:** Automatyczne pobieranie rzeczywistych ikon z plików `.exe` aktywnych procesów.

## 🛠 Technologia

- **Frontend:** Svelte + Vite
- **Shell:** Electron
- **Backend Audio:** C# (Core Audio API / COM Interop)
- **Komunikacja:** Strumieniowy mostek JSON (stdin/stdout)

## 📦 Instalacja (Development)

1. Sklonuj repozytorium:
   ```bash
   git clone https://github.com/NightBossman/volumeflow.git
   ```
2. Zainstaluj zależności:
   ```bash
   npm install
   ```
3. Uruchom v trybie deweloperskim:
   ```bash
   npm run dev
   ```

## 🛤 Plany Rozwoju (Roadmap)

- [ ] **Tryb Scen (Profile):** Zapisywanie i wczytywanie presetów głośności.
- [ ] **Grupowanie Procesów:** Łączenie wielu aplikacji pod jeden suwak.
- [ ] **Visualizer:** Pulsujące paski głośności (Audio Peaks).
- [ ] **Mobile Remote:** Sterowanie głośnością PC z telefonu (Android/iOS).

---
*Created with ❤️ by NightBossman & Antigravity AI*
