/*
   Test Task
   Please implement a program that synchronizes two folders: source and
   replica. The program should maintain a full, identical copy of source
   folder at replica folder. Solve the test task by writing a program in C#.

   Synchronization must be one-way: after the synchronization content of the
   replica folder should be modified to exactly match content of the source
   folder;

   ✓ Synchronization should be performed periodically;

   ✓ File creation/copying/removal operations should be logged to a file and to the
   console output;

   Folder paths, synchronization interval and log file path should be provided
   using the command line arguments;

   It is undesirable to use third-party libraries that implement folder
   synchronization;

   It is allowed (and recommended) to use external libraries implementing other
   well-known algorithms. For example, there is no point in implementing yet
   another function that calculates MD5 if you need it for the task – it is perfectly
   acceptable to use a third-party (or built-in) library
*/

// Inputs
// Which folder should be looked at
// Which folder to copy
// Which log file
// Interval in seconds for example

using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Runtime.InteropServices;
using TestTask.Classes;

namespace TestTask;

using System;
using System.IO;
using System.CommandLine;

class Program
{
    static async Task Main(string[] args)
    {
        var sourceFolderOption = new Option<string>(
            name: "--source",
            description: "The folder which the program will be looking for changes."
        )
        {
            IsRequired = true
        };
        //sourceFolderOption.AddValidator(IsExistingDirectory);

        var replicaFolderOption = new Option<string>(
            name: "--replica",
            description: "The folder that will copy the content from source folder"
        )
        {
            IsRequired = true
        };
        //replicaFolderOption.AddValidator(IsExistingDirectory);

        var logFileOption = new Option<string>(
            name: "--log",
            description: "The file to keep in track the changes from source folder"
        )
        {
            IsRequired = true
        };
        /*
        logFileOption.AddValidator(result =>
        {
            var value = result.GetValueOrDefault<string>();
            if (string.IsNullOrEmpty(value))
            {
                result.ErrorMessage = "The log file path cannot be empty.";
            }
            else
            {
                var directory = Path.GetDirectoryName(value);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    result.ErrorMessage = $"The directory for the log file '{value}' does not exist.";
                }
            }
        });
        */

        var intervalOption = new Option<int>(
            name: "--interval",
            description: "The time in seconds that the program waits to copy the new changes"
        )
        {
            IsRequired = true
        };
        /*
        intervalOption.AddValidator(result => 
        {
            var value = result.GetValueOrDefault<int>();
            if (value < 1)
            {
                result.ErrorMessage = "The interval must be greater than or equal to 1.";
            }
        });
        */

        var rootCommand = new RootCommand("TestTask")
        {
            sourceFolderOption,
            replicaFolderOption,
            logFileOption,
            intervalOption
        };

        rootCommand.SetHandler(MainOperations.MainCommand, sourceFolderOption, replicaFolderOption, logFileOption,
            intervalOption);

        var commandLineBuilder = new CommandLineBuilder(rootCommand);

        commandLineBuilder.AddMiddleware(async (context, next) => { await next(context); });

        commandLineBuilder.UseDefaults();
        Parser parser = commandLineBuilder.Build();

        await parser.InvokeAsync(args);
    }

    // The path parameter is permitted to specify relative or absolute path information.
    // Relative path information is interpreted as relative to the current working directory.
    private static void IsExistingDirectory(OptionResult result)
    {
        // IsItWellFormatted
        // We don't care if it is full or relative
        
        bool exists = File.Exists(result.GetValueOrDefault<string>());
    }
    
    // Windows
    // C:\Documents\Newsletters\Summer2018.pdf
    // \Program Files\Custom Utilities\StringFinder.exe
    // 2018\January.xlsx
    
    // Unix
    // ./Documents/Summer2018.pdf
    // /Users/antonio/Documents/Summer2018.pdf
    private static void IsDirectoryWellFormatted(OptionResult result)
    {
    }

    private static bool IsFullOrRelativePath(string path)
    {
        return false;
    }

    // Check depending on the OS
    // Linux   -> Directories & Files are Case Sensitive
    // OSx     -> Directories & Files are Case Sensitive
    // Windows -> Directories & Files ...
    // Perhaps one is passed as full and the other one as relative
    private static bool AreSameFolders(string source, string replica)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
        }

        return false;
    }
    
    // Check is passed path is correct as with the others
    // Check as well if it contains filename ->
    //      True Check if file type is txt -> True then use that one -> False then say that file format is not supported
    //      False Create Default Log File -> What if there is already one with the same name
    private static void IsLogFileCreated(OptionResult result) { }
}