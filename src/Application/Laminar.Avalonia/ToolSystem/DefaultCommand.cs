using System;
using System.Windows.Input;

namespace Laminar.Avalonia.ToolSystem;

public class DefaultCommand : ICommand
{
    public static readonly ICommand Instance = new DefaultCommand();
    
    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter)
    {
    }

#pragma warning disable CS0067
    public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
}