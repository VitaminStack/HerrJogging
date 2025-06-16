using System.Text.Json;
using Microsoft.Maui.Storage;

namespace HerrJogging;

public static class RunStorage
{
    private static string StoragePath =>
        Path.Combine(FileSystem.AppDataDirectory, "jogging_runs.json");

    public static void SaveRuns(List<Pages.JoggingRun> runs)
    {
        var json = JsonSerializer.Serialize(runs);
        File.WriteAllText(StoragePath, json);
    }

    public static List<Pages.JoggingRun> LoadRuns()
    {
        if (!File.Exists(StoragePath))
            return new List<Pages.JoggingRun>();

        var json = File.ReadAllText(StoragePath);
        return JsonSerializer.Deserialize<List<Pages.JoggingRun>>(json) ?? new List<Pages.JoggingRun>();
    }
}
