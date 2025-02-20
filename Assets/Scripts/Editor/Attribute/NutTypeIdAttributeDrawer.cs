using System.Collections.Generic;
using UnityEditor;

namespace Tag.NutSort.Editor
{
    public class NutTypeIdAttributeDrawer : BaseIdAttributesDrawer<NutTypeIdAttribute>
    {
        protected override void Initialize()
        {
            itemList = AssetDatabase.LoadAssetAtPath<BaseIDMappingConfig>(EditorConstant.MAPPING_IDS_PATH + "/NutTypeIdMappings.asset");
            values = new List<string>();
            names = new List<string>();
        }
    }
}