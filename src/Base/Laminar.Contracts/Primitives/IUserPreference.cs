using Laminar.Contracts.UserInterface;

namespace Laminar.Contracts.Primitives;

public interface IUserPreference
{
    public IDisplay Display { get; }

    public void Reset();
}
