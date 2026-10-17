using CommunityToolkit.Mvvm.Input;
using Laminar.Domain.ValueObjects;

namespace Laminar.Avalonia.ViewModels.Contracts;

public interface IChangeVersion
{
    public RelativeVersionState SetVersionType { get; }

    public string TargetVersionName { get; }
    
    public SemanticVersion? TargetVersion { get; }
    
    public IAsyncRelayCommand SetVersionCommand { get; }

    public IRelayCommand UninstallCommand { get; }
}

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
}