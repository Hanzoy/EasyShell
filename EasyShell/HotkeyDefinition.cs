using System.Globalization;
using System.Text;

namespace EasyShell;

[Flags]
internal enum HotkeyModifiers : uint
{
    None = 0,
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Win = 0x0008,
    NoRepeat = 0x4000
}

internal sealed record HotkeyDefinition(HotkeyModifiers Modifiers, uint Key)
{
    public string DisplayText
    {
        get
        {
            var parts = new List<string>();
            if (Modifiers.HasFlag(HotkeyModifiers.Control))
            {
                parts.Add("Ctrl");
            }

            if (Modifiers.HasFlag(HotkeyModifiers.Alt))
            {
                parts.Add("Alt");
            }

            if (Modifiers.HasFlag(HotkeyModifiers.Shift))
            {
                parts.Add("Shift");
            }

            if (Modifiers.HasFlag(HotkeyModifiers.Win))
            {
                parts.Add("Win");
            }

            parts.Add(KeyToText((Keys)Key));
            return string.Join("+", parts);
        }
    }

    public static bool TryParse(string text, out HotkeyDefinition hotkey, out string error)
    {
        hotkey = new HotkeyDefinition(HotkeyModifiers.None, 0);
        error = string.Empty;

        var parts = text.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            error = "请输入快捷键，例如 Ctrl+Space。";
            return false;
        }

        var modifiers = HotkeyModifiers.NoRepeat;
        Keys? key = null;

        foreach (var rawPart in parts)
        {
            var part = Normalize(rawPart);
            switch (part)
            {
                case "CTRL":
                case "CONTROL":
                    modifiers |= HotkeyModifiers.Control;
                    break;
                case "ALT":
                    modifiers |= HotkeyModifiers.Alt;
                    break;
                case "SHIFT":
                    modifiers |= HotkeyModifiers.Shift;
                    break;
                case "WIN":
                case "WINDOWS":
                case "META":
                    modifiers |= HotkeyModifiers.Win;
                    break;
                default:
                    if (key is not null)
                    {
                        error = "快捷键只能包含一个主键。";
                        return false;
                    }

                    if (!TryParseKey(part, out var parsedKey))
                    {
                        error = $"无法识别按键：{rawPart}";
                        return false;
                    }

                    key = parsedKey;
                    break;
            }
        }

        if (key is null)
        {
            error = "快捷键缺少主键。";
            return false;
        }

        if ((modifiers & (HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Shift | HotkeyModifiers.Win)) == 0)
        {
            error = "请至少使用 Ctrl、Alt、Shift 或 Win 中的一个修饰键。";
            return false;
        }

        hotkey = new HotkeyDefinition(modifiers, (uint)key.Value);
        return true;
    }

    public static HotkeyDefinition ParseOrDefault(string text)
    {
        return ParseOrDefault(text, DefaultHotkey);
    }

    public static HotkeyDefinition ParseOrDefault(string text, HotkeyDefinition defaultHotkey)
    {
        return TryParse(text, out var hotkey, out _) ? hotkey : defaultHotkey;
    }

    public static HotkeyDefinition DefaultHotkey { get; } =
        new(HotkeyModifiers.Control | HotkeyModifiers.NoRepeat, (uint)Keys.Space);

    public static HotkeyDefinition DefaultAdminHotkey { get; } =
        new(HotkeyModifiers.Control | HotkeyModifiers.Shift | HotkeyModifiers.NoRepeat, (uint)Keys.Space);

    public bool HasSameGesture(HotkeyDefinition other)
    {
        return Key == other.Key && GetComparableModifiers(Modifiers) == GetComparableModifiers(other.Modifiers);
    }

    private static bool TryParseKey(string part, out Keys key)
    {
        key = Keys.None;

        var aliases = new Dictionary<string, Keys>
        {
            ["SPACE"] = Keys.Space,
            ["ESC"] = Keys.Escape,
            ["ESCAPE"] = Keys.Escape,
            ["ENTER"] = Keys.Enter,
            ["RETURN"] = Keys.Return,
            ["TAB"] = Keys.Tab,
            ["BACKSPACE"] = Keys.Back,
            ["BKSP"] = Keys.Back,
            ["DELETE"] = Keys.Delete,
            ["DEL"] = Keys.Delete,
            ["INSERT"] = Keys.Insert,
            ["INS"] = Keys.Insert,
            ["UP"] = Keys.Up,
            ["DOWN"] = Keys.Down,
            ["LEFT"] = Keys.Left,
            ["RIGHT"] = Keys.Right
        };

        if (aliases.TryGetValue(part, out key))
        {
            return true;
        }

        if (part.Length == 1)
        {
            var c = part[0];
            if (c is >= 'A' and <= 'Z')
            {
                key = (Keys)c;
                return true;
            }

            if (c is >= '0' and <= '9')
            {
                key = Keys.D0 + (c - '0');
                return true;
            }
        }

        if (part.Length is 2 or 3 && part[0] == 'F' && int.TryParse(part[1..], NumberStyles.None, CultureInfo.InvariantCulture, out var fKey) && fKey is >= 1 and <= 24)
        {
            key = Keys.F1 + (fKey - 1);
            return true;
        }

        return Enum.TryParse(part, true, out key) && key != Keys.None;
    }

    private static string Normalize(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            if (!char.IsWhiteSpace(c) && c != '_')
            {
                builder.Append(char.ToUpperInvariant(c));
            }
        }

        return builder.ToString();
    }

    private static string KeyToText(Keys key)
    {
        return key switch
        {
            Keys.Space => "Space",
            Keys.Escape => "Esc",
            Keys.Return => "Enter",
            Keys.Back => "Backspace",
            Keys.Delete => "Delete",
            Keys.Insert => "Insert",
            >= Keys.D0 and <= Keys.D9 => ((char)('0' + key - Keys.D0)).ToString(),
            >= Keys.F1 and <= Keys.F24 => $"F{key - Keys.F1 + 1}",
            _ => key.ToString()
        };
    }

    private static HotkeyModifiers GetComparableModifiers(HotkeyModifiers modifiers)
    {
        return modifiers & ~HotkeyModifiers.NoRepeat;
    }
}
