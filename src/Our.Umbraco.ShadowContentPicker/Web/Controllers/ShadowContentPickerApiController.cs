using System;
using System.Web.Http;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Our.Umbraco.ShadowContentPicker.Web.Controllers
{
    [PluginController("ShadowContentPicker")]
    public class ShadowContentPickerApiController : UmbracoAuthorizedJsonController
    {
        [HttpGet]
        public object GetContentTypeGuidByAlias([FromUri] string alias)
        {
            return new { key = Services.ContentTypeService.GetContentType(alias)?.Key ?? Guid.Empty };
        }
    }
}