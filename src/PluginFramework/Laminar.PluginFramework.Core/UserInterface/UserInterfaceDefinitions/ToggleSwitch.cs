namespace Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

public class ToggleSwitch : IUserInterfaceDefinition
{
    public static readonly InterfaceData<ToggleSwitch, bool> DesignInstance = new(true) { Name = "Default Name" };
    
    public interface IXamlTarget : IInterfaceData<ToggleSwitch, bool>;
}