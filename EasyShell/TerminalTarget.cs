namespace EasyShell;

internal enum TerminalTargetKind
{
    DirectCmd,
    DirectPowerShell,
    WindowsTerminalDefault,
    WindowsTerminalProfile
}

internal sealed record TerminalTarget(
    string Id,
    string DisplayName,
    TerminalTargetKind Kind,
    string? ProfileName = null)
{
    public string SourceLabel => Kind switch
    {
        TerminalTargetKind.DirectCmd or TerminalTargetKind.DirectPowerShell => "直接启动",
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
    public const string WindowsTerminalDefaultId = "wt:default";

    public static IReadOnlyList<TerminalTarget> GetAvailableTargets()
    {
        var targets = new List<TerminalTarget>
        {
            new(CmdId, "cmd.exe", TerminalTargetKind.DirectCmd),
            new(PowerShellId, "powershell.exe", TerminalTargetKind.DirectPowerShell)
        };

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
        return GetAvailableTargets().FirstOrDefault(target => string.Equals(target.Id, id, StringComparison.OrdinalIgnoreCase))
            ?? GetAvailableTargets().First(target => target.Id == PowerShellId);
    }
}
