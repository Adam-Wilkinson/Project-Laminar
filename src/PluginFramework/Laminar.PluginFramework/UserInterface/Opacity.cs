using System;
using System.ComponentModel;

namespace Laminar.PluginFramework.UserInterface;

public class Opacity : INotifyPropertyChanged
{
    readonly static PropertyChangedEventArgs OpacityPropertyChangedArgs = new(nameof(Value));

    private double _internalValue = 1.0;
    private double _totalFactorsValue = 1.0;

    public Opacity(double initialValue)
    {
        _internalValue = initialValue;
    }

    public Opacity() : this(1.0)
    {
    }

    public double Value => _internalValue * _totalFactorsValue;

    public event PropertyChangedEventHandler? PropertyChanged;

    private event EventHandler<OpacityChangedEventArgs>? OpacityChanged;

    public void SetInternalValue(double value)
    {
        double oldValue = Value;
        _internalValue = value;
        SendEvents(oldValue);
    }

    public void AddFactor(Opacity factor)
    {
        double oldValue = Value;
        _totalFactorsValue *= factor.Value;
        factor.OpacityChanged += Factor_OpacityChanged;
        SendEvents(oldValue);
    }

    public void RemoveFactor(Opacity factor)
    {
        double oldValue = Value;
        _totalFactorsValue /= factor.Value;
        factor.OpacityChanged -= Factor_OpacityChanged;
        SendEvents(oldValue);
    }

    private void Factor_OpacityChanged(object? sender, OpacityChangedEventArgs e)
    {
        double oldValue = Value;
        _totalFactorsValue *= e.NewValue / e.OldValue;
        SendEvents(oldValue);
    }

    private class OpacityChangedEventArgs : EventArgs
    {
        public required double OldValue { get; init; }

        public required double NewValue { get; init; }
    }

    private void SendEvents(double oldValue)
    {
        OpacityChanged?.Invoke(this, new OpacityChangedEventArgs { OldValue = oldValue, NewValue = Value });
        PropertyChanged?.Invoke(this, OpacityPropertyChangedArgs);
    }
}
