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
    private readonly MapLayerManager _layerManager = new();
    private readonly TrackingManager _tracker = new();
    private bool _satellite = false;

    public MainPage()
    {
        InitializeComponent();

        // 1. Map initialisieren und Standard-Layer setzen
        var map = new Mapsui.Map();
        map.Layers.Add(_layerManager.RoadLayer);
        MyMap.Map = map;

        // 2. Karte auf aktuellen Standort zentrieren
        _ = CenterMapOnStartAsync();

        // 3. UI-State initialisieren
        UpdateButtonStates();
    }

    private void OnMapToggleClicked(object sender, EventArgs e)
    {
        _satellite = !_satellite;
        MapToggleBtn.Text = _satellite ? "🗺 Karte" : "🌎 Satellit";
        var map = MyMap.Map;
        if (map is null) return;

        map.Layers.Clear();
        map.Layers.Add(_satellite ? _layerManager.SatelliteLayer : _layerManager.RoadLayer);
        MyMap.RefreshGraphics();
    }

    private void OnTrackClicked(object sender, EventArgs e)
    {
        _tracker.Start();
        UpdateButtonStates();
        _cts = new CancellationTokenSource();
        _ = TrackLoopAsync(_cts.Token);
    }
    private void OnPauseClicked(object sender, EventArgs e)
    {
        _tracker.Pause();
        UpdateButtonStates();
    }
    private void OnResumeClicked(object sender, EventArgs e)
    {
        _tracker.Resume();
        UpdateButtonStates();
    }
    private void OnStopTrackClicked(object sender, EventArgs e)
    {
        _tracker.Stop();
        UpdateButtonStates();
        _cts?.Cancel();
        _cts = null;
    }

    private void UpdateButtonStates()
    {
        TrackBtn.IsVisible = !_tracker.IsTracking;
        PauseBtn.IsVisible = _tracker.IsTracking && !_tracker.IsPaused;
        ResumeBtn.IsVisible = _tracker.IsTracking && _tracker.IsPaused;
        StopTrackBtn.IsVisible = _tracker.IsTracking;
    }

    // ===== Map- und Tracking-Helper (ggf. weiter auslagerbar) =====

    private async Task CenterMapOnStartAsync()
    {
        try
        {
            var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
            if (location == null) return;

            var pt = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
            var position = new MPoint(pt.x, pt.y);

            MyMap?.Map?.Navigator.CenterOnAndZoomTo(position, 1, 2000);
            MyMap?.RefreshGraphics();
        }
        catch (Exception)
        {
            // Standort konnte nicht bestimmt werden, ignoriere Fehler
        }
    }

    private CancellationTokenSource? _cts;
    private async Task TrackLoopAsync(CancellationToken token)
    {
        var req = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(1));
        while (!token.IsCancellationRequested)
        {
            try
            {
                var loc = await Geolocation.GetLocationAsync(req, token);
                if (loc is null) continue;
                var pt = SphericalMercator.FromLonLat(loc.Longitude, loc.Latitude);
                _tracker.AddPoint(new MPoint(pt.x, pt.y));
                _tracker.UpdateTrackLayer(MyMap.Map!);
                MyMap.Map.Navigator.CenterOn(new MPoint(pt.x, pt.y));
                MyMap.RefreshGraphics();
            }
            catch (Exception)
            {
                // Bei Abbruch oder fehlender Berechtigung einfach weiterlaufen lassen
            }
        }
    }

    // ===== Menü-Button-Handler (kann später weiter ausgelagert werden) =====

    private void OnButton1Clicked(object sender, EventArgs e) { /* ... */ }
    private void OnButton2Clicked(object sender, EventArgs e) { /* ... */ }
    private void OnKarteClicked(object sender, EventArgs e) { /* ... */ }
    private void OnButton4Clicked(object sender, EventArgs e) { /* ... */ }
    private void OnButton5Clicked(object sender, EventArgs e) { /* ... */ }
}


public class TrackingManager
{
    private readonly List<MPoint> _trackPoints = new();
    private MemoryLayer? _trackLayer;

    public bool IsTracking { get; private set; }
    public bool IsPaused { get; private set; }

    public void Start()
    {
        IsTracking = true;
        IsPaused = false;
        _trackPoints.Clear();
    }
    public void Pause() => IsPaused = true;
    public void Resume() => IsPaused = false;
    public void Stop()
    {
        IsTracking = false;
        IsPaused = false;
        _trackPoints.Clear();
    }

    public void AddPoint(MPoint pt)
    {
        if (IsTracking && !IsPaused)
            _trackPoints.Add(pt);
    }

    public void UpdateTrackLayer(Mapsui.Map map)
    {
        if (_trackPoints.Count < 2) return;
        if (_trackLayer is null)
        {
            _trackLayer = new MemoryLayer
            {
                Name = "Jogging-Track",
                IsMapInfoLayer = false,
                Style = null
            };
            map.Layers.Add(_trackLayer);
        }
        var ntsCoords = _trackPoints.Select(p => new Coordinate(p.X, p.Y)).ToArray();
        var lineString = new LineString(ntsCoords);
        var feature = new GeometryFeature { Geometry = lineString };
        feature.Styles.Add(new VectorStyle
        {
            Line = new Pen(Mapsui.Styles.Color.Red, 4) { PenStyle = PenStyle.Solid }
        });
        _trackLayer.Features = new List<IFeature> { feature };
        _trackLayer.DataHasChanged();
    }
}

public class MapLayerManager
{
    public ILayer RoadLayer { get; }
    public ILayer SatelliteLayer { get; }

    public MapLayerManager()
    {
        RoadLayer = OpenStreetMap.CreateTileLayer();
        SatelliteLayer = CreateSatelliteLayer();
    }

    private ILayer CreateSatelliteLayer()
    {
        const string url = "https://services.arcgisonline.com/arcgis/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}";
        var tileSource = new HttpTileSource(
            tileSchema: new GlobalSphericalMercator(),
            urlBuilder: new BasicUrlBuilder(url),
            name: "Esri World Imagery",
            attribution: new BruTile.Attribution(
                Text: "© Esri, Maxar, Earthstar Geographics",
                Url: "https://www.esri.com/en-us/legal/terms/full-master-agreement")
        );
        return new TileLayer(tileSource);
    }
}