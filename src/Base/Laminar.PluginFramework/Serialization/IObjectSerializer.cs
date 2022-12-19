using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_PluginFramework.Serialization
{
    public interface IObjectSerializer<T>
    {
        ISerializedObject<T> Serialize(T toSerialize, ISerializer serializer);

        T DeSerialize(ISerializedObject<T> serialized, ISerializer serializer, object deserializationContext);
    }
}
