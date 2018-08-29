using System.Collections.Generic;
using Our.Umbraco.InnerContent.PropertyEditors;
using Umbraco.Core.PropertyEditors;

namespace Our.Umbraco.ShadowContentPicker.PropertyEditors
{
    internal class ShadowContentPickerPreValueEditor : InnerContentPreValueEditor
    {
        public ShadowContentPickerPreValueEditor()
            : base()
        {
            Fields.Add(new PreValueField()
            {
                Key = "startNodeId",
                View = "treepicker",
                Name = "Start node",
                Config = new Dictionary<string, object>() { { "idType", "udi" } }
            });
        }
    }
}