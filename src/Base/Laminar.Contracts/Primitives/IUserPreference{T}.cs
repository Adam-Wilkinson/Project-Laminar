using System.ComponentModel;

namespace Laminar.Contracts.Primitives;

public interface IUserPreference<T> : INotifyPropertyChanged
{
    public T Value { get; set; }
}
