namespace TestTask.Classes
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Timers;
    
    internal class MainOperations
    {
        private static FileSystemWatcher? _fileSystemWatcher;
        private static Timer? _timer;
        private static readonly List<(string, WatcherChangeTypes)> Changes = new List<(string, WatcherChangeTypes)>();
        private static string? _sourceFolder;
        private static string? _replicaFolder;
        private static string? _logFilePath;
        private static int _interval;
        
        public static void MainCommand(string source, string replica, string log, int interval)
        {
            // Initialize variables
            _sourceFolder = source;
            _replicaFolder = replica;
            _logFilePath = log;
            _interval = interval * 1000;
            
            // Initial copy content from source to replica
            CopyDirectoryContents(_sourceFolder, _replicaFolder);
            
            // Initialize FileSystemWatcher
            _fileSystemWatcher = new FileSystemWatcher(source);
            _fileSystemWatcher.IncludeSubdirectories = true;
            _fileSystemWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
            _fileSystemWatcher.Changed += OnChanged;
            _fileSystemWatcher.Created += OnCreated;
            _fileSystemWatcher.Deleted += OnDeleted;
            _fileSystemWatcher.Renamed += OnRenamed;
            _fileSystemWatcher.EnableRaisingEvents = true;

            // Initialize Timer
            _timer = new Timer(_interval);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;
            
            Console.WriteLine("Monitoring started. Press 'q' to quit.");
            Console.ReadKey();
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
                            LogChange(path, $"{changeType} applied to {replicaPath}");
                        }
                        else if (Directory.Exists(path))
                        {
                            Directory.CreateDirectory(replicaPath);
                            LogChange(path, $"{changeType} applied to {replicaPath}");
                        }

                        break;
                    case WatcherChangeTypes.Deleted:
                        if (File.Exists(replicaPath))
                        {
                            File.Delete(replicaPath);
                            LogChange(path, $"Deleted from {replicaPath}");
                        }
                        else if (Directory.Exists(replicaPath))
                        {
                            Directory.Delete(replicaPath, true);
                            LogChange(path, $"Deleted from {replicaPath}");
                        }

                        break;
                }
            }
        }

        private static void LogChange(string path, string change)
        {
            string log = $"{DateTime.Now}: {change} - {path}{Environment.NewLine}";
            Console.WriteLine(log);
            
            lock (_logFilePath!)
            {
                File.AppendAllText(_logFilePath, log);
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
    }
}
