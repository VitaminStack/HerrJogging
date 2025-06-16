using HerrJogging; // Hinzugefügt, um auf den Namespace HerrJogging zuzugreifen
using System.Collections.ObjectModel;
using System.Windows.Input;


namespace HerrJogging.Pages;

public partial class LäufePage : ContentPage
{
    public ObservableCollection<JoggingRun> JoggingRuns { get; set; }
    public ICommand DeleteRunCommand { get; }

    public LäufePage()
    {
        InitializeComponent();

        // Lädt die Läufe und konvertiert sie in eine ObservableCollection
        JoggingRuns = new ObservableCollection<JoggingRun>(RunStorage.LoadRuns());
        BindingContext = this;

        RunsList.ItemsSource = JoggingRuns;

        DeleteRunCommand = new Command<JoggingRun>(async run => await DeleteRunAsync(run));

        // Leerer Status, falls keine Läufe vorhanden sind
        JoggingRuns.CollectionChanged += (s, e) => UpdateEmptyState();
        UpdateEmptyState();
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // JoggingRuns neu laden (mit deiner Speicherklasse)
        var runs = RunStorage.LoadRuns(); // oder wie immer du speicherst
        JoggingRuns.Clear();
        foreach (var run in runs)
            JoggingRuns.Add(run);

        // Leeren Status anpassen
        UpdateEmptyState();
    }

    private void UpdateEmptyState()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            EmptyState.IsVisible = JoggingRuns.Count == 0;
            RunsList.IsVisible = JoggingRuns.Count > 0;
        });
    }
    private async Task DeleteRunAsync(JoggingRun run)
    {
        if (run == null) return;

        bool confirm = await DisplayAlert(
            "Lauf löschen?",
            "Möchtest du diesen Lauf wirklich löschen?",
            "Ja", "Abbrechen");

        if (!confirm) return;

        // Lauf aus Liste entfernen
        JoggingRuns.Remove(run);

        // Läufe speichern
        RunStorage.SaveRuns(JoggingRuns.ToList());
        UpdateEmptyState();
    }
}
