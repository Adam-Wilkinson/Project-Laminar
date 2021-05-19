using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Serialization
{
    public interface IObjectSerializer<T>
    {
        ISerializer Serializer { get; set; }

        ISerializedObject<T> Serialize(T toSerialize);

        T DeSerialize(ISerializedObject<T> serialized, object deserializationContext);
    }
}
