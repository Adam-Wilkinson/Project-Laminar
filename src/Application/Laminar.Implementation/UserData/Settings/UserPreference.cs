using System.ComponentModel;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Notification;
using Laminar.Contracts.UserData.Settings;

namespace Laminar.Implementation.UserData.Settings;

internal class UserPreference<T> : IUserPreference<T>, INotificationClient
{
    private readonly T _defaultValue;

    public UserPreference(
        IDisplayFactory valueDisplayFactory, 
        T defaultValue, 
        string name)
    {
        SettingsDisplayValue<T> valueInfo = new(name, defaultValue);
        Display = valueDisplayFactory.CreateDisplayForValue(valueInfo);
        Value = defaultValue;
        _defaultValue = defaultValue;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IDisplay Display { get; }

    public T Value { get; set; }

    public void Reset()
    {
        Value = _defaultValue;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
    }

    public void TriggerNotification()
    {
        Display.Refresh();
    }
}
