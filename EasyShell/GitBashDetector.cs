using Microsoft.Win32;

namespace EasyShell;

internal static class GitBashDetector
{
    private static readonly string[] RegistrySubKeys =
    [
        @"Software\GitForWindows",
        @"Software\WOW6432Node\GitForWindows"
    ];

    public static string? FindBashPath()
    {
        foreach (var installPath in GetRegistryInstallPaths())
        {
            var bashPath = Path.Combine(installPath, "bin", "bash.exe");
            if (File.Exists(bashPath))
            {
                return bashPath;
            }
        }

        foreach (var bashPath in GetCommonBashPaths())
        {
            if (File.Exists(bashPath))
            {
                return bashPath;
            }
        }

        return FindInPath("bash.exe");
    }

    private static IEnumerable<string> GetRegistryInstallPaths()
    {
        foreach (var root in new[] { Registry.CurrentUser, Registry.LocalMachine })
        {
            foreach (var subKey in RegistrySubKeys)
            {
                using var key = root.OpenSubKey(subKey, writable: false);
                var value = key?.GetValue("InstallPath") as string;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    yield return value;
                }
            }
        }
    }

    private static IEnumerable<string> GetCommonBashPaths()
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        yield return Path.Combine(programFiles, "Git", "bin", "bash.exe");
        yield return Path.Combine(programFilesX86, "Git", "bin", "bash.exe");
        yield return Path.Combine(localAppData, "Programs", "Git", "bin", "bash.exe");
    }

    private static string? FindInPath(string fileName)
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var candidate = Path.Combine(directory.Trim(), fileName);
                if (File.Exists(candidate) && candidate.Contains($"{Path.DirectorySeparatorChar}Git{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }
            }
            catch
            {
                // Ignore malformed PATH entries.
            }
        }

        return null;
    }
}
