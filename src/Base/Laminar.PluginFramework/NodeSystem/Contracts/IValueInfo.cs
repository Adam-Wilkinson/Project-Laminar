using System;
using Laminar.PluginFramework.UserInterfaces;

namespace Laminar.PluginFramework.NodeSystem.Contracts;

public interface IValueInfo
{
    public string Name { get; }

    public Type? ValueType { get; }

    public bool IsUserEditable { get; }

    public IUserInterfaceDefinition? Editor { get; }

    public IUserInterfaceDefinition? Viewer { get; }

    public object? BoxedValue { get; set; }
}
