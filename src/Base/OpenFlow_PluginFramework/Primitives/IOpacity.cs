using System.ComponentModel;

namespace OpenFlow_PluginFramework.Primitives
{
    public interface IOpacity : IObservableValue<double>
    {
        void AddOpacityFactor(IOpacity factor);

        bool RemoveOpacityFactor(IOpacity factor);
    }
}