namespace Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

public class BoolTwoButton : IUserInterfaceDefinition
{
    public static readonly InterfaceData<BoolTwoButton, bool> DesignInstance = new() { Name = "Default boolean", Value = true };

    public interface IXamlTarget : IInterfaceData<BoolTwoButton, bool>;

    public string TrueText { get; init; } = "True";

    public string FalseText { get; init; } = "False";
}