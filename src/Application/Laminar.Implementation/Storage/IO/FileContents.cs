using System.Runtime.InteropServices;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.Helpers;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.IO;

internal partial class FileContents : IFileContents
{
    private const int ErrorSharingViolation = 32;
    private const int ErrorLockViolation = 33;

    private static readonly TimeSpan FileBusyWaitDuration = TimeSpan.FromSeconds(2);

    private readonly IFileSystem _fileSystem;
    private readonly IFileWatcher _fileWatcher;
    private readonly ILogger<FileContents> _logger;

    private readonly Lock _lock = new();
    private readonly CancellationTokenSource _cts = new();

    private byte[] _contents = [];

    private bool _pendingWrite;
    private bool _pendingRead;
    private bool _running;
    private long _version;

    private TaskCompletionSource _idleTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public FileContents(IFileSystem fileSystem, FileSystemPath path, ILogger<FileContents> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        Path = path;

        if (!_fileSystem.Exists(path))
        {
            _fileSystem.CreateFile(path).Close();
        }
        else
        {
            _contents = _fileSystem.ReadBytes(path);
        }

        _idleTcs.TrySetResult();

        var parent = path.Parent ?? throw new InvalidOperationException("File must have parent");

        _fileWatcher = _fileSystem.CreateFileWatcher(parent, path.NameAndExtension);
        _fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
        _fileWatcher.EnableRaisingEvents = true;
        _fileWatcher.Changed += FileChanged;
    }

    public FileSystemPath Path { get; }

    public event EventHandler? ContentsChanged;

    public byte[] Contents
    {
        get { lock (_lock) return _contents; }
        set
        {
            lock (_lock)
            {
                if (BytesHelper.Equals(_contents, value)) return;
                _contents = value;
                _version++;
                _pendingWrite = true;
                EnsureRunning();
            }

            ContentsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public Task WaitForPendingOperations()
    {
        lock (_lock)
        {
            return _idleTcs.Task;
        }
    }

    private void FileChanged(object? sender, FileSystemEventArgs e)
    {
        if (!e.FullPath.Equals(Path))
            return;

        lock (_lock)
        {
            // Writes dominate.
            // If we already intend to write, ignore external changes.
            if (_pendingWrite)
                return;

            _pendingRead = true;
            EnsureRunning();
        }
    }

    private void EnsureRunning()
    {
        if (_running)
            return;

        _running = true;

        if (_idleTcs.Task.IsCompleted)
        {
            _idleTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        _ = Task.Run(RunAsync);
    }

    private async Task RunAsync()
    {
        try
        {
            while (!_cts.IsCancellationRequested)
            {
                bool doWrite;
                bool doRead;

                byte[] writeContents = [];
                long readVersion = 0;

                lock (_lock)
                {
                    doWrite = _pendingWrite;
                    doRead = !doWrite && _pendingRead;

                    if (!doWrite && !doRead)
                    {
                        _running = false;
                        _idleTcs.TrySetResult();
                        return;
                    }

                    if (doWrite)
                    {
                        _pendingWrite = false;
                        writeContents = _contents;
                    }
                    else
                    {
                        _pendingRead = false;
                        readVersion = _version;
                    }
                }

                try
                {
                    if (doWrite)
                    {
                        await _fileSystem.WriteBytes(Path, writeContents, _cts.Token);
                    }
                    else if (doRead)
                    {
                        byte[] diskContents = await _fileSystem.ReadBytesAsync(Path, _cts.Token);

                        bool changed = false;

                        lock (_lock)
                        {
                            // Ignore stale reads if memory changed while reading
                            if (readVersion != _version)
                                continue;

                            if (!BytesHelper.Equals(_contents, diskContents))
                            {
                                _contents = diskContents;
                                changed = true;
                            }
                        }

                        if (changed)
                        {
                            ContentsChanged?.Invoke(this, EventArgs.Empty);
                        }
                    }
                }
                catch (IOException ex) when (IsFileBusy(ex))
                {
                    LogFileIsBusy(Path, FileBusyWaitDuration.TotalMilliseconds);

                    await Task.Delay(FileBusyWaitDuration, _cts.Token);

                    // Re-queue intent
                    lock (_lock)
                    {
                        if (doWrite)
                        {
                            _pendingWrite = true;
                        }
                        else if (doRead)
                        {
                            _pendingRead = true;
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            lock (_lock)
            {
                _running = false;
                _idleTcs.TrySetCanceled();
            }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();

        _fileWatcher.Dispose();
        _cts.Dispose();

        lock (_lock)
        {
            _idleTcs.TrySetCanceled();
        }

        GC.SuppressFinalize(this);
    }

    private static bool IsFileBusy(IOException e)
    {
        int errorCode = Marshal.GetHRForException(e) & 0xFFFF;
        return errorCode is ErrorSharingViolation or ErrorLockViolation;
    }

    [LoggerMessage(LogLevel.Debug, "File '{filePath}' is busy. Waiting {waitDurationMs}ms before retry")]
    partial void LogFileIsBusy(FileSystemPath filePath, double waitDurationMs);
}