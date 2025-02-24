using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "ABTestRemoteConfigDataSO", menuName = Constant.GAME_NAME + "/Remote Config Data/ABTestRemoteConfigDataSO")]
    public class ABTestRemoteConfigDataSO : BaseConfig
    {
        #region PRIVATE_VARS
        [SerializeField] private Dictionary<ABTestSystemType, ABTestType> abTestDefaultData = new Dictionary<ABTestSystemType, ABTestType>();
        #endregion

        #region PUBLIC_VARS
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        public override string GetDefaultString()
        {
            Dictionary<ABTestSystemType, ABTestType> tempData = new Dictionary<ABTestSystemType, ABTestType>();
            Dictionary<ABTestSystemType, ABTestType> abMapping = abTestDefaultData;
            foreach (var item in abMapping)
            {
                tempData.Add(item.Key, item.Value);
            }
            return SerializeUtility.SerializeObject(tempData);
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region EDITOR
#if UNITY_EDITOR
#endif
        #endregion
    }
}
