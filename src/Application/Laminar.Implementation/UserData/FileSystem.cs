using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Laminar.Contracts;
using Laminar.Contracts.UserData;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData;

public class FileSystem(ILogger<File> fileLogger) : IFileSystem
{
    public bool Exists(string path) => Directory.Exists(path) || System.IO.File.Exists(path);
    
    public bool IsDirectory(string path) => System.IO.File.GetAttributes(path).HasFlag(FileAttributes.Directory);

    public void Delete(string path) 
    {
        if (IsDirectory(path))
        {
            System.IO.Directory.Delete(path);
        }
        else
        {
            System.IO.File.Delete(path);
        }
    }
    
    public IEnumerable<string> EnumerateFileSystemEntries(string path) => Directory.GetFileSystemEntries(path);
    
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

    public bool OpenInSystemFileBrowser(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo("explorer.exe", path));
            return true;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("mimeopen", path);
            return true;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", $"-R \"{path}\"");
            return true;
        }
        
        return false;
    }

    public DirectoryInfo? GetParent(string path) => Directory.GetParent(path);
    
    public IFileWatcher CreateFileWatcher(string path, string filter = "") => string.IsNullOrEmpty(filter) ? new FileWatcher(path) : new FileWatcher(path, filter); 

    public FileStream CreateFile(string path) => System.IO.File.Create(path);
    
    public StreamWriter SetFileText(string path) => System.IO.File.CreateText(path);
    
    public void WriteBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default) => System.IO.File.WriteAllBytesAsync(path, bytes, cancellationToken);

    public byte[] ReadBytes(string path) => System.IO.File.ReadAllBytes(path);
    
    public long GetFileSize(string path) => new FileInfo(path).Length;

    public string ReadTextFile(string path) => System.IO.File.ReadAllText(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    
    public string GetFileName(string path) => Path.GetFileName(path);
    
    public IFile GetFile(string path) => new File(this, path, fileLogger);
}