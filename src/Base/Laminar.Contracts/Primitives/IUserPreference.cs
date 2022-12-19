using Laminar.Contracts.UserInterface;

namespace Laminar.Contracts.Primitives;

public interface IUserPreference
{
    public IValueDisplay ValueDisplay { get; }

    public void Reset();
}
