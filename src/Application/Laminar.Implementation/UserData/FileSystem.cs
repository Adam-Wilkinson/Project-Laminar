using System.IO;
using System.Threading;
using Laminar.Contracts;
using Laminar.Contracts.UserData;

namespace Laminar.Implementation.UserData;

public class FileSystem : IFileSystem
{
    public bool Exists(string path) => Directory.Exists(path);
    
    public DirectoryInfo? GetParent(string path) => Directory.GetParent(path);
    public IFileWatcher CreateFileWatcher(string path, string filter = "") => string.IsNullOrEmpty(filter) ? new FileWatcher(path) : new FileWatcher(path, filter); 

    public FileStream CreateFile(string path) => System.IO.File.Create(path);
    
    public StreamWriter SetFileText(string path) => System.IO.File.CreateText(path);
    public void WriteBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default) => System.IO.File.WriteAllBytesAsync(path, bytes, cancellationToken);

    public byte[] ReadBytes(string path) => System.IO.File.ReadAllBytes(path);

    public string ReadTextFile(string path) => System.IO.File.ReadAllText(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    
    public string GetFileName(string path) => Path.GetFileName(path);
    
    public IFile GetFile(string path) => new File(this, path);
}