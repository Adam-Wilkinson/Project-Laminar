using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Laminar.Avalonia.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [RelayCommand]
    private void Undo()
    {
        throw new NotImplementedException();
    }
}