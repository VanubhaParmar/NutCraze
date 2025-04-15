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
            LoadRemoteData();
            OnLoadingDone();
        }
        #endregion

        #region PUBLIC_FUNCTIONS
        public ABTestType GetABTestType(ABTestSystemType aBTestSystemType)
        {
            if (saveData == null)
            {
                Debug.Log("GetAbTestType saveData is null");
                return ABTestType.Default;
            }

            if (saveData.abMapping.ContainsKey(aBTestSystemType))
            {
                Debug.Log("GetAbTestType " + saveData.abMapping[aBTestSystemType]);
                return saveData.abMapping[aBTestSystemType];
            }
            Debug.Log("Abtest type not exist" + ABTestType.Default);
            return ABTestType.Default;
        }

        public ABTestType GetDefaultABTestType(ABTestSystemType aBTestSystemType)
        {
            Dictionary<ABTestSystemType, ABTestType> abMapping = this.remoteData.GetDefaultValue<Dictionary<ABTestSystemType, ABTestType>>();

            if (abMapping.ContainsKey(aBTestSystemType))
                return abMapping[aBTestSystemType];
            return ABTestType.Default;
        }

        public void UpdateNewABTestType(ABTestSystemType systemType, out ABTestType newAbTestType)
        {
            Dictionary<ABTestSystemType, ABTestType> abMapping = this.remoteData.GetValue<Dictionary<ABTestSystemType, ABTestType>>();

            if (!abMapping.ContainsKey(systemType))
            {
                Debug.Log("UpdateNewABTestType0- " + ABTestType.Default);
                newAbTestType = ABTestType.Default;
            }
            else
            {
                newAbTestType = abMapping[systemType];
                Debug.Log("UpdateNewABTestType1- " + newAbTestType);
            }
            SetABTestType(systemType, newAbTestType);
        }

        public void UpdateNewABTestType(ABTestSystemType systemType, List<ABTestType> availableABTests, out ABTestType newAbTestType)
        {
            Dictionary<ABTestSystemType, ABTestType> abMapping = this.remoteData.GetValue<Dictionary<ABTestSystemType, ABTestType>>();

            if (!abMapping.ContainsKey(systemType))
            {
                Debug.Log("UpdateNewABTestType0- " + ABTestType.Default);
                newAbTestType = ABTestType.Default;
            }
            else
            {
                if (availableABTests.Contains(abMapping[systemType]))
                {
                    newAbTestType = abMapping[systemType];
                    Debug.Log("UpdateNewABTestType1- " + newAbTestType);
                }
                else
                {
                    newAbTestType = availableABTests[UnityEngine.Random.Range(0, availableABTests.Count)];
                    Debug.Log("UpdateNewABTestType2- " + newAbTestType);
                }
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
                saveData = new ABTestSaveData();
            SaveData();
        }

        private void LoadRemoteData()
        {
            StartCoroutine(WaitForRCToLoad(() =>
            {
                Dictionary<ABTestSystemType, ABTestType> remoteData = this.remoteData.GetValue<Dictionary<ABTestSystemType, ABTestType>>();
                OnABTestLoad(remoteData);
            }));
        }

        private void OnABTestLoad(Dictionary<ABTestSystemType, ABTestType> remoteData)
        {
            CheckForUpdateABTestSaveData(remoteData);
            IsInitialized = true;
        }

        private void CheckForUpdateABTestSaveData(Dictionary<ABTestSystemType, ABTestType> remoteData)
        {
            if (saveData == null)
                saveData = new ABTestSaveData();
            //here only update the new abtest data can not update current assigend variant
            foreach (var item in remoteData)
            {
                if (!saveData.abMapping.ContainsKey(item.Key))
                {
                    saveData.abMapping.Add(item.Key, item.Value);
                    Debug.Log($"<color=blue>Assigning Variant  {item.Key} {item.Value}</color>");
                }
            }
            SaveData();
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
        Default = 0,
        AB1 = 1,// Water Sort
        AB2 = 2,// Color Ball Sort
        AB3 = 3,// Removed levels (30,45,95,106) from Default Variant
        AB4 = 4,// Build_AB
    }

    [Serializable]
    public class ABTestSaveData
    {
        public Dictionary<ABTestSystemType, ABTestType> abMapping = new Dictionary<ABTestSystemType, ABTestType>();
    }

}
