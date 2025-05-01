using System.Collections.Generic;
using UnityEditor;

namespace Tag.NutSort.Editor
{
    public class ScrewArrangementIdDrawer : BaseIdAttributesDrawer<ScrewArrangementIdAttribute>
    {
        protected override void Initialize()
        {
            itemList = AssetDatabase.LoadAssetAtPath<BaseIDMappingConfig>(EditorConstant.MAPPING_IDS_PATH + "/ScrewArrangementIdMapping.asset");
            values = new List<string>();
            names = new List<string>();
        }
    }
}
