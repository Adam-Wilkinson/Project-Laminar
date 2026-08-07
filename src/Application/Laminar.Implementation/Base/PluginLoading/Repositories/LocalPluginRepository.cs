using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Base.PluginLoading.Repositories;

public class LocalPluginRepository(
    string id, 
    FileSystemPath path,
    IFileSystem fileSystem) : IPluginRepository
{
    public string Id { get; } = id;
    
    public void Refresh()
    {
        foreach (var file in fileSystem.EnumerateChildren(path, "*.plpkg"))
        {
            
        }
    }

    public Dictionary<string, IPluginInfo> Plugins { get; } = [];
    
    public Task<Stream> StreamPlugin(string id, SemanticVersion version, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}