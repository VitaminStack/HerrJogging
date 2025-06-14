using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HerrJogging;

public partial class BottomNavBar : ContentView
{
    public BottomNavBar()
    {
        InitializeComponent();
        // Initial auf "karte" setzen, oder dynamisch aus Shell.Current.CurrentState.Location lesen!
        SetActiveTab("karte");
        Shell.Current.Navigated += (s, e) =>
        {
            var route = Shell.Current.CurrentState.Location.OriginalString.Replace("//", "");
            SetActiveTab(route);
        };
    }

    public void SetActiveTab(string tab)
    {
        // Setze alle zurück auf den Default
        Tab1Bubble.BackgroundColor = Colors.Transparent;
        Tab1Label.TextColor = Color.FromArgb("#BFC0D7");
        LäufeBubble.BackgroundColor = Colors.Transparent;
        LäufeLabel.TextColor = Color.FromArgb("#BFC0D7");
        KarteBubble.BackgroundColor = Colors.Transparent;
        KarteLabel.TextColor = Color.FromArgb("#BFC0D7");
        Tab4Bubble.BackgroundColor = Colors.Transparent;
        Tab4Label.TextColor = Color.FromArgb("#BFC0D7");
        Tab5Bubble.BackgroundColor = Colors.Transparent;
        Tab5Label.TextColor = Color.FromArgb("#BFC0D7");

        // Der aktive Tab erhält die Bubble & weißes Label, ggf. größere Icon
        if (tab == "Page1")
        {
            Tab1Bubble.BackgroundColor = Color.FromArgb("#4E47EC");
            Tab1Label.TextColor = Colors.White;
        }
        else if (tab == "läufe")
        {
            LäufeBubble.BackgroundColor = Color.FromArgb("#4E47EC");
            LäufeLabel.TextColor = Colors.White;
        }
        else if (tab == "karte")
        {
            KarteBubble.BackgroundColor = Color.FromArgb("#4E47EC");
            KarteLabel.TextColor = Colors.White;
        }
        else if (tab == "Page4")
        {
            Tab4Bubble.BackgroundColor = Color.FromArgb("#4E47EC");
            Tab4Label.TextColor = Colors.White;
        }
        else if (tab == "Page5")
        {
            Tab5Bubble.BackgroundColor = Color.FromArgb("#4E47EC");
            Tab5Label.TextColor = Colors.White;
        }
    }


    private async void OnTab1Clicked(object sender, EventArgs e)
    {
        await (Shell.Current as AppShell).NavigateTo("//Page1");
    }

    private async void OnLäufeClicked(object sender, EventArgs e)
    {
        await (Shell.Current as AppShell).NavigateTo("//läufe");
    }

    private async void OnKarteClicked(object sender, EventArgs e)
    {
        await (Shell.Current as AppShell).NavigateTo("//karte");
    }

    private async void OnTab4Clicked(object sender, EventArgs e)
    {
        await (Shell.Current as AppShell).NavigateTo("//Page4");
    }

    private async void OnTab5Clicked(object sender, EventArgs e)
    {
        await (Shell.Current as AppShell).NavigateTo("//Page5");
    }
}
