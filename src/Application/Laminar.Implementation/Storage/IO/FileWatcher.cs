using Laminar.Contracts.Storage.IO;

namespace Laminar.Implementation.Storage.IO;

internal class FileWatcher : FileSystemWatcher, IFileWatcher
{
    public FileWatcher() : base() { }
    
    public FileWatcher(string path) : base(path) { }
    
    public FileWatcher(string path, string filter) : base(path, filter) { }
}