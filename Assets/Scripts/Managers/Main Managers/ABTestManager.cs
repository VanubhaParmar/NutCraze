using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class ABTestManager : SerializedManager<ABTestManager>
    {
        #region PRIVATE_VARS
        [SerializeField] private ABTestRemoteConfigDataSO remoteData;
        [ShowInInspector] private ABTestSaveData saveData;
        #endregion

        #region PUBLIC_VARS
        #endregion

        #region PROPERTIES
        public bool IsInitialized { get; private set; }
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
            {
                Debug.Log("GetAbTestType " + saveData.abMapping[aBTestSystemType]);
                return saveData.abMapping[aBTestSystemType];
            }
            Debug.Log("Abtest type not exist" + ABTestType.Default);
            return ABTestType.Default;
        }

        public void UpdateNewABTestType(ABTestSystemType systemType, out ABTestType newAbTestType)
        {
            ABTestSaveData aBTestSaveData = remoteData.GetValue<ABTestSaveData>();

            if (aBTestSaveData == null || !aBTestSaveData.abMapping.ContainsKey(systemType))
            {
                Debug.Log("UpdateNewABTestType0- " + ABTestType.Default);
                newAbTestType = ABTestType.Default;
            }
            else
            {
                newAbTestType = aBTestSaveData.abMapping[systemType];
                Debug.Log("UpdateNewABTestType1- " + newAbTestType);
            }
            SetABTestType(systemType, newAbTestType);
        }

        public void UpdateNewABTestType(ABTestSystemType systemType, List<ABTestType> availableVariants, out ABTestType newAbTestType)
        {
            ABTestSaveData aBTestSaveData = remoteData.GetValue<ABTestSaveData>();

            if (aBTestSaveData != null && aBTestSaveData.abMapping.ContainsKey(systemType))
            {
                ABTestType savedType = aBTestSaveData.abMapping[systemType];

                if (availableVariants.Contains(savedType))
                {
                    newAbTestType = savedType;
                    Debug.Log($"Using saved ABTestType: {newAbTestType}");
                }
                else
                {
                    newAbTestType = ABTestType.Default;
                    Debug.Log($"Saved ABTestType {savedType} not available. Using: {newAbTestType}");
                }
            }
            else
            {
                newAbTestType = ABTestType.Default;
                Debug.Log($"No saved ABTestType. Using: {newAbTestType}");
            }

            SetABTestType(systemType, newAbTestType);
        }

        public void SetABTestType(ABTestSystemType systemType, ABTestType newAbTestType)
        {
            if (saveData.abMapping.ContainsKey(systemType))
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
                StartCoroutine(WaitForRCToLoad(() =>
                {
                    Dictionary<ABTestSystemType, ABTestType> remoteData = this.remoteData.GetValue<Dictionary<ABTestSystemType, ABTestType>>();
                    saveData = new ABTestSaveData() { abMapping = remoteData };
                    foreach (var item in saveData.abMapping)
                        Debug.Log("ABTestManager Savedata " + item.Key.ToString() + " " + item.Value.ToString());
                    SaveData();
                    IsInitialized = true;
                }));
            }
            else
            {
                IsInitialized = true;
            }
        }

        private void SaveData()
        {
            if (saveData != null)
                PlayerPersistantData.SetABTestSaveData(saveData);
        }
        #endregion

        #region COROUTINES
        private IEnumerator WaitForRCToLoad(Action actionToCall)
        {
            yield return new WaitUntil(() => GameAnalyticsManager.Instance.IsRCValuesFetched);
            actionToCall?.Invoke();
        }
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
        AB2,
        AB3,
    }

    [Serializable]
    public class ABTestSaveData
    {
        public Dictionary<ABTestSystemType, ABTestType> abMapping = new Dictionary<ABTestSystemType, ABTestType>();
    }

}
