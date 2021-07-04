//using ProcessMemory.Types;
//using System;
//using System.Text.Json;
//using System.Text.Json.Serialization;

//namespace ProcessMemory.SystemTextJsonConverters
//{
//    public class Int24Converter : JsonConverter<Int24>
//    {
//        public override Int24 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetInt32();

//        public override void Write(Utf8JsonWriter writer, Int24 value, JsonSerializerOptions options) => writer.WriteNumberValue(value.Value);
//    }
//}
