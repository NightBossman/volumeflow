# Features - VolumeFlow

## Kategorie funkcji

### 🟢 Funkcje Ukończone (V1.0 Ready - Zrealizowane)
*   **Real Audio Control:** Precyzyjne sterowanie głośnością pojedynczych procesów oraz systemu (Master).
*   **Interactive Mute:** Szybkie wyciszanie procesów jednym kliknięciem z wizualną informacją o stanie.
*   **Master Mute Sync:** Stan wyciszenia urządzenia głównego jest poprawnie synchronizowany z UI.
*   **Dual-Mode Interface:** Tryb kompaktowy (minimalistyczny) oraz rozszerzony (pełna kontrola).
*   **Premium Glass UI:** Nowoczesna estetyka oparta na przezroczystościach i rozmyciu (Acrylic/Mica).
*   **Theme System:** 5 starannie dobranych palet kolorystycznych (Midnight, Solar, Matrix, Frost, Cyberpunk).
*   **Eye Saver Mode:** Filtr redukujący zmęczenie oczu przy pracy v nocy.
*   **About Section:** Karta informacyjna o aplikacji zintegrowana z UI.
*   **Lucide Icons Integration:** Profesjonalne ikony wektorowe.
*   **High-Performance Polling:** Ultra-szybkie odświeżanie stanu audio bez narzutu systemowego (komunikacja strumieniowa JSON).
*   **Settings Persistence:** Motyw i tryb Eye Saver są zapamiętywane i przywracane po restarcie.
*   **System Tray:** Działanie v tle i pełna obsługa ikony v zasobniku systemowym (minimalizacja, menu kontekstowe).
*   **Native App Icons:** Automatyczne pobieranie rzeczywistych ikon z plików .exe procesów.
*   **Persistent Audio Bridge (Pro):** Natywny mostek v C# (COM) eliminujący miganie kursora i zaśmiecanie Menedżera Zadań.
*   **Tryb Scen (Profile):** Możliwość zapisania presetów głośności (np. "Praca", "Gaming") i szybkiego przełączania.
*   **Auto-Start:** Integracja z systemem Windows (Uruchom przy starcie).
*   **Smooth Fade:** Inteligentne, płynne przejścia głośności przy zmianie profilu.

### 🟡 Funkcje Planowane (Aktualizacje 1.x)
*   **Grupowanie procesów:** Możliwość łączenia suwaków kilku aplikacji v jedną grupę sterowania.
*   **Globalne Skróty Klawiszowe (Hotkeys):** Wyciszanie i zmiana głośności profili lub wybranych aplikacji z klawiatury bez otwierania okna.
*   **Audio Peak Visualizer:** Pulsujące paski głośności obok każdego suwaka reagujące na aktualny dźwięk.
*   **Smart Overdrive (Boost):** Cyfrowe wzmocnienie dźwięku powyżej 100% (do 150-200%).
*   **Wyszukiwarka Procesów:** Zintegrowane pole wyszukiwania dla długich list aplikacji.

### 🔴 Funkcje "Ambitne" (Ecosystem & Future)
*   **Modele Biznesowe (Free vs Pro):** Wprowadzenie darmowej wersji z podstawowymi funkcjami oraz "VolumeFlow Pro" odblokowującej wsparcie vtyczek VST, nielimitowane profile i zaawansowane filtry audio.
*   **AI Smart Mixing:** Sztuczna inteligencja analizująca nawyki użytkownika (np. pora dnia, uruchomiona gra) i automatycznie optymalizująca poziomy dźwięku v tle.
*   **Rozszerzenia Przeglądarki (Browser Add-ons):** Oficjalna wtyczka do Chrome/Edge/Firefox pozwalająca na niezależne sterowanie głośnością pojedynczych kart wideo/audio, a nie całej przeglądarki.
*   **Ekosystem Mobile (Android/iOS):** Samodzielna aplikacja na smartfony pełniąca funkcję mobilnego miksera dla telefonu oraz bezprzewodowego pilota (Remote Control) przez Wi-Fi dla aplikacji na PC.
*   **Marketplace (Community Hub):** Sklep/biblioteka vewnątrz aplikacji, gdzie użytkownicy mogą pobierać, tworzyć i udostępniać customowe motywy (Theme Designer), presety EQ i pluginy.
*   **Cloud Sync:** Synchronizacja własnych profili, motywów i ustawień korektora między różnymi komputerami za pośrednictwem chmury.
*   **Auto-Duck & Per-App EQ:** Automatyczne ściszanie muzyki podczas rozmów na komunikatorach oraz zaawansowany korektor graficzny niezależny dla każdego procesu osobno.
*   **Voice Control Integration:** Kompatybilność z asystentami głosowymi ("Wycisz Spotify", "Głośność ogólna na 50%").

## 🌐 Architektura Przyszłości (Technologie Ekosystemu)
Wdrażanie funkcji "Ambitnych" (szczególnie Cloud Sync, konta Pro, Marketplace i aplikacje mobilne) będzie wymagało rozbudowy projektu o usługi chmurowe. Wstępny plan technologiczny zakłada:
*   **Baza Danych i Backend:** **Firebase** (Firestore do synchronizacji profili v czasie rzeczywistym, Firebase Auth do zarządzania kontami Free/Pro, Cloud Storage dla assetów Marketplace).
*   **Aplikacje Mobilne:** **Dart / Flutter** (pozwoli to na jednoczesne wydanie aplikacji Remote Control na Androida i iOS z natywnym wsparwem dla Firebase).
*   **Web Portal:** Landing page, panel logowania oraz zarządzania subskrypcją hostowany na Firebase Hosting.
