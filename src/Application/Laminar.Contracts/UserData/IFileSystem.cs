namespace Laminar.Contracts.UserData;

public interface IFileSystem
{
    public bool Exists(string path);

    public DirectoryInfo? GetParent(string path);
    
    public FileStream CreateFile(string path);

    public StreamWriter CreateTextFile(string path);
    
    public string ReadTextFile(string path);
    
    public void CreateDirectory(string path);
}