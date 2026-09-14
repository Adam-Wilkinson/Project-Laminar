namespace Laminar.Domain.Notifications;

public enum NotificationType
{
    NodeSourcePluginMissing,
    PluginDoesNotContainNode,
    RuntimeLoadingPlugin,
    RuntimePluginNotFound,
    LoadingFolderContents,
}