namespace Snail.MCP.AfterEffects.Tools;

/// <summary>Builds the params of a bridge request, leaving out what the caller did not pass so the bridge's defaults apply.</summary>
internal static class Arguments
{
    extension(JsonObject arguments)
    {
        public JsonObject With(string key, string? value) => arguments.Set(key, value is null ? null : JsonValue.Create(value));

        public JsonObject With(string key, bool? value) => arguments.Set(key, value is null ? null : JsonValue.Create(value.Value));

        public JsonObject With(string key, int? value) => arguments.Set(key, value is null ? null : JsonValue.Create(value.Value));

        public JsonObject With(string key, double? value) => arguments.Set(key, value is null ? null : JsonValue.Create(value.Value));

        public JsonObject With(string key, int[]? value) =>
            arguments.Set(key, value is null ? null : new JsonArray([.. value.Select(item => (JsonNode)JsonValue.Create(item))]));

        public JsonObject With(string key, double[]? value) =>
            arguments.Set(key, value is null ? null : new JsonArray([.. value.Select(item => (JsonNode)item)]));

        public JsonObject With(string key, string[]? value) =>
            arguments.Set(key, value is null ? null : new JsonArray([.. value.Select(item => (JsonNode)item)]));

        public JsonObject With(string key, JsonNode? value) => arguments.Set(key, value?.DeepClone());

        /// <summary>An item or layer named either by its name or by its id: a number travels as a number, so the bridge looks it up by id.</summary>
        public JsonObject WithReference(string key, string? value) =>
            value is null ? arguments
            : int.TryParse(value, out var id) ? arguments.Set(key, JsonValue.Create(id))
            : arguments.Set(key, JsonValue.Create(value));

        /// <summary>Several items or layers, each by name or id.</summary>
        public JsonObject WithReferences(string key, string[]? values) =>
            values is null ? arguments
            : arguments.Set(key, new JsonArray([.. values.Select(value => int.TryParse(value, out var id) ? (JsonNode)id : value)]));

        /// <summary>A parent layer, where an empty string means "no parent" and travels as an explicit null.</summary>
        public JsonObject WithParent(string? parent)
        {
            if (parent is null)
            {
                return arguments;
            }

            arguments["parent"] = parent.Length == 0 ? null : int.TryParse(parent, out var index) ? JsonValue.Create(index) : JsonValue.Create(parent);

            return arguments;
        }

        /// <summary>A mask by name or index, where the word none travels as an explicit null, which the bridge reads as "take the text off its path".</summary>
        public JsonObject WithMask(string? mask)
        {
            if (mask is null)
            {
                return arguments;
            }

            arguments["mask"] = string.Equals(mask, "none", StringComparison.OrdinalIgnoreCase) ? null : int.TryParse(mask, out var index) ? JsonValue.Create(index) : JsonValue.Create(mask);

            return arguments;
        }

        /// <summary>The matte layer and its type, as the two parameters the bridge takes.</summary>
        public JsonObject WithTrackMatte(string? layer, string? type) =>
            arguments.WithReference("track_matte", layer).With("track_matte_type", type);

        private JsonObject Set(string key, JsonNode? node)
        {
            if (node is not null)
            {
                arguments[key] = node;
            }

            return arguments;
        }
    }
}
