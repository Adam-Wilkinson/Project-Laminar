﻿using System.ComponentModel;
using Laminar.Contracts.Primitives;
using Laminar.Contracts.UserInterface;

namespace Laminar.Core.UserPreferences;

internal class UserPreference<T> : IUserPreference, IUserPreference<T>, INotificationClient
{
    private readonly T _defaultValue;
    private readonly ValueInfo<T> _valueInfo;

    public UserPreference(IValueDisplayFactory valueDisplayFactory, T defaultValue, string name)
    {
        _valueInfo = new(name, defaultValue);
        ValueDisplay = valueDisplayFactory.CreateValueDisplay(_valueInfo);
        Value = defaultValue;
        _defaultValue = defaultValue;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public IValueDisplay ValueDisplay { get; }

    public T Value { get; set; }

    public void Reset()
    {
        Value = _defaultValue;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
    }

    public void TriggerNotification()
    {
        Value = (T)_valueInfo.BoxedValue;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
    }
}