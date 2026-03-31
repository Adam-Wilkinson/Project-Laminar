namespace Laminar.Contracts.UserData;

/// <summary>
/// Low-level representation of the file system of the computer we are running on, manipulating path strings.
/// Anything that modifies the file system should do so through this interface for testability.
/// </summary>
public interface IFileSystem
{
    public bool Exists(string path);

    public bool IsDirectory(string path);
    
    public void Move(string sourcePath, string destPath);
    
    public DirectoryInfo? GetParent(string path);
    
    public IFileWatcher CreateFileWatcher(string path, string filter = "");
    
    public FileStream CreateFile(string path);

    public StreamWriter SetFileText(string path);
    
    public void WriteBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default);

    public byte[] ReadBytes(string path);
    
    public long GetFileSize(string path);
    
    public string ReadTextFile(string path);
    
    public void CreateDirectory(string path);

    public string GetFileName(string path);
    
    public IFile GetFile(string path);
    
    bool OpenInSystemFileBrowser(string path);
    
    IEnumerable<string> EnumerateFileSystemEntries(string path);
    
    void Delete(string path);
}