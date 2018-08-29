using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;

namespace Our.Umbraco.ShadowContentPicker.Models
{
    internal class ShadowContentPickerDbModel
    {
        [JsonProperty("udi")]
        public GuidUdi Udi { get; set; }

        [JsonProperty("content")]
        public JObject Content { get; set; }
    }
}