using ProcessMemory.Common.Types;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProcessMemory.Common.SystemTextJsonConverters
{
    public class UInt24Converter : JsonConverter<UInt24>
    {
        public override UInt24 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetUInt32();

        public override void Write(Utf8JsonWriter writer, UInt24 value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
    }
}
