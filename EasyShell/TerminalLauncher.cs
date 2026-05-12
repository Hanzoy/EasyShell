using System.Diagnostics;

namespace EasyShell;

internal static class TerminalLauncher
{
    public static void Launch(string terminalTargetId, string workingDirectory)
    {
        var directory = Directory.Exists(workingDirectory)
            ? workingDirectory
            : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var target = TerminalTargets.Resolve(terminalTargetId);
        switch (target.Kind)
        {
            case TerminalTargetKind.DirectCmd:
                StartDirect("cmd.exe", directory);
                break;
            case TerminalTargetKind.DirectPowerShell:
                StartDirect("powershell.exe", directory);
                break;
            case TerminalTargetKind.WindowsTerminalDefault:
                StartWindowsTerminal(directory, profileName: null);
                break;
            case TerminalTargetKind.WindowsTerminalProfile:
                StartWindowsTerminal(directory, target.ProfileName);
                break;
        }
    }

    private static void StartDirect(string fileName, string directory)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = directory,
            UseShellExecute = true
        });
    }

    private static void StartWindowsTerminal(string directory, string? profileName)
    {
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
}
