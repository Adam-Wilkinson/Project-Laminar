using System;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.UserInterface;

public interface IValueInterfaceDefinition
{
    public Type? ValueType { get; }

    public bool IsUserEditable { get; set; }

    public IUserInterfaceDefinition? Editor { get; set; }

    public IUserInterfaceDefinition? Viewer { get; set; }

    public IUserInterfaceDefinition? GetCurrentDefinition();
}
