using Laminar.Contracts.Storage.IO;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.IO;

public class FileStream : IFileStream
{
    private readonly global::System.IO.FileStream _internal;

    private FileStream(System.IO.FileStream internalStream)
    {
        _internal = internalStream;
    }

    public static FileStream Create(FileSystemPath path) => new(System.IO.File.Create(path.ToString()));

    public void Close() => _internal.Close();
}