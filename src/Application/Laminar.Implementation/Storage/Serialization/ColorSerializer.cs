using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Storage.Serialization;

public class SystemColorSerializer : TypeSerializer<System.Drawing.Color, string>
{
    protected override string SerializeTyped(System.Drawing.Color toSerialize)
    {
        return toSerialize.ToString();
    }

    protected override System.Drawing.Color DeSerializeTyped(string serialized, object? deserializationContext = null)
    {
        return  System.Drawing.ColorTranslator.FromHtml(serialized);
    }
}