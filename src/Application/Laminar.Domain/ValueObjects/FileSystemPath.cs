using System.Runtime.InteropServices;

namespace Laminar.Domain.ValueObjects;

public readonly struct FileSystemPath(string path) : IEquatable<FileSystemPath>
{
    private readonly string _path = Path.GetFullPath(path) ?? throw new ArgumentNullException(nameof(path));

    public FileSystemPath ChildPath(string childName) => new(Path.Join(_path, childName));

    public FileSystemPath? Parent => Path.GetDirectoryName(_path) is { } parent ? new(parent) : null;

    public string Extension => Path.GetExtension(_path);
    public string Name => Path.GetFileNameWithoutExtension(_path);
    public string NameAndExtension => Path.GetFileName(_path);
    
    public override string ToString() => _path;

    public bool Equals(FileSystemPath other) => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? string.Equals(_path, other._path, StringComparison.OrdinalIgnoreCase)
        : string.Equals(_path, other._path, StringComparison.Ordinal);

    public override bool Equals(object? obj)
    {
        return obj is FileSystemPath other && Equals(other);
    }

    public override int GetHashCode() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? StringComparer.OrdinalIgnoreCase.GetHashCode(_path)
        : StringComparer.Ordinal.GetHashCode(_path);

    public static bool operator ==(FileSystemPath left, FileSystemPath right) => left.Equals(right);
    public static bool operator !=(FileSystemPath left, FileSystemPath right) => !left.Equals(right);
}