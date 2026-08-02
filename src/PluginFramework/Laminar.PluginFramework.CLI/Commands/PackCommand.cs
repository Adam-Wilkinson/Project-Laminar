using System.CommandLine;
using Laminar.PluginFramework.CLI.Packing;

namespace Laminar.PluginFramework.CLI.Commands;

public static class PackCommand 
{
    extension(Command parentCommand)
    {
        public Command AddPackCommand()
        {
            var projectArgument = new Argument<FileInfo>("Project file");

            var outputOption = new Option<DirectoryInfo?>("--output", "-o")
            {
                DefaultValueFactory = _ => null
            };

            Command packCommand = new("pack", "Used to package a plugin into a .plpkg file")
            {
                projectArgument,
                outputOption
            };

            packCommand.SetAction((result, ct) => PackageBuilder.BuildAsync(
                result.GetValue(projectArgument)!, 
                result.GetValue(outputOption),
                ct));
            
            parentCommand.Add(packCommand);
            
            return parentCommand;
        }
    }
}