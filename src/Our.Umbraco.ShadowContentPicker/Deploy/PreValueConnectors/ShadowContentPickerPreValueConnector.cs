using System.Collections.Generic;
using Umbraco.Core.Services;
using Umbraco.Deploy.PreValueConnectors;

namespace Umbraco.Deploy.Contrib.Connectors.PreValueConnectors
{
    public class ShadowContentPickerPreValueConnector : ContentPickerPreValueConnector
    {
        public ShadowContentPickerPreValueConnector(IEntityService entityService)
            : base(entityService)
        { }

        public override IEnumerable<string> PropertyEditorAliases => new[] { "Our.Umbraco.ShadowContentPicker" };
    }
}