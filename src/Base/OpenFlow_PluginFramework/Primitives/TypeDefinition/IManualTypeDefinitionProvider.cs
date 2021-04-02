using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_PluginFramework.Primitives.TypeDefinition
{
    public interface IManualTypeDefinitionProvider : ITypeDefinitionProvider
    {
        public void RegisterTypeDefinition(ITypeDefinition typeDefinition);
    }
}
