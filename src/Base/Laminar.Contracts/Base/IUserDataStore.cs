using System.Collections.Generic;

namespace Laminar.Contracts.Base;

public interface IUserDataStore
{
    void Save(string fileName, object toSave);

    T Load<T>(string fileName);

    bool TryLoad<T>(string fileName, out T loaded);

    IEnumerable<T> LoadAllFromFolder<T>(string folder, string fileType);
}
