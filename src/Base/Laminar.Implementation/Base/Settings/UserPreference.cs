﻿using System.ComponentModel;
using Laminar.Contracts.Base.Settings;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Primitives;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Base.Settings;

internal class UserPreference<T> : IUserPreference, IUserPreference<T>, INotificationClient
{
    private readonly T _defaultValue;
    private readonly DisplayValue<T> _valueInfo;

    public UserPreference(
        IDisplayFactory valueDisplayFactory, 
        IUserInterfaceDefinitionFinder uiFinder,
        T defaultValue, 
        string name)
    {
        _valueInfo = new(uiFinder, name, defaultValue);
        Display = valueDisplayFactory.CreateDisplayForValue(_valueInfo);
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
