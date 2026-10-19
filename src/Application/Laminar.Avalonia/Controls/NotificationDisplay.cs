using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;
using Laminar.Domain.Notifications;

namespace Laminar.Avalonia.Controls;

[TemplatePart("MainButton", typeof(Button))]
public class NotificationDisplay : TemplatedControl
{
    public static readonly StyledProperty<NotificationSeverity> DisplayedSeverityProperty = AvaloniaProperty.Register<NotificationDisplay, NotificationSeverity>(nameof(DisplayedSeverity));
    
    public static readonly StyledProperty<IReadOnlyList<object>> NotificationsProperty = AvaloniaProperty.Register<NotificationDisplay, IReadOnlyList<object>>(nameof(Notifications), []);

    private Button? _mainButton;

    static NotificationDisplay()
    {
        NotificationsProperty.Changed.AddClassHandler<NotificationDisplay>((n, args) => n.NotificationsChanged(args));
    }

    public NotificationSeverity DisplayedSeverity
    {
        get => GetValue(DisplayedSeverityProperty);
        set => SetValue(DisplayedSeverityProperty, value);
    }

    public IReadOnlyList<object> Notifications
    {
        get => GetValue(NotificationsProperty);
        set => SetValue(NotificationsProperty, value);
    }

    public void AutoResolveAll()
    {
        if (_mainButton?.Flyout?.IsOpen is true) return;

        int skipped = 0;
        while (Notifications.Count > skipped)
        {
            if (Notifications[skipped] is not NotificationDisplayItem item
                || !item.Resolutions[item.AutoResolveIndex].Resolve())
            {
                skipped++;
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _mainButton = e.NameScope.Find<Button>("MainButton");
        base.OnApplyTemplate(e);
    }
    
    private void NotificationsChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var (oldVal, newVal) = args.GetOldAndNewValue<IReadOnlyList<object>>();

        if (oldVal is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= NotificationsOnCollectionChanged;
        }

        if (newVal is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += NotificationsOnCollectionChanged;
        }
        
        CloseFlyoutIfOpenAndEmpty();
    }

    private void NotificationsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CloseFlyoutIfOpenAndEmpty();
    }

    private void CloseFlyoutIfOpenAndEmpty()
    {
        if (Notifications.Count == 0 && _mainButton?.Flyout?.IsOpen is true)
        {
            _mainButton.Flyout.IsOpen = false;
        }
    }
}

public record NotificationDisplayItem
{
    public NotificationDisplayItem(NotificationSeverity Severity,
        string Message,
        List<NotificationResolution> Resolutions,
        Action? Dismiss = null,
        int AutoResolveIndex = -1)
    {
        this.Severity = Severity;
        this.Message = Message;
        this.Resolutions = Resolutions;
        this.AutoResolveIndex = AutoResolveIndex;
        this.Dismiss = Dismiss is not null ? new RelayCommand(Dismiss) : null;
        if (Resolutions.Count > 0 && AutoResolveIndex >= 0)
        {
            Resolutions[AutoResolveIndex].IsHovered = true;
        }
    }

    public NotificationSeverity Severity { get; init; }
    public string Message { get; init; }
    public List<NotificationResolution> Resolutions { get; init; }
    public int AutoResolveIndex { get; init; }
    public ICommand? Dismiss { get; init; }
}

public record NotificationResolution(string Message, Action? Resolution)
{
    public bool Resolve()
    {
        if (Resolution is null) return false;
        Resolution.Invoke();
        return true;
    }

    public bool IsHovered { get; set; } = false;
}