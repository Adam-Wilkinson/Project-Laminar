using System.CommandLine;
using Laminar.PluginFramework.CLI.Commands;

return await new RootCommand("Project: Laminar plugin management tool for dotnet projects")
    .AddPackCommand()
    .Parse(args)
    .InvokeAsync();