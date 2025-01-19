namespace Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

public class StringViewer : IUserInterfaceDefinition
{
    public static readonly InterfaceData<StringViewer, object> DesignInstance = new() { Name = "Default Name", Value = "Default Value" };
    
    public interface IXamlTarget : IInterfaceData<StringViewer, object>;

    public int MaxStringLength { get; set; } = 20;
}