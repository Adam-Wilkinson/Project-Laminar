using Laminar_PluginFramework;
using Laminar_PluginFramework.Primitives;
using Laminar_PluginFramework.Primitives.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Primitives.LaminarValue
{
    public class LaminarValueFactory : ILaminarValueFactory
    {
        private readonly IObjectFactory _factory;

        public LaminarValueFactory(IObjectFactory factory)
        {
            _factory = factory;
        }

        public ILaminarValue Get(object value, bool isUserEditable)
        {
            ILaminarValue output = new LaminarValue(GetProvider(value), _factory.GetImplementation<IObservableValue<bool>>(), _factory);
            output.IsUserEditable.Value = isUserEditable;
            return output;
        }

        public ILaminarValue Get<T>(bool isUserEditable)
            => Get(typeof(T), isUserEditable);

        private ITypeDefinitionProvider GetProvider(object value)
        {
            if (value is ITypeDefinitionProvider typeDefinitionProvider)
            {
                return typeDefinitionProvider;
            }

            if (value.GetType().IsGenericType && value.GetType().GetInterfaces().Where(x => x.IsGenericType).Select(x => x.GetGenericTypeDefinition()).Contains(typeof(ITypeDefinitionConstructor<>)))
            {
                value = ((dynamic)value).Construct();
            }

            if (value is ITypeDefinition typeDefinition)
            {
                IManualTypeDefinitionProvider manager = _factory.GetImplementation<IManualTypeDefinitionProvider>();
                manager.RegisterTypeDefinition(typeDefinition);
                return manager;
            }

            if (value is Type type)
            {
                IRigidTypeDefinitionManager manager = _factory.GetImplementation<IRigidTypeDefinitionManager>();
                manager.SetType(type);
                return manager;
            }

            IRigidTypeDefinitionManager rigidTypeDefinitionManager = _factory.GetImplementation<IRigidTypeDefinitionManager>();
            rigidTypeDefinitionManager.SetTypeDefinition(value, null, null);
            return rigidTypeDefinitionManager;
        }
    }
}
