using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Laminar.Avalonia.ViewModels;

namespace Laminar.Avalonia;
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        string? name = data.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        Type? type = Type.GetType(name);

        if (type != null)
        {
            Control? control = (Control)Activator.CreateInstance(type)!;
            control.DataContext = data;
            return control;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
