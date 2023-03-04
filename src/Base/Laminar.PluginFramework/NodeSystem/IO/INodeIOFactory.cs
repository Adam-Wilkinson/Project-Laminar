using System;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.NodeSystem.IO;

public interface INodeIOFactory
{
    public IValueInput<T> ValueInput<T>(string valueName, T initialValue, IUserInterfaceDefinition? editor = null, IUserInterfaceDefinition? viewer = null, Action<T>? setter = null);

    public IValueOutput<T> ValueOutput<T>(string valueName, T initialValue, IUserInterfaceDefinition? viewer = null, IUserInterfaceDefinition? editor = null, bool isUserEditable = false, Func<T>? getter = null);
}
