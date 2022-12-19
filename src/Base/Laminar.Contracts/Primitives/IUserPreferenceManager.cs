using Laminar.Domain;

namespace Laminar.Contracts.Primitives;

public interface IUserPreferenceManager
{
    public IUserPreference<T> GetPreference<T>(string key);

    public IReadOnlyItemCatagory<IUserPreference> Preferences { get; }

    public IUserPreferenceManager AddPreference<T>(T defaultValue, string localisationName, string path);
}
