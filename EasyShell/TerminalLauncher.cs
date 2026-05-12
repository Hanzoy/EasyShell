using System.Diagnostics;

namespace EasyShell;

internal static class TerminalLauncher
{
    public static void Launch(string terminalTargetId, string workingDirectory, bool asAdministrator = false)
    {
        var directory = Directory.Exists(workingDirectory)
            ? workingDirectory
            : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var target = TerminalTargets.Resolve(terminalTargetId);
        switch (target.Kind)
        {
            case TerminalTargetKind.DirectCmd:
                StartCmd(directory, asAdministrator);
                break;
            case TerminalTargetKind.DirectPowerShell:
                StartPowerShell(directory, asAdministrator);
                break;
            case TerminalTargetKind.WindowsTerminalDefault:
                StartWindowsTerminal(directory, profileName: null, asAdministrator);
                break;
            case TerminalTargetKind.WindowsTerminalProfile:
                StartWindowsTerminal(directory, target.ProfileName, asAdministrator);
                break;
        }
    }

    private static void StartCmd(string directory, bool asAdministrator)
    {
        StartShell(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/k cd /d {Quote(directory)}",
            WorkingDirectory = directory,
            UseShellExecute = true
        }, asAdministrator);
    }

    private static void StartPowerShell(string directory, bool asAdministrator)
    {
        StartShell(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoExit -NoLogo -Command Set-Location -LiteralPath {PowerShellQuote(directory)}",
            WorkingDirectory = directory,
            UseShellExecute = true
        }, asAdministrator);
    }

    private static void StartShell(ProcessStartInfo startInfo, bool asAdministrator)
    {
        if (asAdministrator)
        {
            startInfo.Verb = "runas";
        }

        Process.Start(startInfo);
    }

    private static void StartWindowsTerminal(string directory, string? profileName, bool asAdministrator)
    {
        if (asAdministrator)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "wt.exe",
                Arguments = BuildWindowsTerminalArguments(directory, profileName),
                UseShellExecute = true,
                Verb = "runas"
            });
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "wt.exe",
            UseShellExecute = false
        };
        if (!string.IsNullOrWhiteSpace(profileName))
        {
            startInfo.ArgumentList.Add("-p");
            startInfo.ArgumentList.Add(profileName);
        }

        startInfo.ArgumentList.Add("-d");
        startInfo.ArgumentList.Add(directory);

        Process.Start(startInfo);
    }

    private static string BuildWindowsTerminalArguments(string directory, string? profileName)
    {
        var arguments = new List<string>();
        if (!string.IsNullOrWhiteSpace(profileName))
        {
            arguments.Add("-p");
            arguments.Add(Quote(profileName));
        }

        arguments.Add("-d");
        arguments.Add(Quote(directory));
        return string.Join(" ", arguments);
    }

    private static string Quote(string value)
    {
        return $"\"{value.Replace("\"", "\\\"")}\"";
    }

    private static string PowerShellQuote(string value)
    {
        return $"'{value.Replace("'", "''")}'";
    }
}
