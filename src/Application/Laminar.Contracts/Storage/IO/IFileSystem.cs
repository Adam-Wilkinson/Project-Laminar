using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.IO;

/// <summary>
/// Low-level representation of the file system of the computer we are running on, manipulating path strings.
/// Anything that modifies the file system should do so through this interface for testability.
/// </summary>
public interface IFileSystem
{
    public bool Exists(FileSystemPath path);

    public bool IsDirectory(FileSystemPath directoryPath);
    
    public void Move(FileSystemPath sourcePath, FileSystemPath destPath);
    
    public IFileWatcher CreateFileWatcher(FileSystemPath path, string filter = "");
    
    public IFileStream CreateFile(FileSystemPath path);
    
    public Task WriteBytes(FileSystemPath path, byte[] bytes, CancellationToken cancellationToken = default);

    public Task<byte[]> ReadBytesAsync(FileSystemPath path, CancellationToken cancellationToken = default);
    
    public byte[] ReadBytes(FileSystemPath path);
    
    public long GetFileSize(FileSystemPath path);
    
    public void CreateDirectory(FileSystemPath path);
    
    public IFileContents GetFile(FileSystemPath path);
    
    public bool OpenInSystemFileBrowser(FileSystemPath path);
    
    public IEnumerable<FileSystemPath> EnumerateChildren(FileSystemPath path);
    
    public void Delete(FileSystemPath path);
    string GetNameWithoutExtension(FileSystemPath path);
    string GetExtension(FileSystemPath path);
}