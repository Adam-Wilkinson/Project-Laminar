using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Reactive;
using Avalonia.VisualTree;
using Laminar.PluginFramework.UserInterface;

namespace BasicFunctionality.Avalonia.UserControls;

public partial class SliderEditor : UserControl
{
    private Point? _pointerDownPoint;
    private double _sliderPositionBeforePointerDown;
    
    static SliderEditor()
    {
        PointerPressedEvent.AddClassHandler<Slider>((slider, args) =>
        {
            if (slider.FindAncestorOfType<SliderEditor>() is { } sliderEditor)
            {
                sliderEditor._pointerDownPoint = args.GetPosition(sliderEditor);
            }
        }, handledEventsToo: true);

        PointerReleasedEvent.AddClassHandler<Slider>((slider, args) =>
        {
            if (slider.FindAncestorOfType<SliderEditor>() is { } sliderEditor)
            {
                if (sliderEditor._pointerDownPoint == args.GetPosition(sliderEditor))
                {
                    slider.Value = sliderEditor._sliderPositionBeforePointerDown;
                    sliderEditor.NumberEntry.Value = (decimal)sliderEditor.MainSlider.Value;
                    sliderEditor.SetIsEnteringValue(true);
                }

                sliderEditor._sliderPositionBeforePointerDown = slider.Value;
            }
        }, handledEventsToo: true);
    }
    
    public SliderEditor()
    {
        InitializeComponent();

        MainSlider.PointerWheelChanged += (_, e) =>
        {
            MainSlider.Value += e.Delta.Y - e.Delta.X;
        };

        NumberEntry.Classes.CollectionChanged += (_, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems!.Contains(":focus-within"))
            {
                if (NumberEntry.Value.HasValue)
                {
                    MainSlider.Value = (double)NumberEntry.Value;
                }
                SetIsEnteringValue(false);
            }
        };
        
        NumberEntry.KeyUp += (_, args) =>
        {
            Debug.WriteLine(args.Key);
            if (args.Key == Key.Enter)
            {
                if (NumberEntry.Value.HasValue)
                {
                    MainSlider.Value = (double)NumberEntry.Value;
                }
                SetIsEnteringValue(false);
            }

            if (args.Key == Key.Escape)
            {
                SetIsEnteringValue(false);
            }
        };

        NumberDisplay[!TextBlock.TextProperty] = new Binding { Path = nameof(IInterfaceData.Value),  };
    }

    private void SetIsEnteringValue(bool isEnteringValue)
    {
        MainSlider.IsVisible = !isEnteringValue;
        NumberEntry.IsVisible = isEnteringValue;
        NumberDisplay.IsVisible = !isEnteringValue;

        if (isEnteringValue)
        {
            NumberEntry.Value = (decimal)MainSlider.Value;
            NumberEntry.Focus();
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is IInterfaceData { Definition: Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions.Slider sliderDefinition })
        {
            NumberDisplay[!TextBlock.TextProperty] = new Binding { Path = nameof(IInterfaceData.Value), StringFormat = sliderDefinition.FormatString };
        }
    }
}