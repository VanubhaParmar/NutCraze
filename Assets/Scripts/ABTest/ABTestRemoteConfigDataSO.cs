using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "ABTestRemoteConfigDataSO", menuName = Constant.GAME_NAME + "/Remote Config Data/ABTestRemoteConfigDataSO")]
    public class ABTestRemoteConfigDataSO : BaseConfig
    {
        #region PRIVATE_VARS
        [SerializeField] private ABTestSaveData abTestDefaultData = new ABTestSaveData();
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
            ABTestSaveData tempData = new ABTestSaveData();
            Dictionary<ABTestSystemType, ABTestType> abMapping = abTestDefaultData.abMapping;
            foreach (var item in abMapping)
            {
                tempData.abMapping.Add(item.Key, item.Value);
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
