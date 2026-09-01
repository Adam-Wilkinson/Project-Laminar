using System.ComponentModel;
using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Laminar.Domain.Notifications;
using Laminar.Domain.Observables.Collections;

namespace Laminar.Avalonia.Controls;

[PseudoClasses(IsError, IsWarning, IsInfo)]
public class NotificationDisplay : TemplatedControl
{
    private const string IsError = ":error";
    private const string IsWarning = ":warning";
    private const string IsInfo = ":info";
    
    private readonly BoundObservableCollection<NotificationDisplayItem> _notifications = [];
    
    public static readonly StyledProperty<NotificationManager?> ManagerProperty = AvaloniaProperty.Register<NotificationDisplay, NotificationManager?>(nameof(Manager));

    static NotificationDisplay()
    {
        ManagerProperty.Changed.AddClassHandler<NotificationDisplay>((display, args) => display.OnManagerChanged(args));
    }

    public NotificationManager? Manager
    {
        get => GetValue(ManagerProperty);
        set => SetValue(ManagerProperty, value);
    }

    public IReadOnlyObservableCollection<NotificationDisplayItem> Notifications => _notifications;

    public void AutoResolveAll()
    {
        
    }
    
    private void OnManagerChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var (oldVal, newVal) = args.GetOldAndNewValue<NotificationManager?>();
        
        _notifications.BindTo(newVal?
            .AllNotifications
            .ObservableMap(x => new NotificationDisplayItem(x.Template.Severity, x.Template.Message, [], 0)));
        
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

public record NotificationDisplayItem(
    NotificationSeverity Severity,
    string Message,
    List<NotificationResolution> Resolutions,
    int AutoResolveIndex);

public record NotificationResolution(string Message, Action Resolution)
{
    public void Resolve() => Resolution();
}