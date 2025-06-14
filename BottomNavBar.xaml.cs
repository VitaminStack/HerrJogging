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
    }

    private async void OnPage1Clicked(object sender, EventArgs e)
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

    private async void OnPage4Clicked(object sender, EventArgs e)
    {
        await (Shell.Current as AppShell).NavigateTo("//Page4");
    }

    private async void OnPage5Clicked(object sender, EventArgs e)
    {
        await (Shell.Current as AppShell).NavigateTo("//Page5");
    }
}
