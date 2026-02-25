using System;
using System.IO;
using System.Threading;
using Laminar.Contracts;
using Laminar.Contracts.UserData;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData;

public class FileSystem(ILogger<File> fileLogger) : IFileSystem
{
    public bool Exists(string path) => Directory.Exists(path) || System.IO.File.Exists(path);
    
    public bool IsDirectory(string path) => System.IO.File.GetAttributes(path).HasFlag(FileAttributes.Directory);

    public void Move(string sourcePath, string destPath) 
    {
        fileLogger.LogTrace("Moving an item from '{sourcePath}' to '{destPath}'", sourcePath, destPath);
        if (IsDirectory(sourcePath))
        {
            Directory.Move(sourcePath, destPath);
        }
        else
        {
            System.IO.File.Move(sourcePath, destPath);            
        }
    }

    public void Move(FileSystemInfo fileSystemInfo, string destPath)
    {
        switch (fileSystemInfo)
        {
            case FileInfo fileInfo:
                fileInfo.MoveTo(destPath);
                break; 
            case DirectoryInfo directoryInfo:
                directoryInfo.MoveTo(destPath);
                break;
        };
    }

    public DirectoryInfo? GetParent(string path) => Directory.GetParent(path);
    
    public IFileWatcher CreateFileWatcher(string path, string filter = "") => string.IsNullOrEmpty(filter) ? new FileWatcher(path) : new FileWatcher(path, filter); 

    public FileStream CreateFile(string path) => System.IO.File.Create(path);
    
    public StreamWriter SetFileText(string path) => System.IO.File.CreateText(path);
    
    public void WriteBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default) => System.IO.File.WriteAllBytesAsync(path, bytes, cancellationToken);

    public byte[] ReadBytes(string path) => System.IO.File.ReadAllBytes(path);

    public string ReadTextFile(string path) => System.IO.File.ReadAllText(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    
    public string GetFileName(string path) => Path.GetFileName(path);
    
    public IFile GetFile(string path) => new File(this, path, fileLogger);
}