namespace Laminar.Domain.ValueObjects;

public readonly struct SemanticVersion : IEquatable<SemanticVersion>, IComparable<SemanticVersion>
{
    private readonly string _toString;

    public SemanticVersion(int majorVersion, int minorVersion, int patchVersion, string? prereleaseVersion = null)
    {
        MajorVersion = majorVersion;
        MinorVersion = minorVersion;
        PatchVersion = patchVersion;
        PrereleaseVersion = string.IsNullOrEmpty(prereleaseVersion) ? null : prereleaseVersion;

        _toString = PrereleaseVersion is null
            ? $"{MajorVersion}.{MinorVersion}.{PatchVersion}"
            : $"{MajorVersion}.{MinorVersion}.{PatchVersion}-{PrereleaseVersion}";
    }

    public SemanticVersion(string version)
        : this(version.AsSpan())
    {
    }

    public SemanticVersion(ReadOnlySpan<char> version)
    {
        int firstDot = version.IndexOf('.');
        if (firstDot < 0)
            throw new FormatException();

        int secondDotOffset = version[(firstDot + 1)..].IndexOf('.');
        if (secondDotOffset < 0)
            throw new FormatException();

        int secondDot = firstDot + 1 + secondDotOffset;

        ReadOnlySpan<char> patchAndPrerelease = version[(secondDot + 1)..];
        int hyphen = patchAndPrerelease.IndexOf('-');

        MajorVersion = int.Parse(version[..firstDot]);
        MinorVersion = int.Parse(version[(firstDot + 1)..secondDot]);

        if (hyphen < 0)
        {
            PatchVersion = int.Parse(patchAndPrerelease);
            PrereleaseVersion = null;
        }
        else
        {
            PatchVersion = int.Parse(patchAndPrerelease[..hyphen]);
            PrereleaseVersion = patchAndPrerelease[(hyphen + 1)..].ToString();
        }

        _toString = PrereleaseVersion is null
            ? $"{MajorVersion}.{MinorVersion}.{PatchVersion}"
            : $"{MajorVersion}.{MinorVersion}.{PatchVersion}-{PrereleaseVersion}";
    }
    
    public int MajorVersion { get; }
    public int MinorVersion { get; }
    public int PatchVersion { get; }
    public string? PrereleaseVersion { get; }

    public int CompareTo(SemanticVersion other)
    {
        int result = MajorVersion.CompareTo(other.MajorVersion);
        if (result != 0) return result;

        result = MinorVersion.CompareTo(other.MinorVersion);
        if (result != 0) return result;

        result = PatchVersion.CompareTo(other.PatchVersion);
        if (result != 0) return result;

        // A version without prerelease is higher than one with prerelease.
        if (PrereleaseVersion is null && other.PrereleaseVersion is null)
            return 0;

        if (PrereleaseVersion is null)
            return 1;

        if (other.PrereleaseVersion is null)
            return -1;

        return ComparePrerelease(PrereleaseVersion, other.PrereleaseVersion);
    }
    
    private static int ComparePrerelease(
        string left,
        string right)
    {
        string[] leftParts = left.Split('.');
        string[] rightParts = right.Split('.');

        int count = Math.Min(leftParts.Length, rightParts.Length);

        for (int i = 0; i < count; i++)
        {
            string l = leftParts[i];
            string r = rightParts[i];

            bool lNumeric = int.TryParse(l, out int lNumber);
            bool rNumeric = int.TryParse(r, out int rNumber);

            int result;

            if (lNumeric && rNumeric)
            {
                result = lNumber.CompareTo(rNumber);
            }
            else if (lNumeric)
            {
                result = -1;
            }
            else if (rNumeric)
            {
                result = 1;
            }
            else
            {
                result = string.CompareOrdinal(l, r);
            }

            if (result != 0)
                return result;
        }

        return leftParts.Length.CompareTo(rightParts.Length);
    }

    public bool Equals(SemanticVersion other) =>
        MajorVersion == other.MajorVersion &&
        MinorVersion == other.MinorVersion &&
        PatchVersion == other.PatchVersion &&
        string.Equals(PrereleaseVersion, other.PrereleaseVersion, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is SemanticVersion other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(MajorVersion, MinorVersion, PatchVersion, PrereleaseVersion);

    public override string ToString() => _toString;

    public static bool operator ==(SemanticVersion left, SemanticVersion right) => left.Equals(right);

    public static bool operator !=(SemanticVersion left, SemanticVersion right) => !left.Equals(right);

    public static bool operator <(SemanticVersion left, SemanticVersion right) => left.CompareTo(right) < 0;

    public static bool operator >(SemanticVersion left, SemanticVersion right) => left.CompareTo(right) > 0;
}

public sealed class SemanticVersionComparer : IComparer<SemanticVersion>
{
    public static readonly SemanticVersionComparer Instance = new();

    private SemanticVersionComparer()
    {
    }

    public int Compare(SemanticVersion x, SemanticVersion y) =>
        x.CompareTo(y);
}