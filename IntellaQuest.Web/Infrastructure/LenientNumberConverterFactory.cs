using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IntellaQuest.Web.Infrastructure
{
    /// <summary>
    /// Makes number binding as forgiving as Newtonsoft.Json was in the old ASP.NET MVC 5 site:
    /// quoted numbers are accepted (e.g. <c>"5"</c>) and empty/missing strings fall back to the
    /// type's default value (e.g. <c>""</c> for an <c>int</c> becomes <c>0</c>) instead of
    /// throwing. System.Text.Json is strict about this by default, which broke clients that send
    /// empty strings for optional numeric fields (e.g. the home page's product filter).
    /// </summary>
    internal sealed class LenientNumberConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            // Only non-nullable numeric types; System.Text.Json wraps these automatically for
            // their Nullable<T> counterparts and delegates to the converter registered here.
            return IsNumeric(typeToConvert);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(LenientNumberConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType);
        }

        private static bool IsNumeric(Type type)
        {
            return type == typeof(byte) || type == typeof(sbyte)
                || type == typeof(short) || type == typeof(ushort)
                || type == typeof(int) || type == typeof(uint)
                || type == typeof(long) || type == typeof(ulong)
                || type == typeof(float) || type == typeof(double)
                || type == typeof(decimal);
        }
    }

    internal sealed class LenientNumberConverter<T> : JsonConverter<T>
        where T : struct
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return default;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var raw = reader.GetString();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    return default;
                }

                return (T)Convert.ChangeType(raw.Trim(), typeof(T), CultureInfo.InvariantCulture);
            }

            var target = typeof(T);

            if (target == typeof(double))
            {
                return (T)(object)reader.GetDouble();
            }

            if (target == typeof(float))
            {
                return (T)(object)reader.GetSingle();
            }

            if (target == typeof(decimal))
            {
                return (T)(object)reader.GetDecimal();
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt64(out var asLong))
                {
                    return (T)Convert.ChangeType(asLong, target, CultureInfo.InvariantCulture);
                }

                if (reader.TryGetUInt64(out var asULong))
                {
                    return (T)Convert.ChangeType(asULong, target, CultureInfo.InvariantCulture);
                }

                return (T)Convert.ChangeType(reader.GetDouble(), target, CultureInfo.InvariantCulture);
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(Convert.ToDecimal(value, CultureInfo.InvariantCulture));
        }
    }
}
