using System;
using System.Windows.Input;
using Avalonia.Data;
using Avalonia.Input;
using Laminar.Contracts.Base.Settings;
using Laminar.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.KeyBIndings;

internal class LaminarKeyBinding : KeyBinding, ICommand
{
    static readonly string BindingPrefix = $"KeyBindings{ItemCatagory<IUserPreference>.SeparationChar}";

    readonly Action _bindingAction;

    public LaminarKeyBinding(string bindingName, KeyGesture defaultValue, Action bindingAction) : base()
    {
        _bindingAction = bindingAction;
        string bindingKey = $"{BindingPrefix}{bindingName}";
        IUserPreferenceManager userPreferenceManager = App.LaminarInstance.ServiceProvider.GetRequiredService<IUserPreferenceManager>();
        userPreferenceManager.AddPreference(defaultValue, bindingName, bindingKey);
        Command = this;

        this[!GestureProperty] = new Binding
        {
            Source = userPreferenceManager.GetPreference<KeyGesture>(bindingKey),
            Path = nameof(IUserPreference<KeyGesture>.Value),
        };
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter) => true;

    public void Execute(object parameter) => _bindingAction();
}
