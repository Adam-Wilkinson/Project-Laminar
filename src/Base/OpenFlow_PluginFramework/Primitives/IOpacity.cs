using System.ComponentModel;

namespace Laminar_PluginFramework.Primitives
{
    public interface IOpacity : INotifyPropertyChanged
    {
        double Value { get; set; }

        void AddOpacityFactor(IOpacity factor);

        bool RemoveOpacityFactor(IOpacity factor);

        IOpacity Clone();
    }
}