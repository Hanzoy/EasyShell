using System.Text.Json;

namespace EasyShell;

internal sealed class AppConfig
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public TerminalKind Terminal { get; set; } = TerminalKind.PowerShell;
    public string Hotkey { get; set; } = "Ctrl+Space";
    public bool StartWithWindows { get; set; }

    public static string ConfigDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasyShell");

    public static string ConfigPath => Path.Combine(ConfigDirectory, "settings.json");

    public static AppConfig Load()
    {
        try
        {
            if (!File.Exists(ConfigPath))
            {
                return new AppConfig();
            }

            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
        catch
        {
            return new AppConfig();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(ConfigDirectory);
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(this, SerializerOptions));
    }
}

internal enum TerminalKind
{
    Cmd,
    PowerShell
}
