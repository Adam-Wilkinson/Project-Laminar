using System.Runtime.InteropServices;

namespace Laminar.Domain.ValueObjects;

/// <summary>
/// Utility struct for managing paths. Does not reflect the file system, just simplifies the managing of absolute path strings
/// </summary>
/// <param name="absolutePath">The absolute path that is being manipulated</param>
public readonly struct FileSystemPath(string absolutePath) : IEquatable<FileSystemPath>
{
    public static readonly StringComparison RuntimeStringComparison = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;
    
    public static readonly StringComparer RuntimeStringComparer = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;
    
    private readonly string _path = Path.GetFullPath(absolutePath) ?? throw new ArgumentNullException(nameof(absolutePath));
    
    public FileSystemPath ChildPath(string childName) => new(Path.Join(_path, childName));

    public FileSystemPath? Parent => Path.GetDirectoryName(_path) is { } parent ? new(parent) : null;

    public string NameAndExtension => Path.GetFileName(_path);
    
    public string[] SplitAfter(FileSystemPath relativeTo) 
        => Path.GetRelativePath(relativeTo._path, _path).Split('/', '\\');
    
    public override string ToString() => _path;

    public bool Equals(FileSystemPath other) => string.Equals(_path, other._path, RuntimeStringComparison);

    public override bool Equals(object? obj)
    {
        return obj is FileSystemPath other && Equals(other);
    }

    public override int GetHashCode() => RuntimeStringComparer.GetHashCode(_path);

    public static bool operator ==(FileSystemPath left, FileSystemPath right) => left.Equals(right);
    public static bool operator !=(FileSystemPath left, FileSystemPath right) => !left.Equals(right);
    
    public static implicit operator FileSystemPath(string path) => new(path);
    
    public static implicit operator string(FileSystemPath path) => path.ToString();
}