using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Laminar.Avalonia.ViewModels.Services;

namespace Laminar.Avalonia.ViewModels;

public partial class LaminarDialogViewModel : ViewModelBase, IModalDialogViewModel
{
    public LaminarDialogViewModel()
    {
        SelectedOptionIndex = 0;
        SelectedOption = Options[0];
    }

    [ObservableProperty]
    public partial string Title { get; set; } = "Dialog Box";

    [ObservableProperty]
    public partial string Message { get; set; } = "Dialog Box Contents";

    public DialogOption[] Options { get; init; } = [ DialogOption.Ok ];

    [ObservableProperty]
    public partial int SelectedOptionIndex { get; set; }

    [ObservableProperty]
    public partial bool AdditionalCheckboxChecked { get; set; }

    public string? AdditionalCheckboxText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DialogResult))]
    public partial DialogOption SelectedOption { get; set; }

    public bool? DialogResult => SelectedOption.DialogResult;

    public IClosable? CloseTarget { get; set; }

    public int CancelledOptionIndex { get; init; }

    [RelayCommand]
    public void SelectOption(DialogOption option)
    {
        SelectedOption = option;
        CloseTarget?.Close();
    }

    partial void OnSelectedOptionChanging(DialogOption oldValue, DialogOption newValue)
    {
        if (!Options.Contains(newValue))
        {
            SelectedOption = oldValue;
        }
    }

    partial void OnSelectedOptionIndexChanging(int oldValue, int newValue)
    {
        if (newValue < 0 || newValue > Options.Length)
        {
            SelectedOptionIndex = oldValue;
        }
    }

    partial void OnSelectedOptionChanged(DialogOption value)
    {
        int selectedOptionIndex = Options.IndexOf(value);
        if (selectedOptionIndex != SelectedOptionIndex)
        {
            SelectedOptionIndex = selectedOptionIndex;
        }
    }

    partial void OnSelectedOptionIndexChanged(int value)
    {
        if (Options[value] != SelectedOption)
        {
            SelectedOption = Options[value];
        }
    }
}

public record ValueDialogOption<T>(T Value, string UiText, bool? DialogResult = null) : DialogOption(UiText, DialogResult);

public record DialogOption(string UiText, bool? DialogResult = null)
{
    public static ValueDialogOption<T> WithValue<T>(T value, string uiText) => new(value, uiText);
    
    public static readonly DialogOption Ok = new("Ok", true);

    public static readonly DialogOption Cancel = new("Cancel", null);

    public static readonly DialogOption Yes = new("Yes", true);

    public static readonly DialogOption No = new("No", false);

    public override string ToString() => UiText;
}