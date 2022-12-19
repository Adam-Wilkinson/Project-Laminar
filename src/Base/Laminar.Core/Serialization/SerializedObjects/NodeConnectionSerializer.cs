using Laminar_Core.NodeSystem.NodeComponents.Visuals;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_Core.Scripting.Advanced.Editing.Connection;
using Laminar_PluginFramework.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Serialization.SerializedObjects
{
    public class NodeConnectionSerializer : IObjectSerializer<INodeConnection>
    {
        public INodeConnection DeSerialize(ISerializedObject<INodeConnection> serialized, ISerializer serialize, object deserialiationContext)
        {
            if (deserialiationContext is not IAdvancedScriptEditor editor)
            {
                throw new ArgumentException("NodeConnectionSerializer needs a context of type IAdvancedScriptEditor to work");
            }

            if (serialized is not SerializedNodeConnection serializedNodeConnection)
            {
                throw new ArgumentException("NodeConnectionSerializer can only deserialize SerializedNodeConnections");
            }

            IConnector outputConnector = GetComponentByIndex(null, serializedNodeConnection.OutputFieldIndex).OutputConnector;
            IConnector inputConnector = GetComponentByIndex(null, serializedNodeConnection.InputFieldIndex).InputConnector;

            if (editor.TryConnectFields(outputConnector, inputConnector, out INodeConnection connection))
            {
                return connection;
            }

            throw new Exception("Unable to connect fields when deserializing");
        }

        private static IVisualNodeComponentContainer GetComponentByIndex(INodeContainer container, int index)
        {
            if (index == -1)
            {
                return container.Name;
            }

            return ((IList<IVisualNodeComponentContainer>)container.Fields)[index];
        }

        public ISerializedObject<INodeConnection> Serialize(INodeConnection toSerialize, ISerializer serialize)
        {
            return new SerializedNodeConnection(toSerialize.OutputConnector.ConnectorNode.Guid, toSerialize.OutputConnector.ParentComponentContainer.Child.IndexInParent, toSerialize.InputConnector.ConnectorNode.Guid, toSerialize.InputConnector.ParentComponentContainer.Child.IndexInParent);
        }

        public record SerializedNodeConnection(Guid OutputNodeGuid, int OutputFieldIndex, Guid InputNodeGuid, int InputFieldIndex) : ISerializedObject<INodeConnection>;
    }
}
