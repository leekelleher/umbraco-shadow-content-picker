using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Our.Umbraco.ShadowContentPicker.Models
{
    public class ShadowPublishedContent : IPublishedContent
    {
        private readonly IPublishedContent _content;
        private readonly IPublishedContent _fallback;

        private bool _propertiesInitialized;

        private Dictionary<string, IPublishedProperty> _properties;

        public ShadowPublishedContent(IPublishedContent content, IPublishedContent fallback)
        {
            Mandate.ParameterNotNull(content, nameof(content));
            Mandate.ParameterNotNull(fallback, nameof(fallback));

            _content = content;
            _fallback = fallback;
        }

        public object this[string alias] => GetProperty(alias);

        public IEnumerable<IPublishedContent> Children => _content.Children.Any() ? _content.Children : _fallback.Children;

        public IEnumerable<IPublishedContent> ContentSet => _content.ContentSet.Any() ? _content.ContentSet : _fallback.ContentSet;

        public PublishedContentType ContentType => _content.ContentType ?? _fallback.ContentType;

        public DateTime CreateDate => _content.CreateDate != default(DateTime) ? _content.CreateDate : _fallback.CreateDate;

        public int CreatorId => _content.CreatorId != default(int) ? _content.CreatorId : _fallback.CreatorId;

        public string CreatorName => string.IsNullOrWhiteSpace(_content.CreatorName) == false ? _content.CreatorName : _fallback.CreatorName;

        public string DocumentTypeAlias => string.IsNullOrWhiteSpace(_content.DocumentTypeAlias) == false ? _content.DocumentTypeAlias : _fallback.DocumentTypeAlias;

        public int DocumentTypeId => _content.DocumentTypeId != default(int) ? _content.DocumentTypeId : _fallback.DocumentTypeId;

        public int Id => _content.Id != default(int) ? _content.Id : _fallback.Id;

        public bool IsDraft => _content != null ? _content.IsDraft : _fallback.IsDraft;

        public PublishedItemType ItemType => _content != null ? _content.ItemType : _fallback.ItemType;

        public int Level => _content.Level != default(int) ? _content.Level : _fallback.Level;

        public string Name => string.IsNullOrWhiteSpace(_content.Name) == false ? _content.Name : _fallback.Name;

        public IPublishedContent Parent => _content.Parent ?? _fallback.Parent;

        public string Path => string.IsNullOrWhiteSpace(_content.Path) == false ? _content.Path : _fallback.Path;

        public ICollection<IPublishedProperty> Properties { get; private set; }

        public int SortOrder => _content.SortOrder != default(int) ? _content.SortOrder : _fallback.SortOrder;

        public int TemplateId => _content.TemplateId != default(int) ? _content.TemplateId : _fallback.TemplateId;

        public DateTime UpdateDate => _content.UpdateDate != default(DateTime) ? _content.UpdateDate : _fallback.UpdateDate;

        public string Url => string.IsNullOrWhiteSpace(_content.Url) == false && _content.Url.Equals("#") == false ? _content.Url : _fallback.Url;

        public string UrlName => string.IsNullOrWhiteSpace(_content.UrlName) == false ? _content.UrlName : _fallback.UrlName;

        public Guid Version => _content.Version != default(Guid) ? _content.Version : _fallback.Version;

        public int WriterId => _content.WriterId != default(int) ? _content.WriterId : _fallback.WriterId;

        public string WriterName => string.IsNullOrWhiteSpace(_content.WriterName) == false ? _content.WriterName : _fallback.WriterName;

        public int GetIndex() => _content != null ? _content.GetIndex() : _fallback.GetIndex();

        public IPublishedProperty GetProperty(string alias) => GetProperty(alias, false);

        public IPublishedProperty GetProperty(string alias, bool recurse)
        {
            if (_propertiesInitialized == false)
            {
                InitializeProperties();
            }

            if (_properties.TryGetValue(alias, out IPublishedProperty property) == false && recurse && Parent != null)
            {
                return Parent.GetProperty(alias, true);
            }

            return property;
        }

        private void InitializeProperties()
        {
            bool isNullOrEmpty(object value)
            {
                if (value == null)
                    return true;

                var empties = new[] { null, string.Empty, "[]", "{}" };
                return empties.Contains(value.ToString());
            };

            _properties = new Dictionary<string, IPublishedProperty>(StringComparer.OrdinalIgnoreCase);

            foreach (var prop in _content.Properties)
            {
                if (prop != null && prop.HasValue == true && isNullOrEmpty(prop.DataValue) == false && _properties.ContainsKey(prop.PropertyTypeAlias) == false)
                {
                    _properties.Add(prop.PropertyTypeAlias, prop);
                }
            }

            foreach (var prop in _fallback.Properties)
            {
                if (prop != null && prop.HasValue == true && isNullOrEmpty(prop.DataValue) == false && _properties.ContainsKey(prop.PropertyTypeAlias) == false)
                {
                    _properties.Add(prop.PropertyTypeAlias, prop);
                }
            }

            _propertiesInitialized = true;
        }
    }
}