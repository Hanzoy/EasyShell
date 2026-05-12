namespace EasyShell;

internal sealed class SettingsForm : Form
{
    private readonly ComboBox _terminalCombo = new();
    private readonly CheckBox _useGitDirectoryTerminalCheck = new();
    private readonly ComboBox _gitDirectoryTerminalCombo = new();
    private readonly TextBox _hotkeyText = new();
    private readonly TextBox _adminHotkeyText = new();
    private readonly CheckBox _startWithWindowsCheck = new();
    private readonly Label _errorLabel = new();

    public string TerminalTargetId => ((TerminalTarget)_terminalCombo.SelectedItem!).Id;
    public bool UseGitDirectoryTerminal => _useGitDirectoryTerminalCheck.Checked;
    public string GitDirectoryTerminalTargetId => ((TerminalTarget)_gitDirectoryTerminalCombo.SelectedItem!).Id;
    public string HotkeyText => _hotkeyText.Text.Trim();
    public string AdminHotkeyText => _adminHotkeyText.Text.Trim();
    public bool StartWithWindows => _startWithWindowsCheck.Checked;

    public SettingsForm(AppConfig config)
    {
        Text = "EasyShell 设置";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(520, 338);

        var terminalLabel = new Label
        {
            AutoSize = true,
            Text = "默认终端",
            Location = new Point(20, 22)
        };

        var targets = TerminalTargets.GetAvailableTargets();
        ConfigureTerminalCombo(_terminalCombo, targets, config.TerminalTargetId, TerminalTargets.PowerShellId);
        _terminalCombo.Location = new Point(105, 18);
        _terminalCombo.Width = 380;
        _terminalCombo.DropDownWidth = 460;

        _useGitDirectoryTerminalCheck.AutoSize = true;
        _useGitDirectoryTerminalCheck.Text = "Git 目录使用额外终端";
        _useGitDirectoryTerminalCheck.Checked = config.UseGitDirectoryTerminal;
        _useGitDirectoryTerminalCheck.Location = new Point(105, 58);
        _useGitDirectoryTerminalCheck.CheckedChanged += (_, _) => UpdateGitDirectoryTerminalState();

        var gitDirectoryTerminalLabel = new Label
        {
            AutoSize = true,
            Text = "额外终端",
            Location = new Point(20, 96)
        };

        ConfigureTerminalCombo(_gitDirectoryTerminalCombo, targets, config.GitDirectoryTerminalTargetId, TerminalTargets.GitBashId);
        _gitDirectoryTerminalCombo.Location = new Point(105, 92);
        _gitDirectoryTerminalCombo.Width = 380;
        _gitDirectoryTerminalCombo.DropDownWidth = 460;

        var hotkeyLabel = new Label
        {
            AutoSize = true,
            Text = "普通快捷键",
            Location = new Point(20, 148)
        };

        _hotkeyText.Text = config.Hotkey;
        _hotkeyText.Location = new Point(105, 144);
        _hotkeyText.Width = 220;
        _hotkeyText.KeyDown += HotkeyTextOnKeyDown;

        var adminHotkeyLabel = new Label
        {
            AutoSize = true,
            Text = "管理员快捷键",
            Location = new Point(20, 188)
        };

        _adminHotkeyText.Text = config.AdminHotkey;
        _adminHotkeyText.Location = new Point(105, 184);
        _adminHotkeyText.Width = 220;
        _adminHotkeyText.KeyDown += HotkeyTextOnKeyDown;

        _startWithWindowsCheck.AutoSize = true;
        _startWithWindowsCheck.Text = "开机自启";
        _startWithWindowsCheck.Checked = config.StartWithWindows;
        _startWithWindowsCheck.Location = new Point(105, 224);

        _errorLabel.AutoSize = false;
        _errorLabel.ForeColor = Color.Firebrick;
        _errorLabel.Location = new Point(105, 250);
        _errorLabel.Size = new Size(380, 34);

        var saveButton = new Button
        {
            DialogResult = DialogResult.OK,
            Text = "保存",
            Location = new Point(329, 296),
            Size = new Size(75, 28)
        };
        saveButton.Click += SaveButtonOnClick;

        var cancelButton = new Button
        {
            DialogResult = DialogResult.Cancel,
            Text = "取消",
            Location = new Point(410, 296),
            Size = new Size(75, 28)
        };

        Controls.AddRange([terminalLabel, _terminalCombo, _useGitDirectoryTerminalCheck, gitDirectoryTerminalLabel, _gitDirectoryTerminalCombo, hotkeyLabel, _hotkeyText, adminHotkeyLabel, _adminHotkeyText, _startWithWindowsCheck, _errorLabel, saveButton, cancelButton]);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
        UpdateGitDirectoryTerminalState();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _hotkeyText.Focus();
        _hotkeyText.SelectAll();
    }

