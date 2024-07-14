namespace TestTask;

using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;

public class MainOperations
{
    private static FileSystemWatcher? _fileSystemWatcher;
    private static Timer? _timer;
    private static readonly List<(string, WatcherChangeTypes)> Changes = new List<(string, WatcherChangeTypes)>();
    private static string? _sourceFolder;
    private static string? _replicaFolder;
    private static string? _logFilePath;

    public static void MainCommand(string source, string replica, string log, int interval)
    {
        InitVariables(source, replica, log, interval);
        Validator.CheckAreNotSameDirectory(_sourceFolder!, _replicaFolder!);
        CreateLogFile();
        CopyDirectoryContents(source, replica);

        Console.WriteLine("Monitoring started. Press any key to quit.");
        Console.ReadKey();
    }

    private static void InitVariables(string source, string replica, string log, int interval)
    {
        // Initialize variables
        _sourceFolder = source;
        _replicaFolder = replica;
        _logFilePath = Path.Combine(log, "logs.txt");

        // Initialize FileSystemWatcher
        _fileSystemWatcher = new FileSystemWatcher(source);
        _fileSystemWatcher.IncludeSubdirectories = true;
        _fileSystemWatcher.NotifyFilter =
            NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
        _fileSystemWatcher.Changed += OnChanged;
        _fileSystemWatcher.Created += OnCreated;
        _fileSystemWatcher.Deleted += OnDeleted;
        _fileSystemWatcher.Renamed += OnRenamed;
        _fileSystemWatcher.EnableRaisingEvents = true;

        // Initialize Timer
        _timer = new Timer(interval * 1000);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
        _timer.Enabled = true;
    }
    
    private static void OnCreated(object sender, FileSystemEventArgs e)
    {
        lock (Changes)
        {
            Changes.Add((e.FullPath, WatcherChangeTypes.Created));
            LogChange(e.FullPath, WatcherChangeTypes.Created.ToString());
        }
    }

    private static void OnDeleted(object sender, FileSystemEventArgs e)
    {
        lock (Changes)
        {
            Changes.Add((e.FullPath, WatcherChangeTypes.Deleted));
            LogChange(e.FullPath, WatcherChangeTypes.Deleted.ToString());
        }
    }

    private static void OnChanged(object sender, FileSystemEventArgs e)
    {
        lock (Changes)
        {
            Changes.Add((e.FullPath, WatcherChangeTypes.Changed));
            LogChange(e.FullPath, WatcherChangeTypes.Changed.ToString());
        }
    }

    private static void OnRenamed(object sender, RenamedEventArgs e)
    {
        lock (Changes)
        {
            Changes.Add((e.OldFullPath, WatcherChangeTypes.Deleted));
            Changes.Add((e.FullPath, WatcherChangeTypes.Created));
            LogChange(e.OldFullPath, WatcherChangeTypes.Deleted.ToString());
            LogChange(e.FullPath, WatcherChangeTypes.Created.ToString());
        }
    }

    private static void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        List<(string, WatcherChangeTypes)> changesCopy;

        lock (Changes)
        {
            changesCopy = [..Changes];
            Changes.Clear();
        }

        foreach (var (path, changeType) in changesCopy)
        {
            string relativePath = Path.GetRelativePath(_sourceFolder!, path);
            string replicaPath = Path.Combine(_replicaFolder!, relativePath);

            switch (changeType)
            {
                case WatcherChangeTypes.Created:
                case WatcherChangeTypes.Changed:
                    if (File.Exists(path))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(replicaPath)!);
                        File.Copy(path, replicaPath, true);
                    }
                    else if (Directory.Exists(path))
                    {
                        Directory.CreateDirectory(replicaPath);
                    }

                    break;
                case WatcherChangeTypes.Deleted:
                    if (File.Exists(replicaPath))
                    {
                        File.Delete(replicaPath);
                    }
                    else if (Directory.Exists(replicaPath))
                    {
                        Directory.Delete(replicaPath, true);
                    }

                    break;
            }
        }
    }

    private static void LogChange(string path, string change)
    {
        string currentChange = $"{DateTime.Now}: {change} - {path}{Environment.NewLine}";
        Console.WriteLine(currentChange);

        lock (_logFilePath!)
        {
            File.AppendAllText(_logFilePath, currentChange);
        }
    }

    private static void CopyDirectoryContents(string source, string replica)
    {
        foreach (var file in Directory.GetFiles(source))
        {
            string destFile = Path.Combine(replica, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        foreach (var directory in Directory.GetDirectories(source))
        {
            string destSubDir = Path.Combine(replica, Path.GetFileName(directory));
            Directory.CreateDirectory(destSubDir);
            CopyDirectoryContents(directory, destSubDir);
        }
    }
    
    private static void CreateLogFile()
    {
        if (!File.Exists(_logFilePath))
        {
            File.Create(_logFilePath!);
        }
    }
}