using System.Diagnostics;

namespace EasyShell;

internal static class TerminalLauncher
{
    public static void Launch(TerminalKind terminal, string workingDirectory)
    {
        var directory = Directory.Exists(workingDirectory)
            ? workingDirectory
            : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var fileName = terminal == TerminalKind.Cmd ? "cmd.exe" : "powershell.exe";

        Process.Start(new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = directory,
            UseShellExecute = true
        });
    }
}
