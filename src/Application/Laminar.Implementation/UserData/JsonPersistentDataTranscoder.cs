using System;
using System.Collections.Generic;
using System.IO;
using Laminar.Contracts.UserData;
using System.Text.Json;

namespace Laminar.Implementation.UserData;

public class JsonPersistentDataTranscoder(JsonSerializerOptions jsonOptions) : IPersistentDataTranscoder<JsonElement>
{
    public JsonPersistentDataTranscoder() : this(new JsonSerializerOptions { WriteIndented = true })
    {
    }

    public byte[] EncodeDictionary<T>(Dictionary<string, T> dict, Func<T, JsonElement> converter)
    {
        var options = new JsonWriterOptions
        {
            Indented = jsonOptions.WriteIndented,
        };
        
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, options);

        writer.WriteStartObject();
        foreach (var kvp in dict)
        {
            writer.WritePropertyName(kvp.Key);
            converter(kvp.Value).WriteTo(writer);
        }
        
        writer.WriteEndObject();
        writer.Flush();
        
        return stream.ToArray();
    }

    public void DecodeByteArray(byte[] encoded, Action<string, JsonElement> decodeAction)
    {
        var options = new JsonReaderOptions
        {
            AllowTrailingCommas = true,
        };
        
        var reader = new Utf8JsonReader(encoded, options);

        if (encoded.Length == 0) return;

        while (reader.TokenType == JsonTokenType.None)
        {
            reader.Read();
        }
        
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return;
            }

            if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() is not { } propertyName)
            {
                throw new JsonException();
            }

            reader.Read();

            var value = JsonElement.ParseValue(ref reader);

            decodeAction(propertyName, value);
        }
    }

    public JsonElement EncodeValue(object value) => JsonSerializer.SerializeToElement(value, jsonOptions);

    public object DecodeValue(JsonElement element, Type targetType) => element.Deserialize(targetType)!;
    
    public string FileExtension => ".json";
}