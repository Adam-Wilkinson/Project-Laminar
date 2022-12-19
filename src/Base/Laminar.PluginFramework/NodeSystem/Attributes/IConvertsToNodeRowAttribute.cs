using System.Reflection;

namespace Laminar.PluginFramework.NodeSystem.Attributes;

public interface IConvertsToNodeRowAttribute
{
    public NodeRow GenerateNodeRow(PropertyInfo childProperty, object containingObject);
}
