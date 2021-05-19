using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Serialization
{
    public interface ISerializer
    {
        public ISerializedObject<T> Serialize<T>(T serializable);

        public IEnumerable<ISerializedObject<T>> Serialize<T>(IEnumerable<T> serializables);

        public T Deserialize<T>(object serialized, object deserializationContext);

        public IEnumerable<T> Deserialize<T>(IEnumerable<object> serializeds, object deserializationContext);
    }
}
