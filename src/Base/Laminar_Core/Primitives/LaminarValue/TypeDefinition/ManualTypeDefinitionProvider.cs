using Laminar_PluginFramework.Primitives.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Primitives.LaminarValue.TypeDefinition
{
    public class ManualTypeDefinitionProvider : IManualTypeDefinitionProvider
    {
        private readonly List<ITypeDefinition> _typeDefinitions = new();

        public ITypeDefinition DefaultDefinition { get; private set; }

        public void RegisterTypeDefinition(ITypeDefinition typeDefinition)
        {
            _typeDefinitions.Add(typeDefinition);

            if (_typeDefinitions.Count == 1)
            {
                DefaultDefinition = _typeDefinitions[0];
            }
        }

        public bool TryGetDefinitionFor(object value, out ITypeDefinition typeDefinition)
        {
            foreach (ITypeDefinition myTypeDefinition in _typeDefinitions)
            {
                if (myTypeDefinition.CanAcceptValue(value))
                {
                    typeDefinition = myTypeDefinition;
                    return true;
                }
            }

            typeDefinition = default;
            return false;
        }
    }
}
