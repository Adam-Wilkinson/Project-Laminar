using System.Collections.Specialized;

namespace Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

public class EnumDropdown : IUserInterfaceDefinition
{
    public static readonly InterfaceData<EnumDropdown, object> Default = new() { Value = NotifyCollectionChangedAction.Add, Name = "Default Enum"};
    
    public interface IXamlTarget : IInterfaceData<EnumDropdown, object>;
}