using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using static HerrJogging.Pages.JoggingRun;
using Color = Mapsui.Styles.Color;

namespace HerrJogging.Controls;

public partial class MiniMapView : ContentView
{
    public static readonly BindableProperty RouteProperty =
        BindableProperty.Create(nameof(Route), typeof(List<PointDto>), typeof(MiniMapView), propertyChanged: OnRouteChanged);

    public List<PointDto> Route
    {
        get => (List<PointDto>)GetValue(RouteProperty);
        set => SetValue(RouteProperty, value);
    }

    public MiniMapView()
    {
        InitializeComponent();
        MiniMap.Map = new Mapsui.Map();
    }

    private static void OnRouteChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MiniMapView miniMapView)
            miniMapView.RenderRoute();
    }

    private void RenderRoute()
    {

        const double MinSpanDeg = 0.01;  // ≈ 1 km
        const double PaddingFrac = 0.15;  // 15 % Rand

        if (Route == null || Route.Count < 2)
        {
            // Dummy-Viewport (z. B. zentriert auf Deutschland)
            MiniMap.Map.Navigator.CenterOnAndZoomTo(new Mapsui.MPoint(10, 51), 50000); // Koordinaten & Zoom anpassen
            MiniMap.RefreshGraphics();
            return;
        }

        var map = MiniMap.Map;
        map.Layers.Clear();

        // OSM Layer
        map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

        // Route-Layer
        var points = Route.Select(p => new NetTopologySuite.Geometries.Coordinate(p.X, p.Y)).ToArray();
        var line = new NetTopologySuite.Geometries.LineString(points);

        var routeLayer = new Mapsui.Layers.MemoryLayer
        {
            Features = new List<Mapsui.IFeature>
        {
            new Mapsui.Nts.GeometryFeature
            {
                Geometry = line,
                Styles = { new Mapsui.Styles.VectorStyle { Line = new Mapsui.Styles.Pen(Mapsui.Styles.Color.Red, 3) } }
            }
        }
        };
        map.Layers.Add(routeLayer);

        var env = line.EnvelopeInternal;

        // Mindest­größe
        if (env.Width < MinSpanDeg) env.ExpandBy((MinSpanDeg - env.Width) / 2, 0);
        if (env.Height < MinSpanDeg) env.ExpandBy(0, (MinSpanDeg - env.Height) / 2);

        // zusätzlicher Rand
        env.ExpandBy(env.Width * PaddingFrac, env.Height * PaddingFrac);

        var mrect = new Mapsui.MRect(env.MinX, env.MinY, env.MaxX, env.MaxY);
        MiniMap.Map.Navigator.ZoomToBox(mrect, Mapsui.MBoxFit.Fit);

        MiniMap.RefreshGraphics();
    }

}
