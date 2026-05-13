namespace EasyShell;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly GlobalHotkey _hotkey = new();
    private readonly GlobalHotkey _adminHotkey = new();
    private AppConfig _config;

    public TrayApplicationContext()
    {
        _config = AppConfig.Load();

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "EasyShell",
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };
        _notifyIcon.DoubleClick += (_, _) => OpenSettings();

        _hotkey.Pressed += (_, _) => OpenTerminal();
        _adminHotkey.Pressed += (_, _) => OpenTerminal(asAdministrator: true);
        RegisterConfiguredHotkeys(showFailure: true);
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("打开终端", null, (_, _) => OpenTerminal());
        menu.Items.Add("以管理员方式打开终端", null, (_, _) => OpenTerminal(asAdministrator: true));
        menu.Items.Add("设置", null, (_, _) => OpenSettings());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("退出", null, (_, _) => ExitThread());
        return menu;
    }

    private void OpenTerminal(bool asAdministrator = false)
    {
        try
        {
            var path = ExplorerPathResolver.GetForegroundExplorerPath();
            var terminalTargetId = ResolveTerminalTargetForPath(path);
            TerminalLauncher.Launch(terminalTargetId, path, asAdministrator);
        }
        catch (Exception ex)
        {
            ShowError($"无法打开终端：{ex.Message}");
        }
    }

    private void OpenSettings()
    {
        UnregisterHotkeys();
        _config.StartWithWindows = StartupManager.IsEnabled();
        using var form = new SettingsForm(_config);
        if (form.ShowDialog() != DialogResult.OK)
        {
            RegisterConfiguredHotkeys(showFailure: true, throwOnFailure: false);
            return;
        }

        var oldConfig = _config;
        _config = new AppConfig
        {
            TerminalTargetId = form.TerminalTargetId,
            UseGitDirectoryTerminal = form.UseGitDirectoryTerminal,
            GitDirectoryTerminalTargetId = form.GitDirectoryTerminalTargetId,
            Hotkey = form.HotkeyText,
            AdminHotkey = form.AdminHotkeyText,
            StartWithWindows = form.StartWithWindows
        };

        try
        {
            RegisterConfiguredHotkeys(showFailure: false, throwOnFailure: true);
            StartupManager.SetEnabled(_config.StartWithWindows);
            _config.Save();
            _notifyIcon.ShowBalloonTip(2000, "EasyShell", "快捷键已更新", ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            _config = oldConfig;
            RegisterConfiguredHotkeys(showFailure: true, throwOnFailure: false);
            ShowError(ex.Message);
        }
    }

    private string ResolveTerminalTargetForPath(string path)
    {
        if (_config.UseGitDirectoryTerminal && GitDirectoryDetector.IsInGitDirectory(path))
        {
            return _config.GitDirectoryTerminalTargetId;
        }

        return _config.TerminalTargetId;
    }

    private void RegisterConfiguredHotkeys(bool showFailure, bool throwOnFailure = false)
    {
        var hotkey = HotkeyDefinition.ParseOrDefault(_config.Hotkey, HotkeyDefinition.DefaultHotkey);
        var adminHotkey = HotkeyDefinition.ParseOrDefault(_config.AdminHotkey, HotkeyDefinition.DefaultAdminHotkey);
        if (hotkey.HasSameGesture(adminHotkey))
        {
            adminHotkey = HotkeyDefinition.DefaultAdminHotkey;
        }

        if (hotkey.HasSameGesture(adminHotkey))
        {
            hotkey = HotkeyDefinition.DefaultHotkey;
        }

        _config.Hotkey = hotkey.DisplayText;
        _config.AdminHotkey = adminHotkey.DisplayText;

        try
        {
            UnregisterHotkeys();
            _hotkey.Register(hotkey);
            _adminHotkey.Register(adminHotkey);
            _notifyIcon.Text = $"EasyShell ({_config.Hotkey})";
        }
        catch (Exception ex)
        {
            UnregisterHotkeys();
            if (showFailure)
            {
                ShowError(ex.Message);
            }

            _notifyIcon.Text = "EasyShell";
            if (throwOnFailure)
            {
                throw;
            }
        }
    }

    private void UnregisterHotkeys()
    {
        _hotkey.Unregister();
        _adminHotkey.Unregister();
    }

    private static void ShowError(string message)
    {
        MessageBox.Show(message, "EasyShell", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    protected override void ExitThreadCore()
    {
        _hotkey.Dispose();
        _adminHotkey.Dispose();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        base.ExitThreadCore();
    }
}
