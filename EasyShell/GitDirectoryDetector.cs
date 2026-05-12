namespace EasyShell;

internal static class GitDirectoryDetector
{
    public static bool IsInGitDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            return false;
        }

        var directory = new DirectoryInfo(path);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")) ||
                File.Exists(Path.Combine(directory.FullName, ".git")))
            {
                return true;
            }

            directory = directory.Parent;
        }

        return false;
    }
}
