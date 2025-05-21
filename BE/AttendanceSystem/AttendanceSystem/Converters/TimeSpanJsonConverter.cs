using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AttendanceSystem.Converters
{
    public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        private const string DefaultFormat = @"hh\:mm\:ss";

        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringValue = reader.GetString();

            if (TimeSpan.TryParse(stringValue, out var timeSpan))
            {
                return timeSpan;
            }

            throw new JsonException($"Định dạng TimeSpan không hợp lệ. Hãy sử dụng định dạng hợp lệ ví dụ: {DefaultFormat}");
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            // Luôn xuất TimeSpan theo định dạng hh:mm:ss để đồng nhất
            writer.WriteStringValue(value.ToString(DefaultFormat));
        }
    }
}
