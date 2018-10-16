using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Our.Umbraco.InnerContent.PropertyEditors;
using Our.Umbraco.ShadowContentPicker.Models;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Our.Umbraco.ShadowContentPicker.PropertyEditors
{
    internal class ShadowContentPickerPropertyValueEditor : InnerContentPropertyValueEditor
    {
        public ShadowContentPickerPropertyValueEditor(PropertyValueEditor wrapped)
            : base(wrapped)
        { }

        public override object ConvertDbToEditor(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            var propertyValue = property?.Value?.ToString();
            if (string.IsNullOrWhiteSpace(propertyValue))
                return base.ConvertDbToEditor(property, propertyType, dataTypeService);

            // TODO: We could potentially do a check here, in case it's been hot-swapped with a ContentPicker or MNTP? [LK]
            if (propertyValue.DetectIsJson() == false)
            {
                // assume the value is legacy ContentPicker or MNTP
                // let's explode the CSV, then implode to serialized string of the model
                propertyValue = string.Concat("[{ \"udi\": \"", string.Join("\" }, { \"udi\": \"", propertyValue.ToDelimitedList()), "\" }]");
            }

            var model = JsonConvert.DeserializeObject<ShadowContentPickerEditorModel[]>(propertyValue);
            if (model == null)
                return base.ConvertDbToEditor(property, propertyType, dataTypeService);

            var guids = model.Where(x => x.Udi != null && x.Udi.Guid.Equals(Guid.Empty) == false).Select(x => x.Udi);
            if (guids.Any() == false)
                return base.ConvertDbToEditor(property, propertyType, dataTypeService);

            var nodes = ApplicationContext.Current.Services.ContentService.GetByIds(guids).ToList();
            if (nodes == null || nodes.Count == 0)
                return base.ConvertDbToEditor(property, propertyType, dataTypeService);

            bool isNullOrEmpty(JToken token)
            {
                return (token == null) ||
                       (token.Type == JTokenType.Array && token.HasValues == false) ||
                       (token.Type == JTokenType.Object && token.HasValues == false) ||
                       (token.Type == JTokenType.String && string.IsNullOrEmpty(token.ToString())) ||
                       (token.Type == JTokenType.Null);
            };

            foreach (var item in model)
            {
                var node = nodes.FirstOrDefault(x => x.Key.Equals(item.Udi.Guid));
                if (node == null)
                    continue;

                var overrides = new List<object>();

                if (item.Content != null && item.Content.HasValues)
                {
                    ConvertInnerContentDbToEditor(item.Content, dataTypeService);

                    var systemKeys = new[] { "key", "name", "icon", "icContentTypeGuid" };
                    foreach (var token in item.Content)
                    {
                        if (systemKeys.Contains(token.Key))
                            continue;

                        if (isNullOrEmpty(token.Value) == false)
                        {
                            var prop = node.ContentType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias.InvariantEquals(token.Key));
                            overrides.Add(new { name = prop != null ? prop.Name : token.Key });
                        }
                    }
                }

                item.ContentTypeGuid = node.ContentType.Key;
                item.Item = new ShadowContentPickerEditorModelItem
                {
                    Name = node.Name,
                    Id = node.Id,
                    Icon = node.ContentType.Icon,
                    Path = node.Path,
                    Trashed = node.Trashed,
                    Published = node.Published,
                    Overrides = overrides
                };
            }

            return model;
        }

        public override string ConvertDbToString(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            var propertyValue = property?.Value?.ToString();
            if (string.IsNullOrWhiteSpace(propertyValue))
                return base.ConvertDbToString(property, propertyType, dataTypeService);

            var model = JsonConvert.DeserializeObject<ShadowContentPickerDbModel[]>(propertyValue);
            if (model == null)
                return base.ConvertDbToString(property, propertyType, dataTypeService);

            foreach (var item in model)
            {
                if (item.Content != null && item.Content.HasValues)
                {
                    ConvertInnerContentDbToString(item.Content, dataTypeService);
                }
            }

            return JsonConvert.SerializeObject(model);
        }

        public override XNode ConvertDbToXml(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            // TODO: For the XML cache. (Don't worry about this just yet) [LK]
            return base.ConvertDbToXml(property, propertyType, dataTypeService);
        }

        public override object ConvertEditorToDb(ContentPropertyData editorValue, object currentValue)
        {
            var value = editorValue?.Value?.ToString();
            if (value == null || string.IsNullOrWhiteSpace(value))
                return base.ConvertEditorToDb(editorValue, currentValue);

            var model = JsonConvert.DeserializeObject<ShadowContentPickerDbModel[]>(value);
            if (model == null)
                return base.ConvertEditorToDb(editorValue, currentValue);

            foreach (var item in model)
            {
                ConvertInnerContentEditorToDb(item.Content, ApplicationContext.Current.Services.DataTypeService);
            }

            return JsonConvert.SerializeObject(model);
        }
    }
}