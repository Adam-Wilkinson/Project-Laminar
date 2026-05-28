namespace Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

public class StringEditor : IUserInterfaceDefinition
{
    public static readonly InterfaceData<StringEditor, string> DesignInstance = new() { Name = "Default String", Value = "Default Value" };
    
    public interface IXamlTarget : IInterfaceData<StringEditor, string>;
}