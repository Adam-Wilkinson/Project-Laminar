using System.IO;
using Laminar.Contracts.UserData;

namespace Laminar.Implementation.UserData;

public class FileSystem : IFileSystem
{
    public bool Exists(string path) => Directory.Exists(path);
    
    public DirectoryInfo? GetParent(string path) => Directory.GetParent(path);
    
    public FileStream CreateFile(string path) => File.Create(path);
    
    public StreamWriter CreateTextFile(string path) => File.CreateText(path);

    public string ReadTextFile(string path) => File.ReadAllText(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
}