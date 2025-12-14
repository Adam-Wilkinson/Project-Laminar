namespace Laminar.Contracts;

public interface IFile : IDisposable
{   
    public string Path { get; }

    public byte[] Contents { get; set; }
    
    public event EventHandler<EventArgs>? ContentsChanged;
}