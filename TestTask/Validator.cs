using System.CommandLine.Parsing;

namespace TestTask;

public static class Validator
{
    public static void IsExistingDirectory(OptionResult result)
    {
        var exists = Directory.Exists(result.GetValueOrDefault<string>());
        if (!exists)
        {
            result.ErrorMessage = $"The directory {result.GetValueOrDefault<string>()} doesn't exist or it is not well formatted.";
        }
    }

    public static void IsValidInterval(OptionResult result)
    {
        var value = result.GetValueOrDefault<int>();
        if (value < 1)
        {
            result.ErrorMessage = "The interval must be greater than or equal to 1.";
        }
    }
    
    private static string CastToFullPath(string path)
    {
        return Path.GetFullPath(path);
    }
    
    public static void CheckAreNotSameDirectory(string path, string path2)
    {
        var sourceFullPath = CastToFullPath(path);
        var replicaFullPath = CastToFullPath(path2);
        bool sameFolder = false;

        if (OperatingSystem.IsWindows())
        {
            sameFolder = string.Equals(sourceFullPath, replicaFullPath, StringComparison.OrdinalIgnoreCase);
            if (!sameFolder) return;
        }

        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            sameFolder = string.Equals(sourceFullPath, replicaFullPath, StringComparison.Ordinal);
            if (!sameFolder) return;
        }

        if (sameFolder)
        {
            throw new Exception("Source folder and Directory folder should not be the same");
        }

        throw new Exception("Script is only valid for Linux, MacOS and Windows");
    }
}