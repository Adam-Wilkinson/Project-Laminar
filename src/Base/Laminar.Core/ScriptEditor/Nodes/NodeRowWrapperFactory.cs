using System;
using System.Reflection;
using Laminar.Contracts.NodeSystem;
using Laminar.Contracts.NodeSystem.Connection;
using Laminar.Contracts.Primitives;
using Laminar.Contracts.UserInterface;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;

namespace Laminar.Core.ScriptEditor.Nodes;

public class NodeRowWrapperFactory : INodeRowWrapperFactory
{
    private readonly IConnectorViewFactory _connectorFactory;
    private readonly IDisplayFactory _valueDisplayFactory;

    public NodeRowWrapperFactory(IConnectorViewFactory connectorFactory, IDisplayFactory displayFactory)
    {
        _connectorFactory = connectorFactory;
        _valueDisplayFactory = displayFactory;
    }

    public INodeRowWrapper CreateNodeRowWrapper(NodeRow row, INotificationClient<LaminarExecutionContext>? userChangedValueNotifiee)
    {
        return new NodeRowWrapper(row, _connectorFactory, _valueDisplayFactory, userChangedValueNotifiee);
    }

    public bool TryCreateNodeRowFromMember(MemberInfo memberInfo, object containingObject, out INodeRowWrapper nodeRowWrapper, INotificationClient<LaminarExecutionContext> userChangedValueNotifiee)
    {
        if (memberInfo.MemberType == MemberTypes.Field && GetNodeForFromFieldInfo((FieldInfo)memberInfo, containingObject, out NodeRow? row))
        {
            nodeRowWrapper = CreateNodeRowWrapper(row, userChangedValueNotifiee);
            return true;
        }

        if (memberInfo.MemberType == MemberTypes.Property && GetNodeRowFromPropertyInfo((PropertyInfo)memberInfo, containingObject, out NodeRow? propertyRow))
        {
            nodeRowWrapper = CreateNodeRowWrapper(propertyRow, userChangedValueNotifiee);
            return true;
        }

        nodeRowWrapper = null;
        return false;
    }

    private static bool GetNodeRowFromPropertyInfo(PropertyInfo propertyInfo, object containingObject, out NodeRow? row)
    {
        foreach (Attribute attribute in propertyInfo.GetCustomAttributes()) 
        {
            if (attribute is IConvertsToNodeRowAttribute converter)
            {
                row = converter.GenerateNodeRow(propertyInfo, containingObject);
                return true;
            }
        }

        row = null;
        return false;
    }

    private static bool GetNodeForFromFieldInfo(FieldInfo fieldInfo, object containingObject, out NodeRow? row)
    {
        if (typeof(NodeRow).IsAssignableFrom(fieldInfo.FieldType))
        {
            row = (NodeRow)fieldInfo.GetValue(containingObject);
            return true;
        }

        if (typeof(IValueInfo).IsAssignableFrom(fieldInfo.FieldType))
        {
            if (typeof(IInput).IsAssignableFrom(fieldInfo.FieldType))
            {
                IInput nodeInput = (IInput)fieldInfo.GetValue(containingObject);
                row = new NodeRow(fieldInfo.GetCustomAttribute<RemoveConnectorAttribute>() is null ? nodeInput : null, (IValueInfo)nodeInput, null);
                return true;
            }

            if (typeof(IOutput).IsAssignableFrom(fieldInfo.FieldType))
            {
                IOutput nodeOutput = (IOutput)fieldInfo.GetValue(containingObject);
                row = new NodeRow(null, (IValueInfo)nodeOutput, fieldInfo.GetCustomAttribute<RemoveConnectorAttribute>() is null ? nodeOutput : null);
                return true;
            }
        }

        row = null;
        return false;
    }
}