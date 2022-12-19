using System.Collections.Generic;
using Laminar.Contracts.Primitives;
using Laminar.Contracts.UserInterface;
using Laminar.Domain;

namespace Laminar.Core.UserPreferences;

internal class UserPreferenceManager : IUserPreferenceManager
{
    private readonly IValueDisplayFactory _valueDisplayFactory;
    private ItemCatagory<IUserPreference> _userPreferences = new("root");
    private Dictionary<string, IUserPreference> _userPreferencesDictionary = new();

    public UserPreferenceManager(IValueDisplayFactory valueDisplayFactory)
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
