using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Laminar.Contracts.UserData;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData;

public class FileSystem(ILogger<File> fileLogger) : IFileSystem
{
    public bool Exists(FileSystemPath path) 
        => Directory.Exists(path.ToString()) || System.IO.File.Exists(path.ToString());
    
    public bool IsDirectory(FileSystemPath path) 
        => Exists(path) 
            ? System.IO.File.GetAttributes(path.ToString()).HasFlag(FileAttributes.Directory)
            : string.IsNullOrWhiteSpace(path.Extension);

    public void Delete(FileSystemPath path) 
    {
        if (IsDirectory(path))
        {
            Directory.Delete(path.ToString());
        }
        else
        {
            System.IO.File.Delete(path.ToString());
        }
    }

    public IEnumerable<FileSystemPath> EnumerateChildren(FileSystemPath path) 
        => Directory.GetFileSystemEntries(path.ToString()).Select(x => new FileSystemPath(x));
    
    public void Move(FileSystemPath sourcePath, FileSystemPath destPath) 
    {
        fileLogger.LogTrace("Moving an item from '{sourcePath}' to '{destPath}'", sourcePath, destPath);
        if (IsDirectory(sourcePath))
        {
            Directory.Move(sourcePath.ToString(), destPath.ToString());
        }
        else
        {
            System.IO.File.Move(sourcePath.ToString(), destPath.ToString());            
        }
    }

    public bool OpenInSystemFileBrowser(FileSystemPath path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo("explorer.exe", path.ToString()));
            return true;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("mimeopen", path.ToString());
            return true;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", $"-R \"{path}\"");
            return true;
        }
        
        return false;
    }
    
    public IFileWatcher CreateFileWatcher(FileSystemPath path, string filter = "") 
        => string.IsNullOrEmpty(filter) ? new FileWatcher(path.ToString()) : new FileWatcher(path.ToString(), filter);

    public IFileStream CreateFile(FileSystemPath path)
        => FileStream.Create(path);
    
    public void WriteBytesAsync(FileSystemPath path, byte[] bytes, CancellationToken cancellationToken = default) 
        => System.IO.File.WriteAllBytesAsync(path.ToString(), bytes, cancellationToken);

    public byte[] ReadBytes(FileSystemPath path) 
        => System.IO.File.ReadAllBytes(path.ToString());

    public long GetFileSize(FileSystemPath path)
        => new FileInfo(path.ToString()).Length;

    public void CreateDirectory(FileSystemPath path) 
        => Directory.CreateDirectory(path.ToString());
    
    public IFile GetFile(FileSystemPath path) 
        => new File(this, path, fileLogger);
}