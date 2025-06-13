using BruTile;
using BruTile.Predefined;
using BruTile.Web;
using Mapsui;                 // Map, MPoint, …
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using NetTopologySuite.Geometries;

namespace HerrJogging;

public partial class MainPage : ContentPage
{
    private bool _satellite;
    private ILayer _roadLayer;
    private ILayer _satLayer;
    private bool _tracking;
    private CancellationTokenSource? _cts;
    private readonly List<MPoint> _trackPoints = new();
    private MemoryLayer? _trackLayer;

    public MainPage()
    {
        InitializeComponent();

        // ❶ Neue Mapsui-Map instanzieren
        var map = new Mapsui.Map();

        // ❷ Layer initialisieren
        _roadLayer = OpenStreetMap.CreateTileLayer();
        _satLayer = CreateSatelliteLayer(); // Hier Ihre spezifische Satellitenebene erstellen

        // Straßen-Layer als Standard hinzufügen
        map.Layers.Add(_roadLayer);

        // ❸ Der MapControl-Instanz zuweisen
        MyMap.Map = map;

        // Starte automatische Zentrierung/Zoom auf aktuellen Standort
        _ = CenterMapOnStartAsync();
    }

    public class CustomUrlBuilder : IUrlBuilder
    {
        private readonly string _urlTemplate;

        public CustomUrlBuilder(string urlTemplate)
        {
            _urlTemplate = urlTemplate;
        }

        public Uri GetUrl(TileInfo tileInfo)
        {
            var url = _urlTemplate
                .Replace("{z}", tileInfo.Index.Level.ToString())
                .Replace("{x}", tileInfo.Index.Col.ToString())
                .Replace("{y}", tileInfo.Index.Row.ToString());
            return new Uri(url);
        }
    }

    private ILayer CreateSatelliteLayer()
    {
        const string url =
        "https://services.arcgisonline.com/arcgis/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}";

        var tileSource = new HttpTileSource(
            tileSchema: new GlobalSphericalMercator(),
            urlBuilder: new BasicUrlBuilder(url), // CustomUrlBuilder verwenden
            name: "Esri World Imagery",
            attribution: new BruTile.Attribution(
                Text: "© Esri, Maxar, Earthstar Geographics",
                Url: "https://www.esri.com/en-us/legal/terms/full-master-agreement")
        );

        return new TileLayer(tileSource);
    }

    private void OnMapToggleClicked(object sender, EventArgs e)
    {
        _satellite = !_satellite;
        MapToggleBtn.Text = _satellite ? "🗺 Karte" : "🌎 Satellit";

        if (MyMap.Map is null) return;

        if (_satellite)
        {
            // Umschalten auf Satellit
            if (MyMap.Map.Layers.Contains(_roadLayer))
                MyMap.Map.Layers.Remove(_roadLayer);

            if (!MyMap.Map.Layers.Contains(_satLayer))
                MyMap.Map.Layers.Insert(0, _satLayer);
        }
        else
        {
            // Umschalten auf Karte
            if (MyMap.Map.Layers.Contains(_satLayer))
                MyMap.Map.Layers.Remove(_satLayer);

            if (!MyMap.Map.Layers.Contains(_roadLayer))
                MyMap.Map.Layers.Insert(0, _roadLayer);
        }


        MyMap.RefreshGraphics();
    }
    private void OnTrackClicked(object sender, EventArgs e)
    {
        if (!_tracking)
        {
            _tracking = true;
            TrackBtn.Text = "■ Stop Tracking";

            _trackPoints.Clear();
            _cts = new CancellationTokenSource();

            _ = TrackLoopAsync(_cts.Token);
            return;
        }

        // 2) Stoppen
        _tracking = false;
        TrackBtn.Text = "▶ Start Tracking";
        _cts?.Cancel();
        _cts = null;
    }
    private void OnStopTrackClicked(object sender, EventArgs e)
    {
        _tracking = false;
        TrackBtn.IsEnabled = true;
        StopTrackBtn.IsVisible = false;
        // ... Tracking stoppen
    }

    private async Task TrackLoopAsync(CancellationToken token)
    {
        var req = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(1));

        while (!token.IsCancellationRequested)
        {
            try
            {
                var loc = await Geolocation.GetLocationAsync(req, token);
                if (loc is null) continue;

                // Koordinate ins Web-Mercator-System von Mapsui projizieren
                var pt = new MPoint(
                    SphericalMercator.FromLonLat(loc.Longitude, loc.Latitude).x,
                    SphericalMercator.FromLonLat(loc.Longitude, loc.Latitude).y
                );
                _trackPoints.Add(pt);

                UpdateTrackLayer();          // Linie neu zeichnen
                MyMap.Map.Navigator.CenterOn(pt); // Karte mittig setzen
                MyMap.RefreshGraphics();
            }
            catch (Exception)
            {
                // Bei Abbruch oder fehlender Berechtigung einfach weiterlaufen lassen
            }
        }
    }
    private void UpdateTrackLayer()
    {
        if (_trackPoints.Count < 2) return; // Linie erst ab 2 Punkten

        // Erster Aufruf? Layer anlegen
        if (_trackLayer is null)
        {
            _trackLayer = new MemoryLayer
            {
                Name = "Jogging-Track",
                IsMapInfoLayer = false,
                Style = null // Wir stylen jede Feature selbst
            };
            MyMap.Map!.Layers.Add(_trackLayer);
        }

        var ntsCoords = _trackPoints.Select(p => new Coordinate(p.X, p.Y)).ToArray();

        var lineString = new LineString(ntsCoords);
        var feature = new GeometryFeature { Geometry = lineString };
        feature.Styles.Add(new VectorStyle
        {
            Line = new Pen(Mapsui.Styles.Color.Red, 4) { PenStyle = PenStyle.Solid }
        });

        // Dem Layer den neuen Inhalt geben
        _trackLayer.Features = new List<IFeature> { feature }; // Korrekte Eigenschaft verwenden
        _trackLayer.DataHasChanged(); // Layer neu berechnen
    }
    private async Task CenterMapOnStartAsync()
    {
        try
        {
            var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
            if (location == null) return;

            var pt = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
            var position = new MPoint(pt.x, pt.y);

            // Neuer: Auf "Zoomlevel" statt "Maßstab" gehen
            MyMap?.Map?.Navigator.CenterOnAndZoomTo(position, 1, 2000); // Zoomlevel 16 = nah dran
            MyMap?.RefreshGraphics();
        }
        catch (Exception)
        {
            // Standort konnte nicht bestimmt werden, ignoriere Fehler
        }
    }




    //MENU BUTTONS LOGIC
    private void OnButton1Clicked(object sender, EventArgs e)
    {
        // Logik für den Button "⚡" hier einfügen
    }
    private void OnButton2Clicked(object sender, EventArgs e)
    {
        // Logik für den Button "⚡" hier einfügen
    }
    private void OnKarteClicked(object sender, EventArgs e)
    {
        // Logik für den Button "⚡" hier einfügen
    }
    private void OnButton4Clicked(object sender, EventArgs e)
    {
        // Logik für den Button "⚡" hier einfügen
    }
    private void OnButton5Clicked(object sender, EventArgs e)
    {
        // Logik für den Button "⚡" hier einfügen
    }

}
