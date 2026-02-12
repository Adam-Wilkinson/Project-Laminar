using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts;

public interface IFile : IDisposable, IValueSink<byte[]>
{
    /// <summary>
    /// The path of the file
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// The contents of the file
    /// </summary>
    public byte[] Contents { get; set; }
    
    /// <summary>
    /// Raised when the contents of the file are changed, either by a file update or the setting of <see cref="Contents"/>
    /// </summary>
    public event EventHandler<EventArgs>? ContentsChanged;

    /// <summary>
    /// Ensures that the file is up to date, waits for all read/write tasks to be complete and synced up.
    /// </summary>
    public void CheckAccess();
    
    byte[] IValueSink<byte[]>.Value { set => Contents = value; }
}