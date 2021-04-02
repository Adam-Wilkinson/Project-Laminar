using OpenFlow_PluginFramework.Primitives.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Primitives.LaminarValue.TypeDefinition
{
    public class RigidTypeDefinitionProvider : TypeDefinitionProvider, IRigidTypeDefinitionManager
    {
        public void RegisterTypeDefinition(object defaultValue, string editorName, string displayName)
        {
            DefaultDefinition = new TypeDefinition(defaultValue) { EditorName = editorName, DisplayName = displayName, };
        }

        public override bool TryGetDefinitionFor(object value, out ITypeDefinition typeDefinition)
        {
            if (DefaultDefinition.CanAcceptValue(value))
            {
                typeDefinition = DefaultDefinition;
                return true;
            }

            typeDefinition = default;
            return false;
        }
    }
}
