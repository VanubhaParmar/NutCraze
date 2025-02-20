using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class ABTestManager : SerializedManager<ABTestManager>
    {
        #region PRIVATE_VARS
        [SerializeField] private ABTestRemoteConfigDataSO remoteData;
        private ABTestSaveData saveData;
        #endregion

        #region PUBLIC_VARS
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            LoadSaveData();
            OnLoadingDone();
        }
        #endregion

        #region PUBLIC_FUNCTIONS

        public ABTestType GetAbTestType(ABTestSystemType aBTestSystemType)
        {
            if (saveData.abMapping.ContainsKey(aBTestSystemType))
                return saveData.abMapping[aBTestSystemType];
            return ABTestType.Default;
        }

        public void UpdateNewABTestType(ABTestSystemType systemType, out ABTestType newAbTestType)
        {
            ABTestSaveData aBTestSaveData = remoteData.GetValue<ABTestSaveData>();
            if (aBTestSaveData == null)
                newAbTestType = ABTestType.Default;
            if (aBTestSaveData.abMapping.ContainsKey(systemType))
                newAbTestType = aBTestSaveData.abMapping[systemType];
            newAbTestType = ABTestType.Default;

            if (!saveData.abMapping.ContainsKey(systemType))
                saveData.abMapping[systemType] = newAbTestType;
            else
                saveData.abMapping.Add(systemType, newAbTestType);
            SaveData();
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private void LoadSaveData()
        {
            saveData = PlayerPersistantData.GetABTestSaveData();
            if (saveData == null)
            {
                ABTestSaveData aBTestSaveData = remoteData.GetValue<ABTestSaveData>();
                if (aBTestSaveData == null)
                    aBTestSaveData = new ABTestSaveData();
                saveData = aBTestSaveData;
                SaveData();
            }
        }

        private void SaveData()
        {
            if (saveData != null)
                PlayerPersistantData.SetABTestSaveData(saveData);
        }
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

    public enum ABTestSystemType
    {
        Level,
    }

    public enum ABTestType
    {
        Default,
        AB1,
    }

    //[Serializable]
    public class ABTestSaveData
    {
        public Dictionary<ABTestSystemType, ABTestType> abMapping = new Dictionary<ABTestSystemType, ABTestType>();
    }

}
