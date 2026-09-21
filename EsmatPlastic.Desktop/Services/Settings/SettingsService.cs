using System.IO;
using System.Text.Json;

namespace EsmatPlastic.Desktop.Services.Settings;

public class SettingsService
{
    private readonly string _settingsDirectory;
    private readonly string _settingsFile;

    public AppSettings Current { get; private set; }

    public SettingsService()
    {
        _settingsDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            "EsmatPlastic");

        _settingsFile = Path.Combine(
            _settingsDirectory,
            "settings.json");

        Current = Load();
    }

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsFile))
                return new AppSettings();

            var json = File.ReadAllText(_settingsFile);

            var settings =
                JsonSerializer.Deserialize<AppSettings>(json);

            return settings ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save()
    {
        Save(Current);
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(_settingsDirectory);

        var json = JsonSerializer.Serialize(
            settings,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(_settingsFile, json);

        Current = settings;
    }

    public void Reset()
    {
        if (File.Exists(_settingsFile))
            File.Delete(_settingsFile);

        Current = new AppSettings();
    }
}
