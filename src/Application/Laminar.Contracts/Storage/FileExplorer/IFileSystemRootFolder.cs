using Laminar.Contracts.Base.PluginLoading;

namespace Laminar.Contracts.Storage.FileExplorer;

public interface IFileSystemRootFolder : IFileSystemFolder
{
    public IRuntimeHost RuntimeHost { get; }
}