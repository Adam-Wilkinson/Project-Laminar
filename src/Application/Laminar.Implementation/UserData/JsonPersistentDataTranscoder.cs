using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Laminar.Contracts.UserData;
using System.Text.Json;
using System.Text.Json.Serialization;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.UserData;

public class JsonPersistentDataTranscoder : IPersistentDataTranscoder
{
    private readonly PersistentDataValueConverter _persistentDataConverter = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
    };

    public JsonPersistentDataTranscoder()
    {
        _jsonOptions.Converters.Add(_persistentDataConverter);
    }
    
    public string FileExtension => ".json";
    
    public byte[] Encode(Dictionary<string, IPersistentDataValue> objectToSave) 
        => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(objectToSave, _jsonOptions));

    public Dictionary<string, IPersistentDataValue> Decode(byte[] data,
        Dictionary<string, IPersistentDataValue> typeHints)
    {
        _persistentDataConverter.TypeHints = typeHints;
        return JsonSerializer.Deserialize<Dictionary<string, IPersistentDataValue>>(Encoding.UTF8.GetString(data), _jsonOptions) ?? [];
    } 
    
    private class PersistentDataValueConverter : JsonConverter<Dictionary<string, IPersistentDataValue>>
    {
        private readonly Dictionary<string, JsonElement> _notRecognisedData = [];
        
        public Dictionary<string, IPersistentDataValue> TypeHints { get; set; } = [];
        
        public override Dictionary<string, IPersistentDataValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            _notRecognisedData.Clear();
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }
            
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return TypeHints;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                if (reader.GetString() is not { } propertyName)
                {
                    throw new JsonException();
                }

                reader.Read();
                var value = JsonElement.ParseValue(ref reader);
                if (!TypeHints.TryGetValue(propertyName, out var dataVal))
                {
                    _notRecognisedData.Add(propertyName, value);
                    continue;
                }
                
                if (value.Deserialize(dataVal.SerializedType, options) is not { } readValue)
                {
                    throw new JsonException();
                }

                dataVal.SerializedValue = readValue;
            }

            return TypeHints;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, IPersistentDataValue> dict, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var (key, value) in dict)
            {
                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(key) ?? key);
                JsonSerializer.SerializeToElement(value.SerializedValue, options).WriteTo(writer);
            }

            foreach (var (key, unrecognisedElement) in _notRecognisedData)
            {
                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(key) ?? key);
                unrecognisedElement.WriteTo(writer);
            }
            
            writer.WriteEndObject();
        }
    }
}