using System;
using Newtonsoft.Json;
using Skybrud.Umbraco.Spa.Models.Meta;

namespace Skybrud.Umbraco.Spa.Json.Converters {

    /// <summary>
    /// JSON converter class for serializing an instance of <see cref="SpaMetaData"/> to JSON. This converter does not support deserialization.
    /// </summary>
    public class SpaMetaDataJsonConverter : JsonConverter {
        
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            if (!(value is SpaMetaData data)) throw new ArgumentException("Must be an instance of SpaMetaData", nameof(value));
            data.WriteJson(writer);
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType) {
            return false;
        }

    }

}