using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Deploy.Contrib.Connectors.ValueConnectors;
using Umbraco.Deploy.ValueConnectors;
using static Umbraco.Deploy.Contrib.Connectors.ValueConnectors.InnerContentConnector;

namespace Umbraco.Deploy.Contrib.Connectors.ValueConnectors
{
    public class ShadowContentPickerValueConnector : IValueConnector
    {
        private readonly IEntityService _entityService;
        private readonly IValueConnector _innerContentValueConnector;

        public ShadowContentPickerValueConnector(
            IContentTypeService contentTypeService,
            IEntityService entityService,
            Lazy<ValueConnectorCollection> valueConnectors)
        {
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
            _innerContentValueConnector = new InnerContentValueConnectorWrapped(contentTypeService, valueConnectors);
        }

        public IEnumerable<string> PropertyEditorAliases => new[] { "Our.Umbraco.ShadowContentPicker" };

        public string GetValue(Property property, ICollection<ArtifactDependency> dependencies)
        {
            var value = property.Value as string;
            if (string.IsNullOrWhiteSpace(value) || value.DetectIsJson() == false)
                return null;

            var model = JsonConvert.DeserializeObject<ShadowContentPickerValue[]>(value);
            if (model == null)
                return null;

            var mockPropertyType = new PropertyType("Our.Umbraco.InnerContent", DataTypeDatabaseType.Ntext);

            foreach (var item in model)
            {
                if (item.Udi?.Guid.Equals(Guid.Empty) == false)
                {
                    var entity = _entityService.GetByKey(item.Udi.Guid, UmbracoObjectTypes.Document);
                    if (entity == null)
                        continue;

                    dependencies.Add(new ArtifactDependency(item.Udi, false, ArtifactDependencyMode.Exist));
                }

                if (item.Content != null && item.Content.IcContentTypeGuid.HasValue)
                {
                    var innerContentValue = JsonConvert.SerializeObject(item.Content.AsEnumerableOfOne());
                    var convertedValue = _innerContentValueConnector.GetValue(new Property(mockPropertyType, innerContentValue), dependencies);
                    item.Content = JsonConvert.DeserializeObject<InnerContentValue[]>(convertedValue).FirstOrDefault();
                }
            }

            return JsonConvert.SerializeObject(model);
        }

        public void SetValue(IContentBase content, string alias, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                content.SetValue(alias, value);
                return;
            }

            if (value.DetectIsJson() == false)
                return;

            var model = JsonConvert.DeserializeObject<ShadowContentPickerValue[]>(value);
            if (model == null)
                return;

            var mockPropertyAlias = "scpValue";
            var mockProperties = new PropertyCollection(new[]
            {
                new Property(new PropertyType("Our.Umbraco.InnerContent", DataTypeDatabaseType.Ntext, mockPropertyAlias))
            });
            var mockContent = new Content("ShadowContentPicker", -1, new ContentType(-1), mockProperties);

            foreach (var item in model)
            {
                if (item.Content != null && item.Content.IcContentTypeGuid.HasValue)
                {
                    var innerContentValue = JsonConvert.SerializeObject(item.Content.AsEnumerableOfOne());
                    _innerContentValueConnector.SetValue(mockContent, mockPropertyAlias, innerContentValue);

                    var convertedValue = mockContent.GetValue<string>(mockPropertyAlias);
                    item.Content = JsonConvert.DeserializeObject<InnerContentValue[]>(convertedValue).FirstOrDefault();
                }
            }

            content.SetValue(alias, JArray.FromObject(model).ToString(Formatting.None));
        }

        internal class InnerContentValueConnectorWrapped : InnerContentConnector
        {
            public InnerContentValueConnectorWrapped(
                IContentTypeService contentTypeService,
                Lazy<ValueConnectorCollection> valueConnectors)
                : base(contentTypeService, valueConnectors)
            { }
        }

        internal class ShadowContentPickerValue
        {
            [JsonProperty("udi")]
            public GuidUdi Udi { get; set; }

            [JsonProperty("content")]
            public InnerContentValue Content { get; set; }
        }
    }
}