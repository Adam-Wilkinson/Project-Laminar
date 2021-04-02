using OpenFlow_PluginFramework.Primitives;
using OpenFlow_PluginFramework.Primitives.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Primitives.TypeDefinitionManagers
{
    public class TypeDefinitionConstructor<T> : ITypeDefinitionConstructor<T>
    {
        private IValueConstraint<T> _currentConstraint;
        private readonly Dictionary<string, object> _properties = new();

        public TypeDefinitionConstructor(IValueConstraint<T> valueConstraint)
        {
            _currentConstraint = valueConstraint;
        }

        public string EditorName { get; set; }

        public string DisplayName { get; set; }

        public T DefaultValue { get; set; }

        public void AddConstraint(IValueConstraint<T> constraint)
        {
            constraint.AddToEndOfChain(_currentConstraint);

            _currentConstraint = constraint;
        }

        public void AddProperty(string propertyName, object propertyValue)
        {
            _properties.Add(propertyName, propertyValue);
        }

        public ITypeDefinition Construct() => new ConstructableTypeDefinition(_properties, _currentConstraint.TotalFunc)
        {
            DefaultValue = DefaultValue,
            DisplayName = DisplayName,
            EditorName = EditorName,
        };

        private class ConstructableTypeDefinition : ITypeDefinition
        {
            private readonly Dictionary<string, object> _properties;
            private readonly Func<T, T> _constraint;

            public ConstructableTypeDefinition(Dictionary<string, object> properties, Func<T, T> constraint)
            {
                _properties = properties;
                _constraint = constraint;
            }

            public object this[string key] => _properties[key];

            public Type ValueType => typeof(T);

            public object DefaultValue { get; init; }

            public string EditorName { get; init; }

            public string DisplayName { get; init; }

            public bool CanAcceptValue(object value) => value != null && value.GetType() == ValueType;

            public bool TryConstrainValue(object inputValue, out object outputValue)
            {
                if (CanAcceptValue(inputValue))
                {
                    outputValue = _constraint((T)inputValue);
                    return true;
                }

                outputValue = default;
                return false;
            }
        }
    }
}
