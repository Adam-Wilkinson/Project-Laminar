using Laminar.Domain.ValueObjects;

namespace Laminar.Domain.Exceptions;

public class FileCreationTimeoutException(FileSystemPath filePath)
    : Exception($"Timed out waiting for file {filePath} to be created.")
{
    public FileSystemPath FilePath { get; } = filePath;
}