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
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Serialization.SerializedObjects
{
    public class NodeContainerSerializer : IObjectSerializer<INodeContainer>
    {
        readonly INodeFactory _nodeFactory;

        public ISerializer Serializer { get; set; }

        public NodeContainerSerializer(INodeFactory nodeFactory)
        {
            _nodeFactory = nodeFactory;
        }

        public ISerializedObject<INodeContainer> Serialize(INodeContainer toSerialize)
        {
            List<object> components = new();
            foreach (INodeComponent component in toSerialize.CoreNode.Fields)
            {
                components.Add(SerializeNodeComponent(component));
            }
            return new SerializedNodeContainer(toSerialize.Guid, toSerialize.CoreNode.GetType(), toSerialize.CoreNode.GetNameLabel().LabelText.Value, toSerialize.Location.X, toSerialize.Location.Y, components);
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
            if (serializedNodeContainer.CoreNodeType == typeof(InputNode))
            {
                coreNode = advancedScript.Inputs[serializedNodeContainer.Name];
            }
            else
            {
                coreNode = (INode)Activator.CreateInstance(serializedNodeContainer.CoreNodeType);
                coreNode.GetNameLabel().Name.Value = serializedNodeContainer.Name;
                foreach ((object serializedComponent, INodeComponent component) in serializedNodeContainer.SerializedComponents.Zip(coreNode.Fields))
                {
                    DeserializeNodeComponentTo(serializedComponent, component);
                }
            }

            INodeContainer output = (INodeContainer)typeof(INodeFactory).GetMethod(nameof(INodeFactory.Get)).MakeGenericMethod(serializedNodeContainer.CoreNodeType).Invoke(_nodeFactory, new object[] { coreNode, serializedNodeContainer.Guid });
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
                return new SerializedNodeComponentCollection(componentCollection.Select(x => SerializeNodeComponent(x)));
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

        public record SerializedNodeComponentCollection(IEnumerable<object> SerializedChildComponents);

        public record SerializedNodeContainer(
            Guid Guid,
            Type CoreNodeType,
            string Name,
            double X,
            double Y,
            List<object> SerializedComponents
        ) : ISerializedObject<INodeContainer>;
    }
}
