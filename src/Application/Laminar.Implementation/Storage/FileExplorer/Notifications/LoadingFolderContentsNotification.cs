using Laminar.Domain.Notifications;

namespace Laminar.Implementation.Storage.FileExplorer.Notifications;

public class LoadingFolderContentsNotification : LifetimeControlledNotification
{
    public override NotificationSeverity Severity => NotificationSeverity.Loading;
    public override NotificationType Type => NotificationType.LoadingFolderContents;
}