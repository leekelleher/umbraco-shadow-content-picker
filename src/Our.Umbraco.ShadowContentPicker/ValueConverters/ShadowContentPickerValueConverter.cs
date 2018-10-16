using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Our.Umbraco.InnerContent.Helpers;
using Our.Umbraco.InnerContent.ValueConverters;
using Our.Umbraco.ShadowContentPicker.Models;
using Our.Umbraco.ShadowContentPicker.PropertyEditors;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web;

namespace Our.Umbraco.ShadowContentPicker.ValueConverters
{
    public class ShadowContentPickerValueConverter : InnerContentValueConverter, IPropertyValueConverterMeta
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.InvariantEquals(ShadowContentPickerPropertyEditor.PropertyEditorAlias);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            var value = source?.ToString();
            if (value == null || string.IsNullOrWhiteSpace(value))
                return null;

            // TODO: We could potentially do a check here, in case it's been hot-swapped with a ContentPicker or MNTP? [LK]
            if (value.DetectIsJson() == false)
            {
                // assume the value is legacy ContentPicker or MNTP
                // let's explode the CSV, then implode to serialized string of the model
                value = string.Concat("[{ \"udi\": \"", string.Join("\" }, { \"udi\": \"", value.ToDelimitedList()), "\" }]");
            }

            return JsonConvert.DeserializeObject<ShadowContentPickerDbModel[]>(value);
        }

        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (UmbracoContext.Current == null)
                return null;

            if (source is ShadowContentPickerDbModel[] model)
            {
                var items = new List<IPublishedContent>();

                for (int i = 0; i < model.Length; i++)
                {
                    var item = model[i];
                    if (item.Udi == null || item.Udi.Guid.Equals(Guid.Empty))
                        continue;

                    var fallback = UmbracoContext.Current.ContentCache.GetById(item.Udi.Guid);
                    if (fallback == null)
                        continue;

                    // If there is no shadowed content, then we can use the master/fallback directly.
                    if (item.Content == null || item.Content.HasValues == false)
                    {
                        items.Add(fallback);
                        continue;
                    }

                    var content = InnerContentHelper.ConvertInnerContentToPublishedContent(item.Content, null, i, 1, preview);

                    // Let the current model factory create a typed model to wrap our model
                    var shadow = PublishedContentModelFactoryResolver.HasCurrent && PublishedContentModelFactoryResolver.Current.HasValue
                        ? PublishedContentModelFactoryResolver.Current.Factory.CreateModel(new ShadowPublishedContent(content, fallback))
                        : new ShadowPublishedContent(content, fallback);

                    items.Add(shadow);
                }

                return items;
            }

            return base.ConvertSourceToObject(propertyType, source, preview);
        }

        public override object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview)
        {
            // TODO: For the XPath querying. (Don't worry about this just yet) [LK]
            return base.ConvertSourceToXPath(propertyType, source, preview);
        }

        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof(IEnumerable<IPublishedContent>);
        }

        public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType, PropertyCacheValue cacheValue)
        {
            return PropertyCacheLevel.Content;
        }
    }
}