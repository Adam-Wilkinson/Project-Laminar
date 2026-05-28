namespace Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

public class DefaultViewer : IUserInterfaceDefinition
{
    public static readonly InterfaceData<DefaultViewer, None> DesignInstance = new() { Name = "Default Name", Value = None.Instance};
    
    public interface IXamlTarget : IInterfaceData<DefaultViewer, None>;
}