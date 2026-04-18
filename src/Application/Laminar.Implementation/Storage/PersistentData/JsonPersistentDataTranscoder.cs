using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Laminar.Contracts.Storage.PersistentData;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.PersistentData;

public class JsonPersistentDataTranscoder(JsonSerializerOptions jsonOptions, ILogger<JsonPersistentDataTranscoder> logger) : IPersistentDataTranscoder
{
    public JsonPersistentDataTranscoder(ILogger<JsonPersistentDataTranscoder> logger) : this(new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }, logger)
    {
    }

    public string FileExtension => ".json";

    public byte[] ToBytes(object value) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, jsonOptions));

    public T? FromBytes<T>(byte[] bytes) => JsonSerializer.Deserialize<T>(bytes, jsonOptions);

    public object? EncodeElement(object value)
    {
        try
        {
            return JsonSerializer.SerializeToElement(value, jsonOptions);
        }
        catch (JsonException jsonException)
        {
            logger.LogError(jsonException, jsonException.Message);
            return null;
        }
    }

    public object? DecodeElement(object value, Type targetType)
    {
        if (value is not JsonElement jsonElement) throw new InvalidOperationException();
        
        try
        {
            return jsonElement.Deserialize(targetType);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, ex.Message);
            return null;
        }
    } 
}