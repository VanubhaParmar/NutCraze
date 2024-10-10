using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tag.NutSort.Editor;
using UnityEditor;

namespace Tag.NutSort
{
    public class CurrencyIdAttributesDrawer : BaseIdAttributesDrawer<CurrencyIdAttribute>
    {
        protected override void Initialize()
        {
            itemList = AssetDatabase.LoadAssetAtPath<BaseIDMappingConfig>(EditorCosntant.MAPPING_IDS_PATH + "/CurrencyIdMappings.asset");
            values = new List<string>();
            names = new List<string>();
        }
    }
}