using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_PluginFramework.Primitives.TypeDefinition
{
    public interface IRigidTypeDefinitionManager : ITypeDefinitionProvider
    {
        public void RegisterTypeDefinition(object defaultValue, string editorName, string displayName);
    }
}
