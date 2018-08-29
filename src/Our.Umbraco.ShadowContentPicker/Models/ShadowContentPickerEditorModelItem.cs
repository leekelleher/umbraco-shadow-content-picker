using System.Collections.Generic;
using Newtonsoft.Json;

namespace Our.Umbraco.ShadowContentPicker.Models
{
    internal class ShadowContentPickerEditorModelItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("trashed")]
        public bool Trashed { get; set; }

        [JsonProperty("published")]
        public bool Published { get; set; }

        [JsonProperty("overrides")]
        public List<object> Overrides { get; set; }
    }
}