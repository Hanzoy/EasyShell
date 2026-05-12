using System.Text.Json;
using System.Text.Json.Nodes;

namespace EasyShell;

internal static class TerminalProfileScanner
{
    private static readonly JsonNodeOptions NodeOptions = new() { PropertyNameCaseInsensitive = true };
    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    public static bool IsWindowsTerminalAvailable()
    {
        return TryFindExecutable("wt.exe");
    }

    public static IReadOnlyList<TerminalTarget> ScanProfiles()
    {
        var profiles = new List<TerminalTarget>();
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in GetProfileFiles())
        {
            foreach (var name in ReadProfileNames(file))
            {
                if (!seenNames.Add(name))
                {
                    continue;
                }

                profiles.Add(new TerminalTarget(
                    $"wt:profile:{name}",
                    name,
                    TerminalTargetKind.WindowsTerminalProfile,
                    name));
            }
        }

        return profiles;
    }

    private static IEnumerable<string> GetProfileFiles()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        var files = new List<string>
        {
            Path.Combine(localAppData, "Packages", "Microsoft.WindowsTerminal_8wekyb3d8bbwe", "LocalState", "settings.json"),
            Path.Combine(localAppData, "Packages", "Microsoft.WindowsTerminalPreview_8wekyb3d8bbwe", "LocalState", "settings.json"),
            Path.Combine(localAppData, "Microsoft", "Windows Terminal", "settings.json")
        };

        files.AddRange(GetJsonFiles(Path.Combine(localAppData, "Microsoft", "Windows Terminal", "Fragments")));
        files.AddRange(GetJsonFiles(Path.Combine(programData, "Microsoft", "Windows Terminal", "Fragments")));

        return files.Where(File.Exists);
    }

    private static IEnumerable<string> GetJsonFiles(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return [];
        }

        try
        {
            return Directory.EnumerateFiles(directory, "*.json", SearchOption.AllDirectories);
        }
        catch
        {
            return [];
        }
    }

    private static IEnumerable<string> ReadProfileNames(string file)
    {
        JsonNode? root;
        try
        {
            root = JsonNode.Parse(File.ReadAllText(file), NodeOptions, DocumentOptions);
        }
        catch
        {
            yield break;
        }

        foreach (var profile in GetProfileArray(root))
        {
            var name = profile?["name"]?.GetValue<string>();
            var hidden = profile?["hidden"]?.GetValue<bool>() ?? false;
            if (!hidden && !string.IsNullOrWhiteSpace(name))
            {
                yield return name.Trim();
            }
        }
    }

    private static IEnumerable<JsonNode?> GetProfileArray(JsonNode? root)
    {
        var profiles = root?["profiles"];
        if (profiles is JsonArray array)
        {
            return array;
        }

        if (profiles?["list"] is JsonArray list)
        {
            return list;
        }

        return [];
    }

    private static bool TryFindExecutable(string fileName)
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                if (File.Exists(Path.Combine(directory.Trim(), fileName)))
                {
                    return true;
                }
            }
            catch
            {
                // Ignore malformed PATH entries.
            }
        }

        return false;
    }
}
