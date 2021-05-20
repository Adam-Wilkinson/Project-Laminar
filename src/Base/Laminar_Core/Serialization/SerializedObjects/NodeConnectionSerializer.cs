using Laminar_Core.NodeSystem.NodeComponents.Visuals;
using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_Core.Scripting.Advanced.Editing.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Serialization.SerializedObjects
{
    public class NodeConnectionSerializer : IObjectSerializer<INodeConnection>
    {
        public ISerializer Serializer { get; set; }

        public INodeConnection DeSerialize(ISerializedObject<INodeConnection> serialized, object deserialiationContext)
        {
            if (deserialiationContext is not IAdvancedScriptEditor editor)
            {
                throw new ArgumentException("NodeConnectionSerializer needs a context of type IAdvancedScriptEditor to work");
            }

            if (serialized is not SerializedNodeConnection serializedNodeConnection)
            {
                throw new ArgumentException("NodeConnectionSerializer can only deserialize SerializedNodeConnections");
            }

            IConnector outputConnector = ((IList<IVisualNodeComponentContainer>)editor.GetNode(serializedNodeConnection.OutputNodeGuid).Fields)[serializedNodeConnection.OutputFieldIndex].OutputConnector;
            IConnector inputConnector = ((IList<IVisualNodeComponentContainer>)editor.GetNode(serializedNodeConnection.InputNodeGuid).Fields)[serializedNodeConnection.InputFieldIndex].InputConnector;

            if (editor.TryConnectFields(outputConnector, inputConnector, out INodeConnection connection))
            {
                return connection;
            }

            throw new Exception("Unable to connect fields when deserializing");
        }

        public ISerializedObject<INodeConnection> Serialize(INodeConnection toSerialize)
        {
            return new SerializedNodeConnection(toSerialize.OutputConnector.ConnectorNode.Guid, toSerialize.OutputConnector.ParentComponentContainer.Child.IndexInParent, toSerialize.InputConnector.ConnectorNode.Guid, toSerialize.InputConnector.ParentComponentContainer.Child.IndexInParent);
        }

        public record SerializedNodeConnection(Guid OutputNodeGuid, int OutputFieldIndex, Guid InputNodeGuid, int InputFieldIndex) : ISerializedObject<INodeConnection>;
    }
}
