using System.ComponentModel;
using Laminar_PluginFramework.UserInterfaces;

namespace Laminar.Contracts.UserInterface;

public interface IUserInterface : INotifyPropertyChanged
{
    public IDisplayValue DisplayValue { get; }

    public object Interface { get; }

    public void Refresh();
}
