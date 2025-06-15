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

        var envelope = line.EnvelopeInternal;
        double minSpan = 0.001; // Mindestausdehnung, damit nicht zu stark reingezoomt wird
        if (envelope.Width < minSpan) envelope.ExpandBy(minSpan, 0);
        if (envelope.Height < minSpan) envelope.ExpandBy(0, minSpan);

        var mrect = new Mapsui.MRect(envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY);
        MiniMap.Map.Navigator.ZoomToBox(mrect, Mapsui.MBoxFit.Fit);

        MiniMap.RefreshGraphics();
    }

}
