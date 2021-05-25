using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_PluginFramework.Serialization
{
    public interface ISerializer
    {
        public object TrySerializeObject(object toSerialize);

        public object TryDeserializeObject(object serialized, object deserializationContext);

        public void RegisterSerializer<T>(IObjectSerializer<T> serializer);

        public ISerializedObject<T> Serialize<T>(T serializable);

        public T Deserialize<T>(ISerializedObject<T> serialized, object deserializationContext);
    }
}
