using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.Localizers;
using Laminar.Domain.Notifications;
using Laminar.Domain.Observables.Collections;

namespace Laminar.Avalonia.Controls;

[PseudoClasses(IsError, IsWarning, IsInfo), TemplatePart("MainButton", typeof(Button))]
public class NotificationDisplay : TemplatedControl
{
    private const string IsError = ":error";
    private const string IsWarning = ":warning";
    private const string IsInfo = ":info";
    
    private readonly BoundObservableList<NotificationDisplayItem> _notifications = [];
    
    public static readonly StyledProperty<NotificationManager?> ManagerProperty = AvaloniaProperty.Register<NotificationDisplay, NotificationManager?>(nameof(Manager));
    
    static NotificationDisplay()
    {
        ManagerProperty.Changed.AddClassHandler<NotificationDisplay>((display, args) => display.OnManagerChanged(args));
    }

    private Button? _mainButton;

    public NotificationDisplay()
    {
        _notifications.CollectionChanged += NotificationsOnCollectionChanged;
    }

    private void NotificationsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_notifications.Count == 0 && _mainButton?.Flyout?.IsOpen is true)
        {
            _mainButton.Flyout.IsOpen = false;
        }
    }

    public NotificationManager? Manager
    {
        get => GetValue(ManagerProperty);
        set => SetValue(ManagerProperty, value);
    }

    public IReadOnlyObservableList<NotificationDisplayItem> Notifications => _notifications;

    public void AutoResolveAll()
    {
        if (_mainButton?.Flyout?.IsOpen is true) return;
        
        while (Notifications.Count > 0)
        {
            Notifications[0].Resolutions[Notifications[0].AutoResolveIndex].Resolution();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _mainButton = e.NameScope.Find<Button>("MainButton");
        base.OnApplyTemplate(e);
    }

    private void OnManagerChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var (oldVal, newVal) = args.GetOldAndNewValue<NotificationManager?>();
        
        _notifications.BindTo(newVal?
            .AllNotifications
            .ObservableMap(NotificationLocalizer.Localize));
        
        oldVal?.PropertyChanged -= OnManagerPropertyChanged;
        newVal?.PropertyChanged += OnManagerPropertyChanged;
        UpdatePseudoClasses();
    }

    private void OnManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not nameof(NotificationManager.MaximumSeverity)) return;

        UpdatePseudoClasses();
    }

    private void UpdatePseudoClasses()
    {
        switch (Manager?.MaximumSeverity)
        {
            case NotificationSeverity.Error:
                PseudoClasses.Add(IsError);
                PseudoClasses.Remove(IsWarning);
                PseudoClasses.Remove(IsInfo);
                break;
            case NotificationSeverity.Warning:
                PseudoClasses.Remove(IsError);
                PseudoClasses.Add(IsWarning);
                PseudoClasses.Remove(IsInfo);
                break;
            case NotificationSeverity.Information:
                PseudoClasses.Remove(IsError);
                PseudoClasses.Remove(IsWarning);
                PseudoClasses.Add(IsInfo);
                break;
            case null:
            case NotificationSeverity.None:
            case NotificationSeverity.Loading:
            default:
                PseudoClasses.Remove(IsError);
                PseudoClasses.Remove(IsWarning);
                PseudoClasses.Remove(IsInfo);
                break;
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

public record NotificationResolution(string Message, Action Resolution)
{
    public void Resolve() => Resolution();

    public bool IsHovered { get; set; } = false;
}