using OpenFlow_Core.Nodes;
using OpenFlow_Core.Nodes.Connection;
using OpenFlow_Core.Nodes.NodeComponents.Collections;
using OpenFlow_Core.Nodes.NodeComponents.Visuals;
using OpenFlow_Core.Primitives;
using OpenFlow_Core.Primitives.LaminarValue;
using OpenFlow_Core.Primitives.LaminarValue.TypeDefinition;
using OpenFlow_Core.Primitives.ObservableCollectionMapper;
using OpenFlow_Core.Primitives.UserInterface;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Collections;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;
using OpenFlow_PluginFramework.Primitives.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            RegisterImplementation<INodeFactory, NodeFactory>();
            RegisterImplementation<IConnector, Connector>();
            RegisterImplementation<INodeConnectionFactory, NodeConnectionFactory>();
            RegisterImplementation<ITypeMapper<IVisualNodeComponent, IVisualNodeComponentContainer>, VisualNodeComponentContainerMapper>();
            RegisterImplementation<IVisualNodeComponentContainer, VisualNodeComponentContainer>();
            RegisterImplementationUnsafe(typeof(IObservableValue<>), typeof(ObservableValue<>));
            RegisterImplementationUnsafe(typeof(IValueConstraint<>), typeof(ValueConstraint<>));
            RegisterImplementationUnsafe(typeof(ITypeDefinitionConstructor<>), typeof(TypeDefinitionConstructor<>));
        }

        public T GetImplementation<T>()
        {
            return (T)GetLooseTypedImplementation(typeof(T));
        }

        public T CreateInstance<T>()
        {
            return (T)CreateInstance(typeof(T));
        }

        public object CreateInstance(Type targetType)
        {
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
                if (parameter.ParameterType.GetInterfaces().Contains(typeof(IDependencyAggregate)))
                {
                    parameterObjects[i] = CreateInstance(parameter.ParameterType);
                }
                else
                {
                    parameterObjects[i] = GetLooseTypedImplementation(parameter.ParameterType);
                }
                i++;
            }

            return Activator.CreateInstance(targetType, parameterObjects);
        }

        private IObjectFactory RegisterImplementation<TInterface, TImplementation>()
            where TImplementation : class, TInterface
        {
            interfaceImplementations.Add(typeof(TInterface), typeof(TImplementation));
            return this;
        }

        private void RegisterImplementationUnsafe(Type interfaceType, Type implementationType)
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
                if (interfaceImplementations.TryGetValue(typeToGet, out Type implementation))
                {
                    targetType = implementation;
                }
                else
                {
                    targetType = interfaceImplementations[typeToGet.GetGenericTypeDefinition()];
                    targetType = targetType.MakeGenericType(typeToGet.GetGenericArguments());
                }
            }
            else
            {
                targetType = interfaceImplementations[typeToGet];
            }

            return CreateInstance(targetType);
        }

        public interface IDependencyAggregate { }
    }
}
