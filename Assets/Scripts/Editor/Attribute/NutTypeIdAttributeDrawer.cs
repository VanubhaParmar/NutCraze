using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tag.NutSort.Editor
{
    public class NutTypeIdAttributeDrawer : BaseIdAttributesDrawer<NutTypeIdAttribute>
    {
        protected override void Initialize()
        {
            itemList = AssetDatabase.LoadAssetAtPath<BaseIDMappingConfig>(EditorCosntant.MAPPING_IDS_PATH + "/NutTypeIdMappings.asset");
            values = new List<string>();
            names = new List<string>();
        }
    }
}