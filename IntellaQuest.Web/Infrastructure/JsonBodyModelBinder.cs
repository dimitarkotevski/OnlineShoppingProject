using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IntellaQuest.Web.Infrastructure
{
    /// <summary>
    /// Supplies <see cref="JsonBodyModelBinder"/> for every action parameter.
    /// </summary>
    public sealed class JsonBodyModelBinderProvider : IModelBinderProvider
    {
        private static readonly JsonBodyModelBinder Binder = new JsonBodyModelBinder();

        public IModelBinder GetBinder(ModelBinderProviderContext context) => Binder;
    }

    /// <summary>
    /// Replicates the ASP.NET MVC 5 "JsonValueProviderFactory" binding behaviour that the
    /// Angular client relies on: each action parameter is bound from an equally named
    /// property of the JSON request body, or from the whole body when the parameter is a
    /// complex type and no matching property exists. Route and query values take precedence.
    /// </summary>
    public sealed class JsonBodyModelBinder : IModelBinder
    {
        private const string BodyCacheKey = "__intellaquest.json.body";

        private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

        private static JsonSerializerOptions CreateSerializerOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Match Newtonsoft.Json's forgiving number handling (quoted numbers, empty strings).
            options.Converters.Add(new LenientNumberConverterFactory());

            return options;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var httpContext = bindingContext.HttpContext;
            var name = bindingContext.ModelName;
            var modelType = bindingContext.ModelType;

            // 1. Route values (e.g. api/Identity/GetUserDetails/{id}).
            if (!string.IsNullOrEmpty(name)
                && httpContext.Request.RouteValues.TryGetValue(name, out var routeValue)
                && routeValue is string routeText
                && !string.IsNullOrEmpty(routeText)
                && TryConvertScalar(routeText, modelType, out var fromRoute))
            {
                bindingContext.Result = ModelBindingResult.Success(fromRoute);
                return;
            }

            // 2. Query string.
            if (!string.IsNullOrEmpty(name)
                && httpContext.Request.Query.TryGetValue(name, out var queryValue)
                && queryValue.Count > 0
                && TryConvertScalar(queryValue[0], modelType, out var fromQuery))
            {
                bindingContext.Result = ModelBindingResult.Success(fromQuery);
                return;
            }

            // 3. JSON request body.
            var body = await GetBodyAsync(httpContext);
            if (body != null && body.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (!string.IsNullOrEmpty(name) && TryGetPropertyIgnoreCase(body.RootElement, name, out var property))
                {
                    bindingContext.Result = ModelBindingResult.Success(Deserialize(property, modelType));
                    return;
                }

                if (IsComplexType(modelType))
                {
                    bindingContext.Result = ModelBindingResult.Success(Deserialize(body.RootElement, modelType));
                    return;
                }
            }

            // Nothing matched: leave the parameter at its default value (supports optional
            // parameters such as int quality = 1).
            bindingContext.Result = ModelBindingResult.Failed();
        }

        /// <summary>
        /// Looks up a JSON property by name, ignoring case. ASP.NET MVC 5's JSON value
        /// provider matched property names case-insensitively, so requests that send
        /// <c>{ "Id": ... }</c> must bind to an action parameter named <c>id</c>.
        /// </summary>
        private static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
        {
            if (element.TryGetProperty(name, out value))
            {
                return true;
            }

            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static object Deserialize(JsonElement element, Type type)
        {
            if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
            {
                return DefaultValue(type);
            }

            return JsonSerializer.Deserialize(element.GetRawText(), type, SerializerOptions);
        }

        private static bool TryConvertScalar(string raw, Type type, out object value)
        {
            var target = Nullable.GetUnderlyingType(type) ?? type;

            try
            {
                if (target == typeof(string))
                {
                    value = raw;
                    return true;
                }

                if (target == typeof(Guid))
                {
                    if (Guid.TryParse(raw, out var guid))
                    {
                        value = guid;
                        return true;
                    }

                    value = null;
                    return false;
                }

                if (target.IsEnum)
                {
                    value = Enum.Parse(target, raw, ignoreCase: true);
                    return true;
                }

                value = Convert.ChangeType(raw, target);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        private static bool IsComplexType(Type type)
        {
            var target = Nullable.GetUnderlyingType(type) ?? type;

            return !(target.IsPrimitive
                || target.IsEnum
                || target == typeof(string)
                || target == typeof(decimal)
                || target == typeof(Guid)
                || target == typeof(DateTime)
                || target == typeof(DateTimeOffset)
                || target == typeof(TimeSpan));
        }

        private static object DefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private static async Task<JsonDocument> GetBodyAsync(HttpContext httpContext)
        {
            if (httpContext.Items.TryGetValue(BodyCacheKey, out var cached))
            {
                return cached as JsonDocument;
            }

            JsonDocument document = null;
            try
            {
                httpContext.Request.EnableBuffering();

                if (httpContext.Request.ContentLength.GetValueOrDefault() > 0
                    || httpContext.Request.Headers.ContainsKey("Transfer-Encoding"))
                {
                    using var reader = new StreamReader(
                        httpContext.Request.Body,
                        Encoding.UTF8,
                        detectEncodingFromByteOrderMarks: false,
                        bufferSize: 1024,
                        leaveOpen: true);

                    var text = await reader.ReadToEndAsync();
                    httpContext.Request.Body.Position = 0;

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        try
                        {
                            document = JsonDocument.Parse(text);
                        }
                        catch (JsonException)
                        {
                            document = null;
                        }
                    }
                }
            }
            catch
            {
                document = null;
            }

            httpContext.Items[BodyCacheKey] = document;
            return document;
        }
    }
}
