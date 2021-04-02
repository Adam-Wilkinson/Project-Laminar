using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core
{
    public interface ITypeMapper<in TIn, out TOut>
    {
        public TOut MapType(TIn toMap);
    }
}
