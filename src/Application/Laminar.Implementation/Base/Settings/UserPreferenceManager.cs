using System.Collections.Generic;
using Laminar.Contracts.Base.Settings;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Domain;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Base.Settings;

internal class UserPreferenceManager : IUserPreferenceManager
{
    private readonly IDisplayFactory _valueDisplayFactory;
    private readonly ItemCatagory<IUserPreference> _userPreferences = new("root");
    private readonly Dictionary<string, IUserPreference> _userPreferencesDictionary = new();

    public UserPreferenceManager(IDisplayFactory valueDisplayFactory)
    {
        _valueDisplayFactory = valueDisplayFactory;
    }

    public IUserPreferenceManager AddPreference<T>(T defaultValue, string localisationName, string path)
    {
        if (_userPreferencesDictionary.ContainsKey(path))
        {
            return this;
        }

        IUserPreference newPreference = new UserPreference<T>(_valueDisplayFactory, defaultValue, localisationName);
        _userPreferences.AddItem(newPreference, path);
        _userPreferencesDictionary.Add(path, newPreference);
        return this;
    }

    public IReadOnlyItemCatagory<IUserPreference> Preferences => _userPreferences;

    public IUserPreference<T> GetPreference<T>(string key) => _userPreferencesDictionary[key] as IUserPreference<T>;
}
