namespace Laminar.Domain.Exceptions;

public class InvalidPluginFormatException(string pluginFileName) : Exception($"Unable to read te file {pluginFileName} since it is in an invalid format")
{
    public string PluginFileName { get; } = pluginFileName;
}