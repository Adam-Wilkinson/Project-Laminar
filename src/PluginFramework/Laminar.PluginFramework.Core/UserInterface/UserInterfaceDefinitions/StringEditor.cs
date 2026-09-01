namespace Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

public class StringEditor : IUserInterfaceDefinition
{
    public static readonly InterfaceData<StringEditor, string> DesignInstance = new("Default Value") { Name = "Default String" };
    
    public interface IXamlTarget : IInterfaceData<StringEditor, string>;
}