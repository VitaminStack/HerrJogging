using System.Numerics;
using Microsoft.Maui.Controls;

namespace HerrJogging.Debugging;

public partial class VirtualJoystick : ContentView
{
    public Vector2 Vector { get; private set; }

    private Point _center;
    private double _radius;

    public VirtualJoystick()
    {
        InitializeComponent();

        SizeChanged += (_, _) =>
        {
            _center = new Point(Width / 2, Height / 2);
            _radius = Width / 2;
            ResetKnob();
        };

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPan;
        GestureRecognizers.Add(pan);
    }

    private void OnPan(object? _, PanUpdatedEventArgs e)
    {
        if (e.StatusType == GestureStatus.Completed) { ResetKnob(); return; }

        var pos = new Point(_center.X + e.TotalX, _center.Y + e.TotalY);
        var dx = pos.X - _center.X;
        var dy = pos.Y - _center.Y;

        var len = Math.Sqrt(dx * dx + dy * dy);
        if (len > _radius) { dx *= _radius / len; dy *= _radius / len; }

        Knob.TranslationX = dx;
        Knob.TranslationY = dy;

        // X-Achse bleibt, Y wird invertiert (oben = positiv)
        Vector = new Vector2((float)(dx / _radius), (float)(-dy / _radius));
    }

    private void ResetKnob()
    {
        Knob.TranslationX = Knob.TranslationY = 0;
        Vector = Vector2.Zero;
    }
}
