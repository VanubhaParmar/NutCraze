using System.Collections.Generic;
using System.Linq;

namespace Tag.NutSort.Editor
{
    public class IAPProductIdAttributeDrawer : IdAttributesDrawer<IAPProductIdAttribute>
    {
        protected override List<string> GetIdList()
        {
            IAPManager iapManager = LevelEditorUtility.LoadAssetAtPath<IAPManager>(EditorConstant.IAP_Manager_Prefab_Path);
            return iapManager.IAPProducts.IAPProducts.Select(x => x.packId).ToList();
        }
    }
}