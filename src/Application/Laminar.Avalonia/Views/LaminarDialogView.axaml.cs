using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Laminar.Avalonia.ViewModels;
using Laminar.Avalonia.ViewModels.Services;
using System;

namespace Laminar.Avalonia.Views;

public partial class LaminarDialogView : Window, IClosable
{
    public LaminarDialogView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is LaminarDialogViewModel viewModel)
        {
            viewModel.CloseTarget = this;
            ButtonsContainer.ItemsPanelRoot?.Children[viewModel.SelectedOptionIndex].Focus();
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is LaminarDialogViewModel viewModel)
        {
            var toFocus = ButtonsContainer.ItemsPanelRoot?.Children[viewModel.SelectedOptionIndex];
            if (toFocus is ContentPresenter contentPresenter)
            {
                toFocus = contentPresenter.Child;
            }

            toFocus?.Focus();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key is Key.Escape)
        {
            if (DataContext is LaminarDialogViewModel viewModel)
            {
                viewModel.SelectedOptionIndex = viewModel.CancelledOptionIndex;
            }

            Close();
        }

        base.OnKeyDown(e);
    }
}