    private void SaveButtonOnClick(object? sender, EventArgs e)
    {
        if (!HotkeyDefinition.TryParse(_hotkeyText.Text, out var hotkey, out var error))
        {
            _errorLabel.Text = $"普通快捷键：{error}";
            DialogResult = DialogResult.None;
            return;
        }

        if (!HotkeyDefinition.TryParse(_adminHotkeyText.Text, out var adminHotkey, out error))
        {
            _errorLabel.Text = $"管理员快捷键：{error}";
            DialogResult = DialogResult.None;
            return;
        }

        if (string.Equals(hotkey.DisplayText, adminHotkey.DisplayText, StringComparison.OrdinalIgnoreCase))
        {
            _errorLabel.Text = "普通快捷键和管理员快捷键不能相同。";
            DialogResult = DialogResult.None;
            return;
        }

        _hotkeyText.Text = hotkey.DisplayText;
        _adminHotkeyText.Text = adminHotkey.DisplayText;
        _errorLabel.Text = string.Empty;
    }

    private static void ConfigureTerminalCombo(ComboBox comboBox, IReadOnlyList<TerminalTarget> targets, string selectedId, string fallbackId)
    {
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.DisplayMember = nameof(TerminalTarget.DisplayName);
        comboBox.ValueMember = nameof(TerminalTarget.Id);
        comboBox.Items.AddRange(targets.Cast<object>().ToArray());

        comboBox.SelectedItem = targets.FirstOrDefault(target => string.Equals(target.Id, selectedId, StringComparison.OrdinalIgnoreCase))
            ?? targets.FirstOrDefault(target => target.Id == fallbackId)
            ?? targets.First(target => target.Id == TerminalTargets.PowerShellId);
    }

    private void UpdateGitDirectoryTerminalState()
    {
        _gitDirectoryTerminalCombo.Enabled = _useGitDirectoryTerminalCheck.Checked;
    }

    private void HotkeyTextOnKeyDown(object? sender, KeyEventArgs e)
    {
        e.SuppressKeyPress = true;

        var keyCode = e.KeyCode;
        if (keyCode is Keys.ControlKey or Keys.ShiftKey or Keys.Menu or Keys.LWin or Keys.RWin)
        {
            return;
        }

        var modifiers = HotkeyModifiers.NoRepeat;
        if (e.Control)
        {
            modifiers |= HotkeyModifiers.Control;
        }

        if (e.Alt)
        {
            modifiers |= HotkeyModifiers.Alt;
        }

        if (e.Shift)
        {
            modifiers |= HotkeyModifiers.Shift;
        }

        if ((e.Modifiers & Keys.LWin) == Keys.LWin || (e.Modifiers & Keys.RWin) == Keys.RWin)
        {
            modifiers |= HotkeyModifiers.Win;
        }

        if ((modifiers & (HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Shift | HotkeyModifiers.Win)) == 0)
        {
            _errorLabel.Text = "请至少按住 Ctrl、Alt、Shift 或 Win 中的一个修饰键。";
            return;
        }

        if (sender is TextBox textBox)
        {
            textBox.Text = new HotkeyDefinition(modifiers, (uint)keyCode).DisplayText;
        }

        _errorLabel.Text = string.Empty;
    }
}
