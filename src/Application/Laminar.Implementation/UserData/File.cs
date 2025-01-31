using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Laminar.Contracts;
using Laminar.Contracts.UserData;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData;

public class File : IFile
{
    private const int ErrorSharingViolation = 32;
    private const int ErrorLockViolation = 33;
    
    private readonly IFileSystem _fileSystem;
    private readonly CancellationTokenSource _cancelReadTokenSource = new();
    private readonly CancellationTokenSource _cancelWriteTokenSource = new();
    private readonly IFileWatcher _fileWatcher;
    private readonly ILogger<File> _logger;
    
    private byte[] _contents = [];
    private bool _readNextFileChange = true;
    
    public File(IFileSystem fileSystem, string filePath, ILogger<File> logger)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        Path = filePath;

        if (!_fileSystem.Exists(Path))
        {
            _fileSystem.CreateFile(Path);
        }
        else
        {
            InitiateReadAttempt().Wait();   
        }
        
        _fileWatcher = fileSystem.CreateFileWatcher(_fileSystem.GetParent(Path)!.FullName, _fileSystem.GetFileName(Path));
        _fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
        _fileWatcher.EnableRaisingEvents = true;
        _fileWatcher.Changed += FileChanged;
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
            Task.Run(InitiateWrite);
        }
    }

    public string Path { get; }

    public event EventHandler<EventArgs>? ContentsChanged;
    
    private void FileChanged(object? sender, FileSystemEventArgs e)
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
        
        Task.Run(InitiateReadAttempt);
    }
    
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
            catch (IOException e)
            {
                await WaitIfFileBusyException(e, cancellationToken);
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
            catch (IOException e)
            {
                await WaitIfFileBusyException(e, cancellationToken);
            }
        }
    }
    
    public void Dispose()
    {
        _cancelReadTokenSource.Dispose();
        _cancelWriteTokenSource.Dispose();
        _fileWatcher.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task WaitIfFileBusyException(IOException e, CancellationToken cancellationToken)
    {
        var errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);
        if (errorCode is ErrorSharingViolation or ErrorLockViolation)
        {
            _logger.LogDebug("File {file} is busy. Waiting 200 milliseconds before trying to access again", Path);
            await Task.Delay(200, cancellationToken);   
        }
        else
        {
            throw new Exception("Unexpected error when accessing file", innerException: e);
        }
    }
}