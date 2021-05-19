using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting.Advanced;
using Laminar_Core.Scripting.Advanced.Editing.Connection;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Serialization.SerializedObjects
{
    public class AdvancedScriptSerializer : IObjectSerializer<IAdvancedScript>
    {
        private readonly IObjectFactory _factory;

        public ISerializer Serializer { get; set; }

        public AdvancedScriptSerializer(IObjectFactory factory)
        {
            _factory = factory;
        }

        public IAdvancedScript DeSerialize(ISerializedObject<IAdvancedScript> serialized, object deserializationContext)
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
                output.Editor.Inputs.Add(kvp.Key, kvp.Value);
            }
            output.UpdateInputs();

            foreach (INodeContainer node in Serializer.Deserialize<INodeContainer>(serializedAdvancedScript.Nodes, output))
            {
                output.Editor.AddNode(node);
            }

            Serializer.Deserialize<INodeConnection>(serializedAdvancedScript.Connections, output.Editor);

            output.IsBeingEdited = false;

            return output;

        }

        public ISerializedObject<IAdvancedScript> Serialize(IAdvancedScript toSerialize)
        {
            return new SerializedAdvancedScript(toSerialize.Name.Value, Serializer.Serialize((IEnumerable<INodeContainer>)toSerialize.Editor.Nodes), Serializer.Serialize(toSerialize.Editor.Connections), toSerialize.Inputs.ToDictionary(x => x.Key, x => x.Value.Value));
        }

        public record SerializedAdvancedScript(
            string Name,
            IEnumerable<ISerializedObject<INodeContainer>> Nodes,
            IEnumerable<ISerializedObject<INodeConnection>> Connections,
            Dictionary<string, object> Inputs
            ) : ISerializedObject<IAdvancedScript>;
    }
}
