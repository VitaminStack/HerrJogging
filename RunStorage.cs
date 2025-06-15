using System.Text.Json;
using Microsoft.Maui.Storage;

namespace HerrJogging;

public static class RunStorage
{
    private const string RunsKey = "jogging_runs";

    public static void SaveRuns(List<Pages.JoggingRun> runs)
    {
        var json = JsonSerializer.Serialize(runs);
        Preferences.Set(RunsKey, json);
    }

    public static List<Pages.JoggingRun> LoadRuns()
    {
        var json = Preferences.Get(RunsKey, "");
        if (string.IsNullOrEmpty(json)) return new List<Pages.JoggingRun>();
        return JsonSerializer.Deserialize<List<Pages.JoggingRun>>(json) ?? new List<Pages.JoggingRun>();
    }

}
