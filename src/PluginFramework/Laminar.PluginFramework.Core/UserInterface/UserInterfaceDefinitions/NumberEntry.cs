namespace Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

public class NumberEntry : IUserInterfaceDefinition
{
    public static readonly InterfaceData<NumberEntry, double> DesignInstance = new() { Name = "Default Number", Value = 5.0, Definition = new NumberEntry { FormatString = "{0:0} ms" }};
    
    public interface IXamlTarget : IInterfaceData<NumberEntry, double>;

    public string FormatString { get; init; } = "";
}