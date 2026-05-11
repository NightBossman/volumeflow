# Roadmap - VolumeFlow Evolution

Status projektu: **v1.7.0 Stable** (OSD & Performance Edition)

## 🎯 Co już zrobiliśmy
| Wersja | Kamień milowy | Status | Kluczowe cechy |
| :--- | :--- | :--- | :--- |
| v1.5.0 | Podstawa silnika | ✅ | WASAPI Integration, Peak Meters, Basic Mixer |
| v1.6.0 | Power Features | ✅ | Smart Overdrive, Per-process Recording, Auto-Ducking |
| v1.7.0 | UX & Stability | ✅ | OSD, Search, Health Monitoring, AudioBridge V1.8.0 |

## 🚀 W trakcie realizacji (v1.8.0)
- [ ] **Global Hotkeys**: Implementacja skrótów klawiszowych (np. `Ctrl+Shift+R`) do sterowania nagrywaniem z dowolnego miejsca w systemie.
- [ ] **Advanced Settings**: Możliwość konfiguracji progu duckingu i prędkości fade bezpośrednio z UI.

## 🔮 Plany na przyszłość
- [ ] **v1.9.0**: Mini-Player Mode - kompaktowy widok z 3 najważniejszymi suwakami przypięty do rogu ekranu.
- [ ] **v2.0.0**: Audio Profiles - zapisywanie i wczytywanie zestawów głośności dla różnych scenariuszy (Gaming, Work, Movie).
- [ ] **Remote Control**: Aplikacja mobilna do sterowania głośnością komputera przez Wi-Fi.

---

## 📈 Ewolucja Architektury
```mermaid
graph TD
    A[Electron Frontend] -->|JSON Commands| B(AudioBridge.cs)
    B -->|WASAPI| C[Windows Audio Engine]
    B -->|Health Check| D[Performance Monitor]
    B -->|Recording| E[WAV Files]
    D -->|Warnings| A
```

## 🛠 Cele Techniczne (Technical Debt & R&D)
- [ ] Przejście na `Rust` dla mostka audio w wersji v2.1.0 dla jeszcze mniejszego narzutu CPU.
- [ ] Implementacja wizualizacji FFT (Spectrum Analyzer) dla każdego procesu.
