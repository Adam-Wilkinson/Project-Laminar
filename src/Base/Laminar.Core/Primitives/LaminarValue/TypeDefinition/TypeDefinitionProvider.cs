using Laminar_PluginFramework.Primitives.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Primitives.LaminarValue.TypeDefinition
{
    public class TypeDefinitionProvider : ITypeDefinitionProvider
    {
        public ITypeDefinition DefaultDefinition { get; protected set; }

        public virtual bool TryGetDefinitionFor(object value, out ITypeDefinition typeDefinition)
        {
            if (value is null)
            {
                typeDefinition = default;
                return false;
            }

            typeDefinition = new TypeDefinition(value);
            DefaultDefinition = typeDefinition;
            return true;
        }

        protected class TypeDefinition : ITypeDefinition
        {
            public TypeDefinition(Type type, object defaultValue)
            {
                ValueType = type;
                DefaultValue = defaultValue;
            }

            public TypeDefinition(object defaultValue)
            {
                ValueType = defaultValue.GetType();
                DefaultValue = defaultValue;
            }

            public object this[string key] => null;

            public Type ValueType { get; }

            public object DefaultValue { get; }

            public string EditorName { get; init; }

            public string DisplayName { get; init; }

            public bool CanAcceptValue(object value) => ValueType.IsAssignableFrom(value.GetType());

            public bool TryConstrainValue(object inputValue, out object outputValue)
            {
                outputValue = inputValue;
                return true;
            }
        }
    }
}
