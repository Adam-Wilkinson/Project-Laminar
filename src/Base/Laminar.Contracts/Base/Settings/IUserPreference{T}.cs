using System.ComponentModel;

namespace Laminar.Contracts.Base.Settings;

public interface IUserPreference<T> : INotifyPropertyChanged
{
    public T Value { get; set; }
}
