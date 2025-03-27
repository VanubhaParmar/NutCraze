using System.Collections.Generic;
using UnityEditor;

namespace Tag.NutSort.Editor
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
