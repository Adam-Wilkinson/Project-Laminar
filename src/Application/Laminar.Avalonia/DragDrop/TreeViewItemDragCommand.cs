using System;
using System.Windows.Input;
using Avalonia.Controls;

namespace Laminar.Avalonia.DragDrop;

public class TreeViewItemDrag
{
    private static bool _currentDragTargetShouldBeExpanded;
    
    public static ICommand StartedCommand { get; } = new DragStarted();
    
    public static ICommand EndedCommand { get; } = new DragEnded();

    private class DragStarted : ICommand
    {
        public bool CanExecute(object? parameter) => parameter is TreeViewItem;

        public void Execute(object? parameter)
        {
            if (parameter is not TreeViewItem item) return;

            _currentDragTargetShouldBeExpanded = item.IsExpanded;
            item.IsExpanded = false;
        }

        public event EventHandler? CanExecuteChanged;
    }

    private class DragEnded : ICommand
    {
        public bool CanExecute(object? parameter) => parameter is TreeViewItem;

        public void Execute(object? parameter)
        {
            if (parameter is not TreeViewItem item) return;
            item.IsExpanded = _currentDragTargetShouldBeExpanded;
        }

        public event EventHandler? CanExecuteChanged;
    }
}