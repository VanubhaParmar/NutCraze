using System.Collections.Generic;
using Tag.NutSort.Editor;
using UnityEditor;

namespace Tag.NutSort
{
    public class CurrencyIdAttributesDrawer : BaseIdAttributesDrawer<CurrencyIdAttribute>
    {
        protected override void Initialize()
        {
            itemList = AssetDatabase.LoadAssetAtPath<BaseIDMappingConfig>(EditorConstant.MAPPING_IDS_PATH + "/CurrencyIdMappings.asset");
            values = new List<string>();
            names = new List<string>();
        }
    }
}