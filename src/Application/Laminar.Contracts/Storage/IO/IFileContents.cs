using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.IO;

public interface IFileContents : IDisposable, IValueSink<byte[]>
{
    /// <summary>
    /// The path of the file
    /// </summary>
    public FileSystemPath Path { get; }

    /// <summary>
    /// The contents of the file
    /// </summary>
    public byte[] Contents { get; set; }
    
    /// <summary>
    /// Raised when the contents of the file are changed, either by a file update or the setting of <see cref="Contents"/>
    /// </summary>
    public event EventHandler? ContentsChanged;

    public Task WaitForPendingOperations();
    
    byte[] IValueSink<byte[]>.Value { set => Contents = value; }
}