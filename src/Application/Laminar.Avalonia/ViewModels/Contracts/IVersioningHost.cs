using System.ComponentModel;
using Laminar.Domain.ValueObjects;

namespace Laminar.Avalonia.ViewModels.Contracts;

public interface IVersioningHost : INotifyPropertyChanged
{
    public SemanticVersion? TargetVersion { get; }

    public string TargetVersionName { get; }

    public void OnInstalledVersionsChanged();
}