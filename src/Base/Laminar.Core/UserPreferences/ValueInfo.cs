using System;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.UserInterfaces;

namespace Laminar.Core.UserPreferences;

internal class ValueInfo<T> : IValueInfo
{
    public ValueInfo(string name, T value)
    {
        Name = name;
        ValueType = typeof(T);
        BoxedValue = value;
    }

    public string Name { get; }
    public Type ValueType { get; }
    public bool IsUserEditable { get; } = true;
    public IUserInterfaceDefinition Editor { get; }
    public IUserInterfaceDefinition Viewer { get; }
    public object BoxedValue { get; set; }
}
