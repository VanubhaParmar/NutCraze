using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class DataManager : SerializedManager<DataManager>
    {
        #region private veriables
        [SerializeField] private PlayerPersistantDefaultDataHandler _playerPersistantDefaultDataHandler;
        private MainPlayerProgressData playerData;
        #endregion

        #region public static
        #endregion

        #region propertices
        public static int PlayerLevel => Instance.playerData.playerGameplayLevel;
        #endregion

        #region Unity_callback

        public override void Awake()
        {
            base.Awake();
            PlayerPrefbsHelper.SaveData = true;
            _playerPersistantDefaultDataHandler.CheckForDefaultDataAssignment();
            LoadSaveData();
            OnLoadingDone();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            OnCurrencyUnload();
        }

        public Currency GetCurrency(int currencyID)
        {
            var currencyDict = PlayerPersistantData.GetCurrancyDictionary();
            if (currencyDict.ContainsKey(currencyID))
                return currencyDict[currencyID];
            return null;
        }

        public float GetDefaultCurrencyAmount(int currencyId)
        {
            return _playerPersistantDefaultDataHandler.GetDefaultCurrencyAmount(currencyId);
        }

        public bool CanUseBooster(int boosterId)
        {
            switch (boosterId)
            {
                case BoosterIdConstant.UNDO:
                    return playerData.undoBoostersCount > 0;
                case BoosterIdConstant.EXTRA_SCREW:
                    return playerData.extraScrewBoostersCount > 0;
                default:
                    return false;
            }
        }

        public void IncreasePlayerLevel()
        {
            playerData.playerGameplayLevel++;
            SaveData();
        }

        public void SetplayerLevel(int level)
        {
            playerData.playerGameplayLevel = level;
            SaveData();
        }

        public void UseBooster(int boosterId)
        {
            switch (boosterId)
            {
                case BoosterIdConstant.UNDO:
                    playerData.undoBoostersCount = Mathf.Max(playerData.undoBoostersCount - 1, 0);
                    break;

                case BoosterIdConstant.EXTRA_SCREW:
                    playerData.extraScrewBoostersCount = Mathf.Max(playerData.extraScrewBoostersCount - 1, 0);
                    break;
            }
            SaveData();
        }

        public void AddBoosters(int boosterType, int boostersCount)
        {
            switch (boosterType)
            {
                case BoosterIdConstant.UNDO:
                    playerData.undoBoostersCount += boostersCount;
                    break;

                case BoosterIdConstant.EXTRA_SCREW:
                    playerData.extraScrewBoostersCount += boostersCount;
                    break;
            }
            SaveData();
        }

        public int GetBoostersCount(int boosterType)
        {
            switch (boosterType)
            {
                case BoosterIdConstant.UNDO:
                    return playerData.undoBoostersCount;

                case BoosterIdConstant.EXTRA_SCREW:
                    return playerData.extraScrewBoostersCount;
            }
            return 0;
        }

        public void OnPurchaseNoAdsPack(List<BaseReward> extraRewards = null)
        {
            if (extraRewards != null)
                extraRewards.ForEach(x => x.GiveReward());

            playerData.noAdsPurchaseState = true;
            SaveData();
            RaiseOnNoAdsPackPurchased();
        }

        public bool IsNoAdsPackPurchased()
        {
            return playerData.noAdsPurchaseState;
        }
        #endregion

        #region private Methods
        private void LoadSaveData()
        {
            playerData = PlayerPersistantData.GetMainPlayerProgressData();
        }

        private void SaveData()
        {
            PlayerPersistantData.SetMainPlayerProgressData(playerData);
        }

        private void OnCurrencyUnload()
        {
            var currencyDict = PlayerPersistantData.GetCurrancyDictionary();
            foreach (var item in currencyDict)
                item.Value.OnDestroy();
        }

        #endregion

        #region EVENT_HANDLERS
        public delegate void OnNoAdsPackEvent();
        public static event OnNoAdsPackEvent onNoAdsPackPurchased;
        public static void RaiseOnNoAdsPackPurchased()
        {
            if (onNoAdsPackPurchased != null)
                onNoAdsPackPurchased();
        }
        #endregion

        #region public methods

        #endregion

        #region UNITY_EDITOR_FUNCTIONS
#if UNITY_EDITOR
        [Button]
        public void Editor_PrintPlayerPersistantData()
        {
            var allPlayerData = PlayerPersistantData.GetPlayerPrefsData();
            foreach (var keyValuePair in allPlayerData)
            {
                Debug.Log(string.Format("{0} - {1}", keyValuePair.Key, keyValuePair.Value));
            }
        }
#endif
        #endregion
    }
}