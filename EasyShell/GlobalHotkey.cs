using System.ComponentModel;
using System.Runtime.InteropServices;

namespace EasyShell;

internal sealed class GlobalHotkey : NativeWindow, IDisposable
{
    private const int HotkeyId = 0x4548;
    private const int WmHotkey = 0x0312;

    private bool _registered;

    public event EventHandler? Pressed;

    public GlobalHotkey()
    {
        CreateHandle(new CreateParams());
    }

    public void Register(HotkeyDefinition hotkey)
    {
        Unregister();

        if (!NativeMethods.RegisterHotKey(Handle, HotkeyId, hotkey.Modifiers, hotkey.Key))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"无法注册快捷键 {hotkey.DisplayText}。它可能已被其他程序占用。");
        }

        _registered = true;
    }

    public void Unregister()
    {
        if (!_registered)
        {
            return;
        }

        NativeMethods.UnregisterHotKey(Handle, HotkeyId);
        _registered = false;
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotkey && m.WParam.ToInt32() == HotkeyId)
        {
            Pressed?.Invoke(this, EventArgs.Empty);
            return;
        }

        base.WndProc(ref m);
    }

    public void Dispose()
    {
        Unregister();
        DestroyHandle();
    }
}
