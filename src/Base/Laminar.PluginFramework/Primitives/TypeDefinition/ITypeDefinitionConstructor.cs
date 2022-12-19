using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_PluginFramework.Primitives.TypeDefinition
{
    public interface ITypeDefinitionConstructor<T>
    {
        public string EditorName { get; set; }

        public string DisplayName { get; set; }

        public T DefaultValue { get; set; }

        public void AddConstraint(IValueConstraint<T> constraint);

        public void AddProperty(string propertyName, object propertyValue);

        public ITypeDefinition Construct();
    }
}
