namespace KeyShow;

public class Settings
{
    public int DisplayTimeMs { get; set; } = 500;
    public string Position { get; set; } = "TopLeft";
    public int? WindowX { get; set; }
    public int? WindowY { get; set; }

    private static string GetSettingsPath()
    {
        var dir = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "KeyShow");
        try { System.IO.Directory.CreateDirectory(dir); } catch { }
        return System.IO.Path.Combine(dir, "settings.json");
    }

    public void Save()
    {
        try
        {
            var opts = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            var json = System.Text.Json.JsonSerializer.Serialize(this, opts);
            System.IO.File.WriteAllText(GetSettingsPath(), json);
        }
        catch { }
    }

    public static Settings Load()
    {
        try
        {
            var path = GetSettingsPath();
            if (!System.IO.File.Exists(path))
                return new Settings();

            var json = System.IO.File.ReadAllText(path);
            var settings = System.Text.Json.JsonSerializer.Deserialize<Settings>(json);
            if (settings == null)
                return new Settings();
            return settings;
        }
        catch
        {
            return new Settings();
        }
    }
}