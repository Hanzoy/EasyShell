using System.Runtime.InteropServices;

namespace EasyShell;

internal static class ExplorerPathResolver
{
    public static string GetForegroundExplorerPath()
    {
        var foreground = NativeMethods.GetForegroundWindow();
        if (foreground == IntPtr.Zero)
        {
            return string.Empty;
        }

        var root = NativeMethods.GetAncestor(foreground, NativeMethods.GA_ROOT);
        if (root == IntPtr.Zero)
        {
            root = foreground;
        }

        object? shell = null;
        object? windows = null;
        try
        {
            var shellType = Type.GetTypeFromProgID("Shell.Application");
            if (shellType is null)
            {
                return string.Empty;
            }

            shell = Activator.CreateInstance(shellType);
            if (shell is null)
            {
                return string.Empty;
            }

            windows = ((dynamic)shell).Windows();
            foreach (dynamic window in (dynamic)windows)
            {
                try
                {
                    if (new IntPtr(Convert.ToInt64(window.HWND)) != root)
                    {
                        continue;
                    }

                    string path = window.Document.Folder.Self.Path;
                    return Directory.Exists(path) ? path : string.Empty;
                }
                catch
                {
                    // Some Shell windows are virtual folders or browser surfaces without a filesystem path.
                }
                finally
                {
                    TryReleaseComObject(window);
                }
            }
        }
        catch
        {
            return string.Empty;
        }
        finally
        {
            TryReleaseComObject(windows);
            TryReleaseComObject(shell);
        }

        return string.Empty;
    }

    private static void TryReleaseComObject(object? value)
    {
        if (value is not null && Marshal.IsComObject(value))
        {
            Marshal.FinalReleaseComObject(value);
        }
    }
}
