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

        public event EventHandler? CanExecuteChanged;
    }

    private class DragEnded : ICommand
    {
        public bool CanExecute(object? parameter) => parameter is ITreeViewItemViewModel;

        public void Execute(object? parameter)
        {
            if (parameter is not ITreeViewItemViewModel item) return;
            item.IsExpanded = _currentDragTargetShouldBeExpanded;
        }

        public event EventHandler? CanExecuteChanged;
    }
}