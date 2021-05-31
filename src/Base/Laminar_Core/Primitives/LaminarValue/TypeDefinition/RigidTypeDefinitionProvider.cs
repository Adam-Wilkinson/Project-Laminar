using Laminar_PluginFramework.Primitives.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Primitives.LaminarValue.TypeDefinition
{
    public class RigidTypeDefinitionProvider : TypeDefinitionProvider, IRigidTypeDefinitionManager
    {
        private readonly Instance _instance;

        public RigidTypeDefinitionProvider(Instance instance)
        {
            _instance = instance;
        }

        public void SetType(Type type)
        {
            DefaultDefinition = new TypeDefinition(type, _instance.GetTypeInfo(type).DefaultValue);
        }

        public void SetTypeDefinition(object defaultValue, string editorName, string displayName)
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
