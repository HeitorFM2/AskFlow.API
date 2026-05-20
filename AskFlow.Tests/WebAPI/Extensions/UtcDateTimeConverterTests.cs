using System.Text.Json;
using AskFlow.WebAPI.Extensions;

namespace AskFlow.Tests.WebAPI.Extensions
{
    public class UtcDateTimeConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public UtcDateTimeConverterTests()
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(new UtcDateTimeConverter());
        }

        [Fact]
        public void Write_UtcKind_ShouldSerializeWithZSuffix()
        {
            var date = new DateTime(2026, 5, 20, 10, 30, 0, DateTimeKind.Utc);

            var json = JsonSerializer.Serialize(date, _options);

            json.Should().Be("\"2026-05-20T10:30:00Z\"");
        }

        [Fact]
        public void Write_UnspecifiedKind_ShouldSerializeWithZSuffix()
        {
            var date = new DateTime(2026, 5, 20, 10, 30, 0, DateTimeKind.Unspecified);

            var json = JsonSerializer.Serialize(date, _options);

            json.Should().Be("\"2026-05-20T10:30:00Z\"");
        }

        [Fact]
        public void Read_ShouldDeserializeWithUtcKind()
        {
            var json = "\"2026-05-20T10:30:00Z\"";

            var date = JsonSerializer.Deserialize<DateTime>(json, _options);

            date.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public void Read_ShouldPreserveDateTimeValue()
        {
            var json = "\"2026-05-20T10:30:00Z\"";

            var date = JsonSerializer.Deserialize<DateTime>(json, _options);

            date.Should().Be(new DateTime(2026, 5, 20, 10, 30, 0, DateTimeKind.Utc));
        }

        [Fact]
        public void WriteRead_RoundTrip_ShouldPreserveValueAndKind()
        {
            var original = new DateTime(2026, 5, 20, 15, 45, 30, DateTimeKind.Utc);

            var json = JsonSerializer.Serialize(original, _options);
            var result = JsonSerializer.Deserialize<DateTime>(json, _options);

            result.Should().Be(original);
            result.Kind.Should().Be(DateTimeKind.Utc);
        }
    }
}
