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
using static HerrJogging.Pages.JoggingRun;

namespace HerrJogging.Pages;

public partial class KartePage : ContentPage
{
    private readonly MapLayerManager _layerManager = new();
    private readonly TrackingManager _tracker = new();
    private bool _satellite = false;
    private JoggingRun? _currentRun;
    private List<JoggingRun> _runs = new(); // Hier werden alle abgeschlossenen Läufe gespeichert
    private System.Diagnostics.Stopwatch _stopwatch = new();
    private MemoryLayer? _locationMarkerLayer;
    private CancellationTokenSource? _locationCts;
    

    public KartePage()
    {
        InitializeComponent();
        _runs = RunStorage.LoadRuns();
        var map = new Mapsui.Map();
        map.Layers.Add(_layerManager.RoadLayer);
        MyMap.Map = map;

        _ = CenterMapOnStartAsync(2000);

        UpdateButtonStates();
        StartLocationLoop();
    }
    private void StartLocationLoop()
    {
        _locationCts = new CancellationTokenSource();
        _ = LocationLoopAsync(_locationCts.Token);
    }
    private async Task LocationLoopAsync(CancellationToken token)
    {
        var req = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(1));
        while (!token.IsCancellationRequested)
        {
            try
            {
                var loc = await Geolocation.GetLocationAsync(req, token);
                System.Diagnostics.Debug.WriteLine($"[LocationLoop] {loc?.Latitude}, {loc?.Longitude}");

                if (loc != null)
                {
                    var pt = SphericalMercator.FromLonLat(loc.Longitude, loc.Latitude);
                    UpdateLocationMarker(new MPoint(pt.x, pt.y), MyMap.Map!);

                    if (_tracker.IsTracking)
                    {
                        // Hier wirklich immer wieder UpdateTrackLayer!
                        _tracker.UpdateTrackLayer(MyMap.Map!);
                    }

                    MyMap?.RefreshGraphics();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocationLoop] Exception: {ex}");
            }
            await Task.Delay(1000, token);
        }
    }


    private void OnMapToggleClicked(object sender, EventArgs e)
    {
        _satellite = !_satellite;
        MapToggleBtn.Text = _satellite ? "🗺 Karte" : "🌎 Satellit";
        var map = MyMap.Map;
        if (map is null) return;

        // Entferne nur Basemap-Layer
        if (map.Layers.Contains(_layerManager.RoadLayer))
            map.Layers.Remove(_layerManager.RoadLayer);
        if (map.Layers.Contains(_layerManager.SatelliteLayer))
            map.Layers.Remove(_layerManager.SatelliteLayer);

        // Füge neuen Basemap-Layer an erster Stelle ein
        map.Layers.Insert(0, _satellite ? _layerManager.SatelliteLayer : _layerManager.RoadLayer);

        MyMap.RefreshGraphics();
    }


    private void OnTrackClicked(object sender, EventArgs e)
    {
        _ = CenterMapOnStartAsync(1000); // Karte zentrieren und zoomen
        _tracker.Start();
        _stopwatch.Restart();
        _currentRun = new JoggingRun { StartTime = DateTime.Now };
        UpdateButtonStates();
        _cts = new CancellationTokenSource();
        _ = TrackLoopAsync(_cts.Token);
    }
    private void OnPauseClicked(object sender, EventArgs e)
    {
        _tracker.Pause();
        _stopwatch.Stop();
        UpdateButtonStates();
    }
    private void OnResumeClicked(object sender, EventArgs e)
    {
        _tracker.Resume();
        _stopwatch.Start();
        UpdateButtonStates();
    }
    private async void OnStopTrackClicked(object sender, EventArgs e)
    {
        
        _tracker.Stop();
        _stopwatch.Stop();
        UpdateButtonStates();
        _cts?.Cancel();
        _cts = null;

        if (_currentRun != null)
        {
            _currentRun.EndTime = DateTime.Now;
            // Optional: Kopiere die Route noch mal final aus dem Tracker
            _currentRun.Route = _tracker.GetCurrentRoute().Select(pt => new PointDto { X = pt.X, Y = pt.Y }).ToList();

            _runs.Add(_currentRun);
            RunStorage.SaveRuns(_runs);

            await DisplayAlert("Lauf gespeichert",
                $"Dauer: {_currentRun.Duration}\nDistanz: {Math.Round(_currentRun.TotalDistanceMeters / 1000, 2)} km",
                "OK");
        }

        _currentRun = null;
    }


    private void UpdateButtonStates()
    {
        TrackBtn.IsVisible = !_tracker.IsTracking;
        PauseBtn.IsVisible = _tracker.IsTracking && !_tracker.IsPaused;
        ResumeBtn.IsVisible = _tracker.IsTracking && _tracker.IsPaused;
        StopTrackBtn.IsVisible = _tracker.IsTracking;
    }

    // ===== Map- und Tracking-Helper (ggf. weiter auslagerbar) =====
        
    private CancellationTokenSource? _cts;
    private async Task TrackLoopAsync(CancellationToken token)
    {
        var req = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(1));
        while (!token.IsCancellationRequested)
        {
            try
            {

                var loc = await Geolocation.GetLocationAsync(req, token);
                if (loc is null)
                {
                    await Task.Delay(1000, token);
                    continue;
                }
                var pt = SphericalMercator.FromLonLat(loc.Longitude, loc.Latitude);
                _tracker.AddPoint(new MPoint(pt.x, pt.y));
                _tracker.UpdateTrackLayer(MyMap.Map!);
                UpdateLocationMarker(new MPoint(pt.x, pt.y), MyMap.Map!);
                _ = CenterMapOnStartAsync(1000); // Karte zentrieren und zoomen
                MyMap.RefreshGraphics();
            }
            catch (Exception)
            {
                await Task.Delay(1000, token); // Weiterlaufen, ggf. warten
            }
        }
    }


    private void UpdateLocationMarker(Mapsui.MPoint position, Mapsui.Map map)
    {
        if (_locationMarkerLayer == null)
        {
            _locationMarkerLayer = new MemoryLayer { Name = "LocationMarker" };
            map.Layers.Add(_locationMarkerLayer);
        }

        var feature = new Mapsui.Nts.GeometryFeature
        {
            Geometry = new NetTopologySuite.Geometries.Point(position.X, position.Y)
        };
        feature.Styles.Add(new Mapsui.Styles.SymbolStyle
        {
            SymbolScale = 0.5f,
            Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.Blue),
            Outline = new Mapsui.Styles.Pen(Mapsui.Styles.Color.Black, 2),
        });

        _locationMarkerLayer.Features = new List<Mapsui.IFeature> { feature };
        _locationMarkerLayer.DataHasChanged();
    }

    private async Task CenterMapOnStartAsync(long duration)
    {
        try
        {
            var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
            if (location == null) return;

            var pt = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
            var position = new MPoint(pt.x, pt.y);

            MyMap?.Map?.Navigator.CenterOnAndZoomTo(position, 1, duration);
            MyMap?.RefreshGraphics();
        }
        catch (Exception)
        {
            // Standort konnte nicht bestimmt werden, ignoriere Fehler
        }
    }
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
        // Track behalten, falls du ihn noch anzeigen willst!
        // _trackPoints.Clear(); // NICHT hier leeren!
    }


    public List<MPoint> GetCurrentRoute()
    {
        return new List<MPoint>(_trackPoints); // <== richtig!
    }

    public void AddPoint(MPoint pt)
    {
        if (IsTracking && !IsPaused)
            _trackPoints.Add(pt);
    }

    public void UpdateTrackLayer(Mapsui.Map map)
    {
        if (_trackPoints.Count < 2) return; // Korrigiert!

        if (_trackLayer is null)
        {
            _trackLayer = new MemoryLayer
            {
                Name = "Jogging-Track",
                Style = null
            };
            map.Layers.Add(_trackLayer);
        }

        var ntsCoords = _trackPoints.Select(p => new NetTopologySuite.Geometries.Coordinate(p.X, p.Y)).ToArray();
        var lineString = new NetTopologySuite.Geometries.LineString(ntsCoords);
        var feature = new Mapsui.Nts.GeometryFeature { Geometry = lineString };
        feature.Styles.Add(new Mapsui.Styles.VectorStyle
        {
            Line = new Mapsui.Styles.Pen(Mapsui.Styles.Color.Red, 3)
        });

        _trackLayer.Features = new List<Mapsui.IFeature> { feature };
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

public class JoggingRun
{
    public class PointDto
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public List<PointDto> Route { get; set; } = new();
    public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;
    public double TotalDistanceMeters => CalculateDistance();

    private double CalculateDistance()
    {
        if (Route.Count < 2) return 0;
        double sum = 0;
        for (int i = 1; i < Route.Count; i++)
        {
            sum += Distance(Route[i - 1], Route[i]);
        }
        return sum;
    }

    private double Distance(PointDto a, PointDto b)
    {
        double dx = a.X - b.X;
        double dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
