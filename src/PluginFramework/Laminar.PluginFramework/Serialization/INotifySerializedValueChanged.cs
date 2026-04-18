namespace Laminar.PluginFramework.Serialization;

public interface INotifySerializedValueChanged
{
    public event EventHandler? SerializedValueChanged;
}