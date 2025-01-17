using Laminar.Domain;

namespace Laminar.Contracts.UserData.Settings;

public interface IUserPreferenceManager
{
    public IUserPreference<T>? GetPreference<T>(string key);

    public IReadOnlyItemCategory<IUserPreference> Preferences { get; }

    public IUserPreferenceManager AddPreference<T>(T defaultValue, string localisationName, string path);
}
