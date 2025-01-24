namespace Laminar.Contracts.UserData;

public interface IFileSystem
{
    public bool Exists(string path);

    public DirectoryInfo? GetParent(string path);
    
    public IFileWatcher CreateFileWatcher(string path, string filter = "");
    
    public FileStream CreateFile(string path);

    public StreamWriter SetFileText(string path);
    
    public void WriteBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default);

    public byte[] ReadBytes(string path);
    
    public string ReadTextFile(string path);
    
    public void CreateDirectory(string path);

    public string GetFileName(string path);
    
    public IFile GetFile(string path);
}