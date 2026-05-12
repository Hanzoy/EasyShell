namespace EasyShell;

internal sealed class SettingsForm : Form
{
    private readonly ComboBox _terminalCombo = new();
    private readonly TextBox _hotkeyText = new();
    private readonly CheckBox _startWithWindowsCheck = new();
    private readonly Label _errorLabel = new();

    public string TerminalTargetId => ((TerminalTarget)_terminalCombo.SelectedItem!).Id;
    public string HotkeyText => _hotkeyText.Text.Trim();
    public bool StartWithWindows => _startWithWindowsCheck.Checked;

    public SettingsForm(AppConfig config)
    {
        Text = "EasyShell 设置";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(520, 218);

        var terminalLabel = new Label
        {
            AutoSize = true,
            Text = "默认终端",
            Location = new Point(20, 22)
        };

        _terminalCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _terminalCombo.DisplayMember = nameof(TerminalTarget.DisplayName);
        _terminalCombo.ValueMember = nameof(TerminalTarget.Id);
        var targets = TerminalTargets.GetAvailableTargets();
        _terminalCombo.Items.AddRange(targets.Cast<object>().ToArray());
        _terminalCombo.SelectedItem = targets.FirstOrDefault(target => string.Equals(target.Id, config.TerminalTargetId, StringComparison.OrdinalIgnoreCase))
            ?? targets.First(target => target.Id == TerminalTargets.PowerShellId);
        _terminalCombo.Location = new Point(105, 18);
        _terminalCombo.Width = 380;
        _terminalCombo.DropDownWidth = 460;

        var hotkeyLabel = new Label
        {
            AutoSize = true,
            Text = "快捷键",
            Location = new Point(20, 68)
        };

        _hotkeyText.Text = config.Hotkey;
        _hotkeyText.Location = new Point(105, 64);
        _hotkeyText.Width = 220;
        _hotkeyText.KeyDown += HotkeyTextOnKeyDown;

        _startWithWindowsCheck.AutoSize = true;
        _startWithWindowsCheck.Text = "开机自启";
        _startWithWindowsCheck.Checked = config.StartWithWindows;
        _startWithWindowsCheck.Location = new Point(105, 104);

        _errorLabel.AutoSize = false;
        _errorLabel.ForeColor = Color.Firebrick;
        _errorLabel.Location = new Point(105, 130);
        _errorLabel.Size = new Size(380, 34);

        var saveButton = new Button
        {
            DialogResult = DialogResult.OK,
            Text = "保存",
            Location = new Point(329, 176),
            Size = new Size(75, 28)
        };
        saveButton.Click += SaveButtonOnClick;

        var cancelButton = new Button
        {
            DialogResult = DialogResult.Cancel,
            Text = "取消",
            Location = new Point(410, 176),
            Size = new Size(75, 28)
        };

        Controls.AddRange([terminalLabel, _terminalCombo, hotkeyLabel, _hotkeyText, _startWithWindowsCheck, _errorLabel, saveButton, cancelButton]);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _hotkeyText.Focus();
        _hotkeyText.SelectAll();
    }

    private void SaveButtonOnClick(object? sender, EventArgs e)
    {
        if (HotkeyDefinition.TryParse(_hotkeyText.Text, out var hotkey, out var error))
        {
            _hotkeyText.Text = hotkey.DisplayText;
            _errorLabel.Text = string.Empty;
            return;
        }

        _errorLabel.Text = error;
        DialogResult = DialogResult.None;
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

        _hotkeyText.Text = new HotkeyDefinition(modifiers, (uint)keyCode).DisplayText;
        _errorLabel.Text = string.Empty;
    }
}
