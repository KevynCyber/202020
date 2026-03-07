using System.Text.Json;

namespace TwentyTwentyTwenty;

public class SettingsManager
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public Settings Current { get; private set; } = new();

    public SettingsManager() : this(GetDefaultPath()) { }

    public SettingsManager(string filePath)
    {
        _filePath = filePath;
    }

    public void Load()
    {
        if (!File.Exists(_filePath))
        {
            Current = new Settings();
            return;
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            Current = JsonSerializer.Deserialize<Settings>(json, JsonOptions) ?? new Settings();
        }
        catch
        {
            Current = new Settings();
        }
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (dir != null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(Current, JsonOptions);
        File.WriteAllText(_filePath, json);
    }

    private static string GetDefaultPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "202020", "settings.json");
    }
}
