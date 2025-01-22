namespace Laminar.Avalonia.Settings;

public class OptionsSettingValue<T> : OptionsSettingValue where T : notnull
{
    public new T Value
    {
        get => (T)base.Value!;
        set => base.Value = value;
    }
}

public class OptionsSettingValue
{
    public required string Name { get; set; }

    public object? Value { get; set; }

    public override string ToString() => Name;
}