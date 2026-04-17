using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.IO;

public partial class FileContents : IFileContents
{
    private const int ErrorSharingViolation = 32;
    private const int ErrorLockViolation = 33;
    
    private static readonly TimeSpan FileBusyWaitDuration = new(0, 0, 2);
    
    private readonly IFileSystem _fileSystem;
    private readonly CancellationTokenSource _cancelReadTokenSource = new();
    private readonly CancellationTokenSource _cancelWriteTokenSource = new();
    private readonly IFileWatcher _fileWatcher;
    private readonly ILogger<FileContents> _logger;
    
    private byte[] _contents = [];
    private bool _readNextFileChange = true;
    private Task? _writeTask;
    private Task? _readTask;

    public FileContents(IFileSystem fileSystem, FileSystemPath path, ILogger<FileContents> logger)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        Path = path;

        if (!_fileSystem.Exists(Path))
        {
            _fileSystem.CreateFile(Path).Close();
        }
        else
        {
            InitiateReadAttempt().Wait();   
        }

        if (Path.Parent is not { } parent)
        {
            throw new Exception("File must have a parent path");
        }
        
        _fileWatcher = fileSystem.CreateFileWatcher(parent, Path.Name);
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

            if (_writeTask is null || _writeTask.IsCompleted)
            {
                _writeTask = Task.Run(InitiateWrite);
            }
        }
    }

    public FileSystemPath Path { get; }

    public event EventHandler<EventArgs>? ContentsChanged;
    
    public void CheckAccess()
    {
        if (_writeTask is not null && !_writeTask.IsCompleted)
        {
            _writeTask.Wait();
        }

        if (_readTask is not null && !_readTask.IsCompleted)
        {
            _readTask.Wait();
        }
    }

    private void FileChanged(object? sender, FileSystemEventArgs e)
    {
        if (e.FullPath != Path.ToString() || e.ChangeType != WatcherChangeTypes.Changed)
        {
            return;
        }
        
        if (!_readNextFileChange)
        {
            _readNextFileChange = true;
            return;
        }

        if (_readTask is null || _readTask.IsCompleted)
        {
            _readTask = Task.Run(InitiateReadAttempt);
        }
    }
    
    private async Task InitiateWrite()
    {
        var success = false;
        var cancellationToken = _cancelWriteTokenSource.Token;
        _readNextFileChange = false;
        
        while (!success && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _fileSystem.WriteBytes(Path, _contents, cancellationToken);
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
        int errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);
        if (errorCode is ErrorSharingViolation or ErrorLockViolation)
        {
            LogFileFileIsBusyWaitingMillisecondsBeforeTryingToAccessAgain(Path, FileBusyWaitDuration.TotalMilliseconds);
            await Task.Delay(FileBusyWaitDuration, cancellationToken);   
        }
        else
        {
            throw new Exception("Unexpected error when accessing file", innerException: e);
        }
    }

    [LoggerMessage(LogLevel.Debug, "File '{filePath}' is busy. Waiting {waitDurationMs}ms before trying to access again")]
    partial void LogFileFileIsBusyWaitingMillisecondsBeforeTryingToAccessAgain(FileSystemPath filePath, double waitDurationMs);
}