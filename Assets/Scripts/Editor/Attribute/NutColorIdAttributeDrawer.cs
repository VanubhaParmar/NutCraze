using System.Collections.Generic;
using UnityEditor;

namespace Tag.NutSort.Editor
{
    public class NutColorIdAttributeDrawer : BaseIdAttributesDrawer<NutColorIdAttribute>
    {
        protected override void Initialize()
        {
            itemList = AssetDatabase.LoadAssetAtPath<BaseIDMappingConfig>(EditorConstant.MAPPING_IDS_PATH + "/NutColorIdMappings.asset");
            values = new List<string>();
            names = new List<string>();
        }
    }
}