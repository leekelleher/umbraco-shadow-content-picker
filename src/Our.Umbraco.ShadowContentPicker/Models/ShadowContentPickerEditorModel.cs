using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;

namespace Our.Umbraco.ShadowContentPicker.Models
{
    internal class ShadowContentPickerEditorModel
    {
        [JsonProperty("udi")]
        public GuidUdi Udi { get; set; }

        [JsonProperty("contentTypeGuid")]
        public Guid ContentTypeGuid { get; set; }

        [JsonProperty("item")]
        public object Item { get; set; }

        [JsonProperty("content")]
        public JObject Content { get; set; }
    }
}