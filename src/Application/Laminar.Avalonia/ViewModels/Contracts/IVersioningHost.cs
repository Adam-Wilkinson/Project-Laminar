using System.ComponentModel;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;

namespace Laminar.Avalonia.ViewModels.Contracts;

public interface IVersioningHost
{
    public IObservableValue<SemanticVersion?> TargetVersion { get; }

    public IReadOnlyObservableValue<string> TargetVersionName { get; }

    public void OnInstalledVersionsChanged();
}