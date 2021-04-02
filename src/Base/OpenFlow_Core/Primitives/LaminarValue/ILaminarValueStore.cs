using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Primitives.LaminarValue
{
    public interface ILaminarValueStore
    {
        object this[object key] { get; set; }

        int Count { get; }

        event EventHandler<object> ChangedAtKey;
        event EventHandler AnyValueChanged;

        void AddValue(object key, object value, bool isUserEditable);

        void SetValueName(string name);

        ILaminarValue GetValue(object key);

        ILaminarValueStore Clone();
    }
}
