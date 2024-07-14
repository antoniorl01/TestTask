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

   ✓ Folder paths, synchronization interval and log file path should be provided
   using the command line arguments;

   ✓ It is undesirable to use third-party libraries that implement folder
   synchronization;

   It is allowed (and recommended) to use external libraries implementing other
   well-known algorithms. For example, there is no point in implementing yet
   another function that calculates MD5 if you need it for the task – it is perfectly
   acceptable to use a third-party (or built-in) library
*/

namespace TestTask;

using System.CommandLine.Builder;
using System.CommandLine.Parsing;
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
        sourceFolderOption.AddValidator(IsExistingDirectory);

        var replicaFolderOption = new Option<string>(
            name: "--replica",
            description: "The folder that will copy the content from source folder"
        )
        {
            IsRequired = true
        };
        replicaFolderOption.AddValidator(IsExistingDirectory);

        var logFileOption = new Option<string>(
            name: "--log",
            description: "The folder where a log file will be created to keep in track the changes from source folder"
        )
        {
            IsRequired = true
        };
        
        logFileOption.AddValidator(IsExistingDirectory);

        var intervalOption = new Option<int>(
            name: "--interval",
            description: "The time in seconds that the program waits to apply the new changes"
        )
        {
            IsRequired = true
        };
        intervalOption.AddValidator(IsValidInterval);
        

        var rootCommand = new RootCommand("TestTask")
        {
            sourceFolderOption,
            replicaFolderOption,
            logFileOption,
            intervalOption
        };

        rootCommand.SetHandler(MainOperations.MainCommand, sourceFolderOption, replicaFolderOption, logFileOption, intervalOption);

        var commandLineBuilder = new CommandLineBuilder(rootCommand);

        commandLineBuilder.AddMiddleware(async (context, next) => { await next(context); });

        commandLineBuilder.UseDefaults();
        Parser parser = commandLineBuilder.Build();
        
        await parser.InvokeAsync(args);
    }

    private static void IsExistingDirectory(OptionResult result)
    {
        var exists = Directory.Exists(result.GetValueOrDefault<string>());
        if (!exists)
        {
            result.ErrorMessage = $"The directory {result.GetValueOrDefault<string>()} doesn't exist or it is not well formatted.";
        }
    }

    private static void IsValidInterval(OptionResult result)
    {
        var value = result.GetValueOrDefault<int>();
        if (value < 1)
        {
            result.ErrorMessage = "The interval must be greater than or equal to 1.";
        }
    }
}