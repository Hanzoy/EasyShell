namespace EasyShell;

internal enum TerminalTargetKind
{
    DirectCmd,
    DirectPowerShell,
    DirectGitBash,
    WindowsTerminalDefault,
    WindowsTerminalProfile
}

internal sealed record TerminalTarget(
    string Id,
    string DisplayName,
    TerminalTargetKind Kind,
    string? ProfileName = null,
    string? ExecutablePath = null)
{
    public string SourceLabel => Kind switch
    {
        TerminalTargetKind.DirectCmd or TerminalTargetKind.DirectPowerShell or TerminalTargetKind.DirectGitBash => "直接启动",
        TerminalTargetKind.WindowsTerminalDefault or TerminalTargetKind.WindowsTerminalProfile => "Windows Terminal",
        _ => string.Empty
    };

    public override string ToString()
    {
        return DisplayName;
    }
}

internal static class TerminalTargets
{
    public const string CmdId = "direct:cmd";
    public const string PowerShellId = "direct:powershell";
    public const string GitBashId = "direct:git-bash";
    public const string WindowsTerminalDefaultId = "wt:default";

    public static IReadOnlyList<TerminalTarget> GetAvailableTargets()
    {
        var targets = new List<TerminalTarget>
        {
            new(CmdId, "cmd.exe", TerminalTargetKind.DirectCmd),
            new(PowerShellId, "powershell.exe", TerminalTargetKind.DirectPowerShell)
        };

        var gitBashPath = GitBashDetector.FindBashPath();
        if (!string.IsNullOrWhiteSpace(gitBashPath))
        {
            targets.Add(new(GitBashId, "Git Bash", TerminalTargetKind.DirectGitBash, ExecutablePath: gitBashPath));
        }

        if (TerminalProfileScanner.IsWindowsTerminalAvailable())
        {
            targets.Add(new(WindowsTerminalDefaultId, "默认 profile", TerminalTargetKind.WindowsTerminalDefault));
            targets.AddRange(TerminalProfileScanner.ScanProfiles());
        }

        return targets
            .GroupBy(target => target.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    public static TerminalTarget Resolve(string id)
    {
        var targets = GetAvailableTargets();
        return targets.FirstOrDefault(target => string.Equals(target.Id, id, StringComparison.OrdinalIgnoreCase))
            ?? targets.First(target => target.Id == PowerShellId);
    }
}
