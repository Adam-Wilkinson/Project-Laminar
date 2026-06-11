using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Scripting.NodeComponents;

/// <summary>
/// The node row serializer simply redirects to serializing the central display 
/// </summary>
/// <param name="serializer"></param>
public class NodeRowSerializer(ISerializer serializer) : INotifyingConditionalSerializer
{
    public Type? SerializedTypeOrNull(Type typeToSerialize)
    {
        if (typeof(INodeRow).IsAssignableFrom(typeToSerialize) 
            && typeToSerialize
                .GetInterfaces()
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(INodeRow<>)) 
                is { } genericNodeRowInterface)
        {
            return serializer.GetSerializedType(genericNodeRowInterface.GetGenericArguments()[0]);
        }

        return null;
    }

    public object Serialize(object toSerialize) => toSerialize is not INodeRow nodeRow
        ? throw new InvalidOperationException()
        : serializer.SerializeObject(nodeRow.CentralDisplay, nodeRow.CentralDisplay.GetType());

    public object DeSerialize(DeserializationRequest request)
    {
        if (request.ExistingInstance is not INodeRow existingNodeRow)
        {
            throw new InvalidOperationException();
        }
        
        return serializer.DeserializeObject(request with
        {
            ExistingInstance = existingNodeRow.CentralDisplay,
            TargetType = existingNodeRow.CentralDisplay.GetType()
        });
    }

    public INotifySerializedValueChanged GetSerializedValueChangedNotifier(object target) 
        => target is not INodeRow nodeRowTarget
            ? throw new InvalidOperationException()
            : serializer.GetSerializedValueChangedNotifier(nodeRowTarget.CentralDisplay,
                nodeRowTarget.CentralDisplay.GetType());
}