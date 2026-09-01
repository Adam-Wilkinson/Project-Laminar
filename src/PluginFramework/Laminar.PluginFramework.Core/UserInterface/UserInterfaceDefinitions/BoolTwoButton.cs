namespace Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

public class BoolTwoButton : IUserInterfaceDefinition
{
    public static readonly InterfaceData<BoolTwoButton, bool> DesignInstance = new(true) { Name = "Default boolean" };

    public interface IXamlTarget : IInterfaceData<BoolTwoButton, bool>;

    public string TrueText { get; init; } = "True";

    public string FalseText { get; init; } = "False";
}