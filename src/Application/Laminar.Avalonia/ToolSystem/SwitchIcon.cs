using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Laminar.Domain.Observables;

namespace Laminar.Avalonia.ToolSystem;

public class SwitchIcon : TemplatedControl
{
    public static readonly StyledProperty<bool> IsOnProperty =
        AvaloniaProperty.Register<SwitchIcon, bool>(nameof(IsOn), defaultBindingMode: BindingMode.TwoWay);

    public bool IsOn
    {
        get => GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }
}
