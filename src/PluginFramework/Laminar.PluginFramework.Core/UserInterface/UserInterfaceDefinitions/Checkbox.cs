namespace Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

public class Checkbox : IUserInterfaceDefinition
{
    public static readonly InterfaceData<Checkbox, bool> DesignInstance = new() { Value = true, Name = "Test Checkbox" };
    
    public interface IXamlTarget : IInterfaceData<Checkbox, bool>;
}