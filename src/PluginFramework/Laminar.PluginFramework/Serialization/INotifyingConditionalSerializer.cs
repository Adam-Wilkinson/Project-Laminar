namespace Laminar.PluginFramework.Serialization;

public interface INotifyingConditionalSerializer : IConditionalSerializer
{
    public INotifySerializedValueChanged GetSerializedValueChangedNotifier(object target);
}