using System.Collections.Generic;
using Tag.NutSort.Editor;
using UnityEditor;

namespace Tag.NutSort
{
    public class BoosterIdAttributeDrawer : BaseIdAttributesDrawer<BoosterIdAttribute>
    {
        protected override void Initialize()
        {
            itemList = AssetDatabase.LoadAssetAtPath<BaseIDMappingConfig>(EditorConstant.MAPPING_IDS_PATH + "/BoosterIdMappings.asset");
            values = new List<string>();
            names = new List<string>();
        }
    }
}
