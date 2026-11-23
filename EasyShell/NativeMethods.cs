using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace EasyShell;

internal static class NativeMethods
{
    public const uint GA_ROOT = 2;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, HotkeyModifiers fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);

    [DllImport("userenv.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CreateEnvironmentBlock(out IntPtr environment, SafeAccessTokenHandle token, bool inherit);

    [DllImport("userenv.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyEnvironmentBlock(IntPtr environment);
}
