using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Laminar.Contracts;
using Laminar.Contracts.UserData;

namespace Laminar.Implementation.UserData;

public class File : IFile
{
    private readonly IFileSystem _fileSystem;
    private readonly CancellationTokenSource _cancelReadTokenSource = new();
    private readonly CancellationTokenSource _cancelWriteTokenSource = new();
    private readonly IFileWatcher _fileWatcher;
    
    private byte[] _contents = [];
    private bool _readNextFileChange = true;
    
    public File(IFileSystem fileSystem, string filePath)
    {
        _fileSystem = fileSystem;
        Path = filePath;
        InitiateReadAttempt().Wait();
        
        _fileWatcher = fileSystem.CreateFileWatcher(_fileSystem.GetParent(Path)!.FullName, _fileSystem.GetFileName(Path));
        _fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
        _fileWatcher.EnableRaisingEvents = true;
        _fileWatcher.Changed += FileChanged;
    }

    public void FileChanged(object? sender, FileSystemEventArgs e)
    {
        if (e.FullPath != Path)
        {
            return;
        }
        
        if (!_readNextFileChange)
        {
            _readNextFileChange = true;
            return;
        }
        
        InitiateReadAttempt();
    }
    
    public byte[] Contents
    {
        get => _contents;
        set
        {
            if (_contents == value)
            {
                return;
            }
            
            _contents = value;
            ContentsChanged?.Invoke(this, EventArgs.Empty);
            InitiateWrite();
        }
    }

    public string Path { get; }

    public event EventHandler<EventArgs>? ContentsChanged;

    private async Task InitiateWrite()
    {
        var success = false;
        var cancellationToken = _cancelWriteTokenSource.Token;
        
        while (!success && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                _readNextFileChange = false;
                _fileSystem.WriteBytesAsync(Path, _contents, cancellationToken);
                success = true;
            }
            catch (IOException)
            {
                await Task.Delay(200, cancellationToken);
            }
        }
    }
    
    private async Task InitiateReadAttempt()
    {
        var readSuccessful = false;
        var cancellationToken = _cancelReadTokenSource.Token;

        while (!readSuccessful && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                _contents = _fileSystem.ReadBytes(Path);
                readSuccessful = true;
                ContentsChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (IOException)
            {
                await Task.Delay(200, cancellationToken);
            }
        }
    }

    public void Dispose()
    {
        _cancelReadTokenSource.Dispose();
        _cancelWriteTokenSource.Dispose();
        _fileWatcher.Dispose();
    }
}