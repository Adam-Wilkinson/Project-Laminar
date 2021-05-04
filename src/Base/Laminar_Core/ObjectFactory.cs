using Laminar_Core.NodeSystem;
using Laminar_Core.NodeSystem.NodeComponents.Collections;
using Laminar_Core.NodeSystem.NodeComponents.Visuals;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.NodeSystem.NodeTreeSystem;
using Laminar_Core.Primitives;
using Laminar_Core.Primitives.LaminarValue;
using Laminar_Core.Primitives.LaminarValue.TypeDefinition;
using Laminar_Core.Primitives.ObservableCollectionMapper;
using Laminar_Core.Primitives.UserInterface;
using Laminar_Core.Scripting;
using Laminar_Core.Scripting.Advanced;
using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_Core.Scripting.Advanced.Editing.Connection;
using Laminar_Core.Scripting.Advanced.Instancing;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Collections;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.Primitives;
using Laminar_PluginFramework.Primitives.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Laminar_Core
{
    public class ObjectFactory : IObjectFactory
    {
        private readonly Dictionary<Type, Type> _interfaceImplementations = new();
        private readonly Instance _instance;

        public ObjectFactory(Instance instance)
        {
            _instance = instance;

            RegisterImplementation<IOpacity, Opacity>();
            RegisterImplementation<INodeField, NodeField>();
            RegisterImplementation<INodeLabel, NodeLabel>();
            RegisterImplementation<IEditableNodeLabel, EditableNodeLabel>();
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
            RegisterImplementation<IAdvancedScript, AdvancedScript>();
            RegisterImplementation<IAdvancedScriptEditor, AdvancedScriptEditor>();
            RegisterImplementation<IVisualNodeComponentContainer, VisualNodeComponentContainer>();
            RegisterImplementation<IPoint, Point>();
            RegisterImplementation<IFlow, Flow>();
            RegisterImplementation<IAdvancedScriptInputs, AdvancedScriptInputs>();
            RegisterImplementation<IScriptCollection, ScriptCollection>();
            RegisterImplementation<IAdvancedScriptInstance, AdvancedScriptInstance>();
            RegisterImplementationUnsafe(typeof(IObservableValue<>), typeof(ObservableValue<>));
            RegisterImplementationUnsafe(typeof(IDependentValue<>), typeof(DependentValue<>));
            RegisterImplementationUnsafe(typeof(IValueConstraint<>), typeof(ValueConstraint<>));
            RegisterImplementationUnsafe(typeof(ITypeDefinitionConstructor<>), typeof(TypeDefinitionConstructor<>));
        }

        public T GetImplementation<T>()
        {
            return (T)CreateInstance(GetLooseTypedImplementation(typeof(T)));
        }

        public T CreateInstance<T>()
        {
            return (T)CreateInstance(typeof(T));
        }

        public object CreateInstance(Type type)
        {
            if (type == typeof(IObjectFactory))
            {
                return this;
            }
            
            if (type == typeof(Instance))
            {
                return _instance;
            }

            if (TryGetImplementation(type, out Type implementationType))
            {
                return CreateInstance(implementationType);
            }

            if (type.GetConstructor(Type.EmptyTypes) is not null)
            {
                return Activator.CreateInstance(type);
            }

            ConstructorInfo info = type.GetConstructors()[0];
            ParameterInfo[] parameters = info.GetParameters();
            object[] parameterObjects = new object[parameters.Length];
            int i = 0;
            foreach (ParameterInfo parameter in info.GetParameters())
            {
                parameterObjects[i] = CreateInstance(parameter.ParameterType);
                i++;
            }

            return Activator.CreateInstance(type, parameterObjects);
        }


        private Type GetLooseTypedImplementation(Type typeToGet)
        {
            if (TryGetImplementation(typeToGet, out Type typeToMake))
            {
                return typeToMake;
            }

            throw new NotSupportedException($"No implementation found for {typeToMake}");
        }

        private bool TryGetImplementation(Type inputType, out Type outputType)
        {
            if (_interfaceImplementations.TryGetValue(inputType, out Type implementation))
            {
                outputType = implementation;
                return true;
            }

            if (inputType.IsGenericType && _interfaceImplementations.TryGetValue(inputType.GetGenericTypeDefinition(), out Type implementationType))
            {
                outputType = implementationType.MakeGenericType(inputType.GetGenericArguments());
                return true;
            }

            outputType = default;
            return false;
        }

        private IObjectFactory RegisterImplementation<TInterface, TImplementation>()
            where TImplementation : class, TInterface
        {
            _interfaceImplementations.Add(typeof(TInterface), typeof(TImplementation));
            return this;
        }

        private void RegisterImplementationUnsafe(Type interfaceType, Type implementationType)
        {
            Type[] interfaces = implementationType.GetInterfaces();
            if (!(implementationType.GetInterfaces().Any(x => x.IsGenericType && (x.GetGenericTypeDefinition() == interfaceType)) && implementationType.IsClass))
            {
                throw new ArgumentException($"Type {implementationType} is not a class that inherits from {interfaceType}");
            }

            _interfaceImplementations.Add(interfaceType, implementationType);
        }
    }
}
