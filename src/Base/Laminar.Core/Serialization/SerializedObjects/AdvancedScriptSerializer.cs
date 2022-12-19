using Laminar.Contracts.NodeSystem;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting.Advanced;
using Laminar_Core.Scripting.Advanced.Editing.Connection;
using Laminar_PluginFramework.Primitives;
using Laminar_PluginFramework.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Serialization.SerializedObjects
{
    public class AdvancedScriptSerializer// : IObjectSerializer<IAdvancedScript>
    {
        private readonly IObjectFactory _factory;

        public AdvancedScriptSerializer(IObjectFactory factory)
        {
            _factory = factory;
        }

        public IAdvancedScript DeSerialize(ISerializedObject<IAdvancedScript> serialized, ISerializer serializer, object deserializationContext)
        {
            if (serialized is not SerializedAdvancedScript serializedAdvancedScript)
            {
                throw new ArgumentException("AdvancedScriptSerializer can only deserialize objects of type SerializedAdvancedScript");
            }

            IAdvancedScript output = _factory.GetImplementation<IAdvancedScript>();
            output.Name.Value = serializedAdvancedScript.Name;
            output.IsBeingEdited = true;

            foreach (var kvp in serializedAdvancedScript.Inputs)
            {
                output.Editor.Inputs.Add(kvp.Key, serializer.TryDeserializeObject(kvp.Value, null));
            }
            output.UpdateInputs();

            foreach (var node in serializedAdvancedScript.Nodes)
            {
                output.Editor.AddNode(serializer.Deserialize(node, output));
            }

            foreach (ISerializedObject<INodeConnection> connection in serializedAdvancedScript.Connections)
            {
                serializer.Deserialize(connection, output.Editor);
            }

            output.IsBeingEdited = false;

            return output;

        }

        public ISerializedObject<IAdvancedScript> Serialize(IAdvancedScript toSerialize, ISerializer serializer)
        {
            return new SerializedAdvancedScript(toSerialize.Name.Value, toSerialize.Editor.Nodes.Select(x => serializer.Serialize(x)).ToList(), toSerialize.Editor.Connections.Select(x => serializer.Serialize(x)).ToList(), toSerialize.Inputs.ToDictionary(x => x.Key, x => serializer.TrySerializeObject(x.Value.Value)));
        }

        public record SerializedAdvancedScript(
            string Name,
            List<ISerializedObject<INodeWrapper>> Nodes,
            List<ISerializedObject<INodeConnection>> Connections,
            Dictionary<string, object> Inputs
            ) : ISerializedObject<IAdvancedScript>;
    }
}
