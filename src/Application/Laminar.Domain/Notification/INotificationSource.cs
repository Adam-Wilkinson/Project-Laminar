using System.ComponentModel;

namespace Laminar.Domain.Notification;

public interface INotificationSource
{
    public event EventHandler OnNotification;

    public static INotificationSource Never { get; } = new NeverNotificationSource();

    private class NeverNotificationSource : INotificationSource
    {
        public event EventHandler? OnNotification { add { } remove { } }
    }
}