namespace Laminar.PluginFramework.Serialization;

public interface INotifySerializedValueChanged : IDisposable
{
    public event EventHandler? SerializedValueChanged;
}