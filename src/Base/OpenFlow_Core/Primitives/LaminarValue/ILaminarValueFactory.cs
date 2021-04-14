using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Primitives.LaminarValue
{
    public interface ILaminarValueFactory
    {
        ILaminarValue Get(object value, bool isUserEditable);

        ILaminarValue Get<T>(bool isUserEditable);
    }
}
