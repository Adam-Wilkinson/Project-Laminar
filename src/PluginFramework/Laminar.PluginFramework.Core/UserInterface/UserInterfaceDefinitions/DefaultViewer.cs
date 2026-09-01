namespace Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

public class DefaultViewer : IUserInterfaceDefinition
{
    public static readonly InterfaceData<DefaultViewer, None> DesignInstance = new(None.Instance) { Name = "Default Name" };
    
    public interface IXamlTarget : IInterfaceData<DefaultViewer, None>;
}