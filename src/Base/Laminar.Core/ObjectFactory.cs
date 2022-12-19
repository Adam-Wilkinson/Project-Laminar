using Laminar.Contracts.ActionSystem;
using Laminar.Contracts.NodeSystem;
using Laminar.Contracts.UserInterface;
using Laminar.Core;
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
using Laminar_Core.Scripting.Advanced.Compilation;
using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_Core.Scripting.Advanced.Editing.Connection;
using Laminar_Core.Scripting.Advanced.Instancing;
using Laminar_Core.Serialization;
using Laminar_PluginFramework.NodeSystem;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Collections;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.Primitives;
using Laminar_PluginFramework.Primitives.TypeDefinition;
using Laminar_PluginFramework.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Laminar_Core;

public class ObjectFactory : IObjectFactory
{
    private readonly Dictionary<Type, object> _implementations = new();
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
        RegisterImplementation<INodeConnectionFactory, NodeConnectionFactory>();
        RegisterImplementation<ITypeMapper<IVisualNodeComponent, IVisualNodeComponentContainer>, VisualNodeComponentContainerMapper>();
        RegisterImplementation<IAdvancedScript, AdvancedScript>();
        RegisterImplementation<IAdvancedScriptEditor, AdvancedScriptEditor>();
        RegisterImplementation<IVisualNodeComponentContainer, VisualNodeComponentContainer>();
        RegisterImplementation<IPoint, Point>();
        RegisterImplementation<IFlow, Flow>();
        RegisterImplementation<IAdvancedScriptCompiler, AdvancedScriptCompiler>();
        RegisterImplementation<IAdvancedScriptInstance, AdvancedScriptInstance>();
        RegisterImplementation<IAdvancedScriptInputs, AdvancedScriptInputs>();
        RegisterImplementation<ICompiledScript, CompiledScript>();
        RegisterImplementation<ICompiledScriptManager, CompiledScriptManager>();
        RegisterImplementation<IScriptCollection, ScriptCollection>();
        RegisterImplementation<ISerializer, Serializer>();
        RegisterImplementation<IUserDataStore, AppDataManager>();
        RegisterImplementationUnsafe(typeof(IObservableValue<>), typeof(ObservableValue<>));
        RegisterImplementationUnsafe(typeof(IDependentValue<>), typeof(DependentValue<>));
        RegisterImplementationUnsafe(typeof(IValueConstraint<>), typeof(ValueConstraint<>));
        RegisterImplementationUnsafe(typeof(ITypeDefinitionConstructor<>), typeof(TypeDefinitionConstructor<>));
        RegisterImplementationUnsafe(typeof(IObservableCollectionMapper<,>), typeof(ObservableCollectionMapper<,>));
    }

    public T GetImplementation<T>()
    {
        return (T)GetObjectOfType(typeof(T));
    }

    public T CreateInstance<T>()
    {
        return (T)GetObjectOfType(typeof(T));
    }

    public object CreateInstance(Type type)
    {
        return GetObjectOfType(type);
    }

    private object GetObjectOfType(Type type)
    {
        if (type == typeof(Instance))
        {
            return _instance;
        }

        if (type == typeof(IObjectFactory))
        {
            return this;
        }

        if (type.IsGenericType && _oldGenericImplementations.TryGetValue(type.GetGenericTypeDefinition(), out Type impl))
        {
            return CreateObject(impl.MakeGenericType(type.GetGenericArguments()));
        }

        if (type.IsInterface)
        {
            if (!_implementations.TryGetValue(type, out object implementationTypeObject))
            {
                throw new NotImplementedException($"No implementation found for interface {type}");
            }

            if (implementationTypeObject is not Type implementationType)
            {
                throw new Exception($"Internal Error");
            }

            return GetObjectOfType(implementationType);
        }

        if (!_implementations.ContainsKey(type))
        {
            _implementations.Add(type, CreateObject(type));
        }

        return _implementations[type];
    }

    private object CreateObject(Type type)
    {
        ConstructorInfo info = type.GetConstructors()[0];
        ParameterInfo[] parameters = info.GetParameters();
        object[] parameterObjects = new object[parameters.Length];
        int i = 0;
        foreach (ParameterInfo parameter in info.GetParameters())
        {
            parameterObjects[i] = GetObjectOfType(parameter.ParameterType);
            i++;
        }

        return Activator.CreateInstance(type, parameterObjects);
    }

    private IObjectFactory RegisterImplementation<TInterface, TImplementation>()
        where TImplementation : class, TInterface
    {
        _implementations.Add(typeof(TInterface), typeof(TImplementation));
        return this;
    }

    private readonly Dictionary<Type, Type> _oldGenericImplementations = new();

    private void RegisterImplementationUnsafe(Type originalType, Type implementationType)
    {
        _oldGenericImplementations[originalType.GetGenericTypeDefinition()] = implementationType.GetGenericTypeDefinition();
    }
}
