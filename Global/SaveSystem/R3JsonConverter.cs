using R3;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Helpers
{
sealed class ReactivePropertyJsonConverterFactory : JsonConverterFactory
{
    // Creates a converter for each ReactiveProperty<T> type.
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(ReactiveProperty<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(ReactivePropertyJsonConverter<>).MakeGenericType(valueType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

sealed class ReactivePropertyJsonConverter<T> : JsonConverter<ReactiveProperty<T>>
{
    // Read the stored value back into a new ReactiveProperty<T>.
    public override ReactiveProperty<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return new ReactiveProperty<T>(default!);

        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        return new ReactiveProperty<T>(value!);
    }

    // Write only the current value to keep saves compact and readable.
    public override void Write(Utf8JsonWriter writer, ReactiveProperty<T> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
}