using Laminar.Contracts.Base.UserInterface;

namespace Laminar.Contracts.Base.Settings;

public interface IUserPreference
{
    public IDisplay Display { get; }

    public void Reset();
}
