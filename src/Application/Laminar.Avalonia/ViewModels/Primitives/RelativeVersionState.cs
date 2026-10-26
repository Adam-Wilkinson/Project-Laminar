using Laminar.Domain.ValueObjects;

namespace Laminar.Avalonia.ViewModels.Primitives;

public enum RelativeVersionState
{
    Equal,
    Upgrade,
    Downgrade,
    SetVersion,
    Install,
    Uninstall,
}

public static class RelativeVersionExtensions
{
    public static RelativeVersionState RelativeChangeTo(this SemanticVersion? currentVersion, SemanticVersion? targetVersion)
    {
        if (currentVersion is null) return RelativeVersionState.Install;
        if (targetVersion is null) return RelativeVersionState.Uninstall;
        if (currentVersion == targetVersion) return RelativeVersionState.Equal;
        if (currentVersion < targetVersion) return RelativeVersionState.Upgrade;
        if (currentVersion >  targetVersion) return RelativeVersionState.Downgrade;
        return RelativeVersionState.SetVersion;
    }

    public static RelativeVersionState Aggregate(this IEnumerable<RelativeVersionState> currentVersionStates)
    {
        var hasInstallations = false;
        var hasUpgrades = false;
        var hasDowngrades = false;
        
        foreach (var versionState in currentVersionStates)
        {
            hasInstallations |= versionState is not RelativeVersionState.Install;
            hasUpgrades |= versionState is RelativeVersionState.Upgrade or RelativeVersionState.SetVersion;
            hasDowngrades |= versionState is RelativeVersionState.Downgrade or RelativeVersionState.SetVersion;
        }

        if (!hasInstallations)
        {
            return RelativeVersionState.Install;
        }
        
        return (hasUpgrades, hasDowngrades) switch
        {
            (false, false) => RelativeVersionState.Equal,
            (true, false) => RelativeVersionState.Upgrade,
            (false, true) => RelativeVersionState.Downgrade,
            (true, true) => RelativeVersionState.SetVersion
        };
    }
}