using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Laminar.Contracts.Storage.PersistentData;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.PersistentData;

public class JsonPersistentDataTranscoder(JsonSerializerOptions jsonOptions, ILogger<JsonPersistentDataTranscoder> logger) : IPersistentDataTranscoder
{
    public JsonPersistentDataTranscoder(ILogger<JsonPersistentDataTranscoder> logger) : this(new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.Create(new UnicodeRange('+', 1)) }, logger)
    {
    }

    public string FileExtension => ".json";

    public object? EncodeValue(object value) => JsonSerializer.SerializeToElement(value, jsonOptions);

    public object? DecodeValue(object value, Type targetType)
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