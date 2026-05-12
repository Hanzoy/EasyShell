namespace EasyShell;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly GlobalHotkey _hotkey = new();
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
        RegisterConfiguredHotkey(showFailure: true);
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("打开终端", null, (_, _) => OpenTerminal());
        menu.Items.Add("设置", null, (_, _) => OpenSettings());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("退出", null, (_, _) => ExitThread());
        return menu;
    }

    private void OpenTerminal()
    {
        try
        {
            var path = ExplorerPathResolver.GetForegroundExplorerPath();
            TerminalLauncher.Launch(_config.TerminalTargetId, path);
        }
        catch (Exception ex)
        {
            ShowError($"无法打开终端：{ex.Message}");
        }
    }

    private void OpenSettings()
    {
        _hotkey.Unregister();
        _config.StartWithWindows = StartupManager.IsEnabled();
        using var form = new SettingsForm(_config);
        if (form.ShowDialog() != DialogResult.OK)
        {
            RegisterConfiguredHotkey(showFailure: true, throwOnFailure: false);
            return;
        }

        var oldConfig = _config;
        _config = new AppConfig
        {
            TerminalTargetId = form.TerminalTargetId,
            Hotkey = form.HotkeyText,
            StartWithWindows = form.StartWithWindows
        };

        try
        {
            RegisterConfiguredHotkey(showFailure: false, throwOnFailure: true);
            StartupManager.SetEnabled(_config.StartWithWindows);
            _config.Save();
            _notifyIcon.ShowBalloonTip(2000, "EasyShell", $"快捷键已更新为 {_config.Hotkey}", ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            _config = oldConfig;
            RegisterConfiguredHotkey(showFailure: true, throwOnFailure: false);
            ShowError(ex.Message);
        }
    }

    private void RegisterConfiguredHotkey(bool showFailure, bool throwOnFailure = false)
    {
        var hotkey = HotkeyDefinition.ParseOrDefault(_config.Hotkey);
        _config.Hotkey = hotkey.DisplayText;

        try
        {
            _hotkey.Register(hotkey);
            _notifyIcon.Text = $"EasyShell ({_config.Hotkey})";
        }
        catch (Exception ex)
        {
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

    private static void ShowError(string message)
    {
        MessageBox.Show(message, "EasyShell", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    protected override void ExitThreadCore()
    {
        _hotkey.Dispose();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        base.ExitThreadCore();
    }
}
