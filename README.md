
# HerrJogging 🏃‍♂️

**HerrJogging** ist eine moderne, plattformübergreifende Jogging-App (entwickelt mit .NET MAUI und Mapsui), die es dir ermöglicht, deine Läufe zu tracken, deine Jogging-Historie einzusehen und Kartenfunktionen komfortabel zu nutzen – alles mit Fokus auf Datenschutz, Eleganz und Usability.

## Features

- 🗺 **Live-Karte:** Anzeige deiner aktuellen Position auf OpenStreetMap-Basis (Mapsui).
- ▶️ **Tracking:** Starte, pausiere, setze fort und stoppe Laufaufzeichnungen bequem per Button.
- 📜 **Laufhistorie:** Übersicht über alle bisherigen Läufe.
- ⭐ **Favoriten:** Markiere und speichere Lieblingsstrecken.
- ⚙️ **Einstellungen:** Passe die App an deine Bedürfnisse an.
- ✨ **Custom Navigation:** Moderne, individuell gestaltete Navigationsleiste für schnelle Page-Wechsel.

## Screenshots

> *Hier kannst du noch eigene Screenshots einfügen!*

## Installation & Build

### Voraussetzungen

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- Visual Studio 2022 (mit MAUI-Workload)
- Windows/macOS/Linux (MAUI unterstützt alle großen Plattformen)

### Schritt-für-Schritt

1. **Repository klonen**
    ```bash
    git clone https://github.com/VitaminStack/HerrJogging.git
    cd HerrJogging
    ```

2. **Abhängigkeiten wiederherstellen**
    ```bash
    dotnet restore
    ```

3. **App starten**
    - **In Visual Studio:** Projektmappe öffnen, gewünschte Plattform auswählen, und auf ▶️ **Starten** klicken.
    - **Kommandozeile (Windows)**
      ```bash
      dotnet build
      dotnet maui run -f:net8.0-windows
      ```
    - **Kommandozeile (Android/iOS/Mac)**
      ```bash
      dotnet maui run -f:net8.0-android
      dotnet maui run -f:net8.0-ios
      dotnet maui run -f:net8.0-maccatalyst
      ```

> Tipp: Die Unterstützung für iOS/Mac erfordert einen Mac mit Xcode und korrekter MAUI-Installation.

## Verzeichnisstruktur (Auszug)

```
HerrJogging/
├── App.xaml, AppShell.xaml         # Einstieg & Shell-Navigation
├── BottomNavBar.xaml               # Custom Bottom Navigation Bar
├── Pages/                          # Alle ContentPages (Karte, Läufe, Page1 usw.)
│    ├── KartePage.xaml
│    ├── LäufePage.xaml
│    └── ...
├── Resources/                      # Bilder, Schriftarten etc.
└── MauiProgram.cs                  # DI und MAUI-Bootstrapper
```

## Technologien & Pakete

- [.NET MAUI](https://learn.microsoft.com/de-de/dotnet/maui/)  
- [Mapsui](https://github.com/Mapsui/Mapsui) für OpenStreetMap-Karten  
- C#, XAML

## Lizenz

[MIT License](LICENSE.txt)

---

**Hinweis:**  
Dieses Projekt befindet sich in aktiver Entwicklung. Ideen, Pull Requests oder Bug Reports sind willkommen!

---

## Mitmachen

1. Forke das Repository
2. Erstelle einen neuen Branch (`git checkout -b feature/DeinFeature`)
3. Committe deine Änderungen (`git commit -am 'Füge ein neues Feature hinzu'`)
4. Push auf deinen Branch (`git push origin feature/DeinFeature`)
5. Erstelle einen Pull Request

---

Viel Spaß beim Laufen und Entwickeln! 👟


![Beispielbild](https://github.com/VitaminStack/HerrJogging/Screen.PNG)