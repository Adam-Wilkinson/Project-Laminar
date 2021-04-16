using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Primitives.LaminarValue
{
    public interface ILaminarValueStore : IEnumerable<KeyValuePair<object, ILaminarValue>>
    {
        object this[object key] { get; set; }

        int Count { get; }

        event EventHandler<object> ChangedAtKey;
        event EventHandler AnyValueChanged;

        void AddValue(object key, object value, bool isUserEditable);

        void AddValue<T>(object value, bool isUserEditable);

        void SetValueName(string name);

        ILaminarValue GetValue(object key);

        void CopyFrom(ILaminarValueStore copyFrom);
    }
}
