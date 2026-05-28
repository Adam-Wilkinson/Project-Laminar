namespace Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

public class ToggleSwitch : IUserInterfaceDefinition
{
    public static readonly InterfaceData<ToggleSwitch, bool> DesignInstance = new() { Name = "Default Name", Value = true };
    
    public interface IXamlTarget : IInterfaceData<ToggleSwitch, bool>;
}