using System;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.UserInterface;

public class ValueInterfaceDefinition
{
    public Type? ValueType { get; init; }

    public required bool IsUserEditable { get; set; }

    public IUserInterfaceDefinition? Editor { get; set; }

    public IUserInterfaceDefinition? Viewer { get; set; }
}
