using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_PluginFramework.Primitives.TypeDefinition
{
    public interface IRigidTypeDefinitionManager : ITypeDefinitionProvider
    {
        public void SetType(Type type);

        public void SetTypeDefinition(object defaultValue, string editorName, string displayName);
    }
}
