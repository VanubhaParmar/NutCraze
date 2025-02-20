using System.Collections.Generic;
using Tag.NutSort;
using UnityEditor;

namespace com.tag.editor
{
    public class ScrewTypeIdAttributeDrawer : BaseIdAttributesDrawer<ScrewTypeIdAttribute>
    {
        protected override void Initialize()
        {
            itemList = AssetDatabase.LoadAssetAtPath<BaseIDMappingConfig>(EditorConstant.MAPPING_IDS_PATH + "/ScrewTypeIdMappings.asset");
            values = new List<string>();
            names = new List<string>();
        }
    }
}