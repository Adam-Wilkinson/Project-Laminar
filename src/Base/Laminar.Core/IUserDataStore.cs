using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar.Core
{
    public interface IUserDataStore
    {
        void Save(string fileName, object toSave);

        T Load<T>(string fileName);

        bool TryLoad<T>(string fileName, out T loaded);

        IEnumerable<T> LoadAllFromFolder<T>(string folder, string fileType);
    }
}
