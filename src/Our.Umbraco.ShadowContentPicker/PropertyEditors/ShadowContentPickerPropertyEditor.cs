using System.Collections.Generic;
using ClientDependency.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PropertyEditors;

namespace Our.Umbraco.ShadowContentPicker.PropertyEditors
{
    [PropertyEditor(PropertyEditorAlias, PropertyEditorName, PropertyEditorValueTypes.Json, PropertyEditorViewPath, Group = "lists", Icon = "icon-page-add")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, PropertyEditorJsPath)]
    public class ShadowContentPickerPropertyEditor : PropertyEditor
    {
        public const string PropertyEditorAlias = "Our.Umbraco.ShadowContentPicker";
        public const string PropertyEditorName = "Shadow Content Picker";
        public const string PropertyEditorViewPath = "~/App_Plugins/ShadowContentPicker/shadow-content-picker.html";
        public const string PropertyEditorJsPath = "~/App_Plugins/ShadowContentPicker/shadow-content-picker.js";

        public ShadowContentPickerPropertyEditor()
            : base()
        {
            DefaultPreValues = new Dictionary<string, object>
            {
                { "startNodeId", "-1" },
            };
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new ShadowContentPickerPreValueEditor();
        }

        protected override PropertyValueEditor CreateValueEditor()
        {
            return new ShadowContentPickerPropertyValueEditor(base.CreateValueEditor());
        }
    }
}