using System;
using System.Windows.Input;
using Avalonia.Controls;
using Laminar.Avalonia.ViewModels;

namespace Laminar.Avalonia.DragDrop;

public static class TreeViewItemDrag
{
    private static bool _currentDragTargetShouldBeExpanded;
    
    public static ICommand StartedCommand { get; } = new DragStarted();
    
    public static ICommand AnimateHomeCommand { get; } = new DragEnded();

    private class DragStarted : ICommand
    {
        public bool CanExecute(object? parameter) => parameter is ITreeViewItemViewModel;

        public void Execute(object? parameter)
        {
            if (parameter is not ITreeViewItemViewModel item) return;

            _currentDragTargetShouldBeExpanded = item.IsExpanded;
            item.IsExpanded = false;
        }

#pragma warning disable CS0067
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
    }

    private class DragEnded : ICommand
    {
        public bool CanExecute(object? parameter) => parameter is ITreeViewItemViewModel;

        public void Execute(object? parameter)
        {
            if (parameter is not ITreeViewItemViewModel item) return;
            item.IsExpanded = _currentDragTargetShouldBeExpanded;
        }

#pragma warning disable CS0067
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
    }
}