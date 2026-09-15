namespace Laminar.Domain.Notifications;

public enum NotificationType
{
    NodeSourcePluginMissing,
    PluginDoesNotContainNode,
    ErrorCreatingNode,
    RuntimeLoadingPlugin,
    RuntimePluginNotFound,
    LoadingFolderContents,
}