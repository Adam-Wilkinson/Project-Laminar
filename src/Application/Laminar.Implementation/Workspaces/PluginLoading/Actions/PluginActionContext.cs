using Laminar.Contracts.Workspaces;
using Laminar.Contracts.Workspaces.PluginLoading;

namespace Laminar.Implementation.Workspaces.PluginLoading.Actions;

public record PluginActionContext(IWorkspace Workspace, IPluginLibrary PluginLibrary, IDictionary<string, IPluginInstallation> UserInstalledPluginsById);