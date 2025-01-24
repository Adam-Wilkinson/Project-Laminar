using System.IO;
using Laminar.Contracts.UserData;

namespace Laminar.Implementation.UserData;

public class FileWatcher : FileSystemWatcher, IFileWatcher
{
    public FileWatcher() : base() { }
    
    public FileWatcher(string path) : base(path) { }
    
    public FileWatcher(string path, string filter) : base(path, filter) { }
}