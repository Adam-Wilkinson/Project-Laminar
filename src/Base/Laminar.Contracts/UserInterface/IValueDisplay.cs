using Laminar.PluginFramework.NodeSystem.Contracts;

namespace Laminar.Contracts.UserInterface;

public interface IValueDisplay
{
    public IValueInfo ValueInfo { get; }

    public IUserInterface this[string frontendKey] { get; }

    public void Refresh();

    public bool CopyValueTo(IValueDisplay value);
}
