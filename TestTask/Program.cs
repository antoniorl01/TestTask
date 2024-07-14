namespace TestTask;

using System.CommandLine.Builder;
using System.CommandLine.Parsing;
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
        sourceFolderOption.AddValidator(Validator.IsExistingDirectory);

        var replicaFolderOption = new Option<string>(
            name: "--replica",
            description: "The folder that will copy the content from source folder"
        )
        {
            IsRequired = true
        };
        replicaFolderOption.AddValidator(Validator.IsExistingDirectory);

        var logFileOption = new Option<string>(
            name: "--log",
            description: "The folder where a log file will be created to keep in track the changes from source folder"
        )
        {
            IsRequired = true
        };
        
        logFileOption.AddValidator(Validator.IsExistingDirectory);

        var intervalOption = new Option<int>(
            name: "--interval",
            description: "The time in seconds that the program waits to apply the new changes"
        )
        {
            IsRequired = true
        };
        intervalOption.AddValidator(Validator.IsValidInterval);
        

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
}