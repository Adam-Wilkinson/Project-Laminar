using OpenFlow_Core.Nodes.Connectors;
using OpenFlow_Core.Nodes.NodeComponents.Collections;
using OpenFlow_Core.Nodes.NodeComponents.Visuals;
using OpenFlow_Core.Primitives;
using OpenFlow_Core.Primitives.LaminarValue;
using OpenFlow_Core.Primitives.LaminarValue.TypeDefinition;
using OpenFlow_Core.Primitives.UserInterface;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Collections;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;
using OpenFlow_PluginFramework.Primitives.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core
{
    public class ObjectFactory : IObjectFactory
    {
        private readonly Dictionary<Type, Type> interfaceImplementations = new();

        public ObjectFactory()
        {
            RegisterImplementation<IOpacity, Opacity>();
            RegisterImplementation<INodeField, NodeField>();
            RegisterImplementation<INodeLabel, NodeLabel>();
            RegisterImplementation<INodeComponentList, NodeComponentList>();
            RegisterImplementation<INodeComponentAutoCloner, NodeComponentAutoCloner>();
            RegisterImplementation<INodeComponentDictionary, NodeComponentDictionary>();
            RegisterImplementation<INodeComponentCollection, NodeComponentCollection>();
            RegisterImplementation<ITypeDefinitionProvider, TypeDefinitionProvider>();
            RegisterImplementation<IRigidTypeDefinitionManager, RigidTypeDefinitionProvider>();
            RegisterImplementation<IManualTypeDefinitionProvider, ManualTypeDefinitionProvider>();
            RegisterImplementation<ILaminarValueStore, LaminarValueStore>();
            RegisterImplementation<ILaminarValueFactory, LaminarValueFactory>();
            RegisterImplementation<IUserInterfaceRegister, UserInterfaceRegister>();
            RegisterImplementation<IUserInterfaceManager, UserInterfaceManager>();
            RegisterImplementation<ISeparator, Separator>();
            RegisterImplementation<IConnectionManager, ConnectionManager>();
            RegisterImplementationUnsafe(typeof(IObservableValue<>), typeof(ObservableValue<>));
            RegisterImplementationUnsafe(typeof(IValueConstraint<>), typeof(ValueConstraint<>));
            RegisterImplementationUnsafe(typeof(ITypeDefinitionConstructor<>), typeof(TypeDefinitionConstructor<>));
        }

        public T GetImplementation<T>()
        {
            return (T)GetLooseTypedImplementation(typeof(T));
        }

        public IObjectFactory RegisterImplementation<TInterface, TImplementation>()
            where TImplementation : class, TInterface
        {
            interfaceImplementations.Add(typeof(TInterface), typeof(TImplementation));
            return this;
        }

        public void RegisterImplementationUnsafe(Type interfaceType, Type implementationType)
        {
            Type[] interfaces = implementationType.GetInterfaces();
            if (!(implementationType.GetInterfaces().Any(x => x.IsGenericType && (x.GetGenericTypeDefinition() == interfaceType)) && implementationType.IsClass))
            {
                throw new ArgumentException($"Type {implementationType} is not a class that inherits from {interfaceType}");
            }

            interfaceImplementations.Add(interfaceType, implementationType);
        }

        private object GetLooseTypedImplementation(Type typeToGet)
        {
            Type targetType;
            if (typeToGet.IsGenericType)
            {
                targetType = interfaceImplementations[typeToGet.GetGenericTypeDefinition()];
                targetType = targetType.MakeGenericType(typeToGet.GetGenericArguments());
            }
            else
            {
                targetType = interfaceImplementations[typeToGet];
            }

            if (targetType.GetConstructor(Type.EmptyTypes) != null)
            {
                return Activator.CreateInstance(targetType);
            }

            ConstructorInfo info = targetType.GetConstructors()[0];
            ParameterInfo[] parameters = info.GetParameters();
            object[] parameterObjects = new object[parameters.Length];
            int i = 0;
            foreach (ParameterInfo parameter in info.GetParameters())
            {
                parameterObjects[i] = GetLooseTypedImplementation(parameter.ParameterType);
                i++;
            }

            return Activator.CreateInstance(targetType, parameterObjects);
        }
    }
}
