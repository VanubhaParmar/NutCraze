using System.Collections.Generic;
using UnityEditor;

namespace Tag.NutSort.Editor
{
    public class LevelArrangementIdDrawer : BaseIdAttributesDrawer<LevelArrangementIdAttribute>
    {
        protected override void Initialize()
        {
            itemList = AssetDatabase.LoadAssetAtPath<BaseIDMappingConfig>(EditorConstant.MAPPING_IDS_PATH + "/LevelArrangementIdMapping.asset");
            values = new List<string>();
            names = new List<string>();
        }
    }
}
