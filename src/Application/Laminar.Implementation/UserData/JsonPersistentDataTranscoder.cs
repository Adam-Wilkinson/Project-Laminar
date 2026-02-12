using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using Laminar.Contracts.UserData;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData;

public class JsonPersistentDataTranscoder(JsonSerializerOptions jsonOptions, ILogger<JsonPersistentDataTranscoder> logger) : IPersistentDataTranscoder<JsonElement>
{
    public JsonPersistentDataTranscoder(ILogger<JsonPersistentDataTranscoder> logger) : this(new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.Create(new UnicodeRange('+', 1)) }, logger)
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

    public object? DecodeValue(JsonElement element, Type targetType)
    {
        try
        {
            return element.Deserialize(targetType);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, ex.Message);
            return null;
        }
    } 
    
    public string FileExtension => ".json";
}