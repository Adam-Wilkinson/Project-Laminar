using Laminar_Core.NodeSystem;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting.Advanced;
using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Collections;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Serialization.SerializedObjects
{
    public class NodeContainerSerializer : IObjectSerializer<INodeContainer>
    {
        readonly MethodInfo _getNodeMethod;
        readonly INodeFactory _nodeFactory;
        readonly Instance _instance;

        public ISerializer Serializer { get; set; }

        public NodeContainerSerializer(Instance instance, INodeFactory nodeFactory)
        {
            _nodeFactory = nodeFactory;
            _instance = instance;
            foreach (MethodInfo methodInfo in typeof(INodeFactory).GetMethods())
            {
                IEnumerable<Type> MethodTypes = methodInfo.GetParameters().Select(x => x.ParameterType);
                Type[] MyTypes = new Type[] { methodInfo.GetGenericArguments()[0], typeof(Type) };
                if (methodInfo.Name == nameof(INodeFactory.Get) && methodInfo.ContainsGenericParameters && methodInfo.GetParameters().Select(x => x.ParameterType).SequenceEqual(new Type[] { methodInfo.GetGenericArguments()[0], typeof(Guid) }))
                {
                    _getNodeMethod = methodInfo;
                }
            }
        }

        public ISerializedObject<INodeContainer> Serialize(INodeContainer toSerialize)
        {
            List<object> components = new();
            foreach (INodeComponent component in toSerialize.CoreNode.Fields)
            {
                components.Add(SerializeNodeComponent(component));
            }
            return new SerializedNodeContainer(toSerialize.Guid, toSerialize.CoreNode.NodeName, _instance.GetNodePlugin(toSerialize.CoreNode).PluginName, toSerialize.CoreNode.GetNameLabel().LabelText.Value, toSerialize.Location.X, toSerialize.Location.Y, components);
        }

        public INodeContainer DeSerialize(ISerializedObject<INodeContainer> serialized, object deserializationContext)
        {
            if (serialized is not SerializedNodeContainer serializedNodeContainer)
            {
                throw new ArgumentException("Can only deserialize objects of type SerializedNodeContainer", nameof(serialized));
            }

            if (deserializationContext is not IAdvancedScript advancedScript)
            {
                throw new ArgumentException("NodeContainerSerializer needs a context of type IAdvancedScript to work");
            }

            INode coreNode;
            INodeContainer output;
            Type coreNodeType = _instance.GetNodeType(serializedNodeContainer.CoreNodeName, serializedNodeContainer.Plugin);
            if (coreNodeType == typeof(InputNode))
            {
                coreNode = advancedScript.Inputs[serializedNodeContainer.Name];
                output = (INodeContainer)_getNodeMethod.MakeGenericMethod(coreNodeType).Invoke(_nodeFactory, new object[] { coreNode, serializedNodeContainer.Guid });
            }
            else
            {
                coreNode = (INode)Activator.CreateInstance(coreNodeType);
                MethodInfo GetNodeMethod = _getNodeMethod.MakeGenericMethod(coreNodeType);
                output = (INodeContainer)GetNodeMethod.Invoke(_nodeFactory, new object[] { coreNode, serializedNodeContainer.Guid });
                coreNode.GetNameLabel().Name.Value = serializedNodeContainer.Name;
                foreach ((object serializedComponent, INodeComponent component) in serializedNodeContainer.SerializedComponents.Zip(coreNode.Fields))
                {
                    component.ParentNode = coreNode;
                    DeserializeNodeComponentTo(serializedComponent, component);
                }
            }
            output.Location.X = serializedNodeContainer.X;
            output.Location.Y = serializedNodeContainer.Y;
            return output;
        }

        private object SerializeNodeComponent(INodeComponent component)
        {
            if (component is INodeField nodeField)
            {
                return new SerializedNodeField(nodeField.Name.Value, nodeField.AllValues.ToDictionary(x => x.Key, x => x.Value.Value));
            }

            if (component is INodeLabel nodeLabel)
            {
                return new SerializedNodeLabel(nodeLabel.LabelText.Value);
            }

            if (component is INodeComponentCollection componentCollection)
            {
                return new SerializedNodeComponentCollection(componentCollection.Select(x => SerializeNodeComponent(x)).ToList());
            }

            return null;
        }

        private void DeserializeNodeComponentTo(object serializedNodeComponent, INodeComponent serializeTo)
        {
            if (serializedNodeComponent is SerializedNodeField serializedNodeField && serializeTo is INodeField nodeField)
            {
                nodeField.Name.Value = serializedNodeField.Name;
                foreach (var kvp in serializedNodeField.Values)
                {
                    nodeField[kvp.Key] = kvp.Value;
                }
            }

            if (serializedNodeComponent is SerializedNodeLabel serializedNodeLabel && serializeTo is INodeLabel nodeLabel)
            {
                nodeLabel.Name.Value = serializedNodeLabel.LabelText;
            }

            if (serializedNodeComponent is SerializedNodeComponentCollection serializedNodeComponentCollection && serializeTo is INodeComponentCollection nodeComponentCollection)
            {
                foreach ((object serializedChildComponent, INodeComponent childSerializeTo) in serializedNodeComponentCollection.SerializedChildComponents.Zip(nodeComponentCollection))
                {
                    DeserializeNodeComponentTo(serializedChildComponent, childSerializeTo);
                }
            }
        }

        public record SerializedNodeField(string Name, Dictionary<object, object> Values);

        public record SerializedNodeLabel(string LabelText);

        public record SerializedNodeComponentCollection(List<object> SerializedChildComponents);

        public record SerializedNodeContainer(
            Guid Guid,
            string CoreNodeName,
            string Plugin,
            string Name,
            double X,
            double Y,
            List<object> SerializedComponents
        ) : ISerializedObject<INodeContainer>;
    }
}
