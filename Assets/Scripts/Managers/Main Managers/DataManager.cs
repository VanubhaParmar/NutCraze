using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class DataManager : SerializedManager<DataManager>
    {
        #region private veriables
        [SerializeField] private PlayerPersistantDefaultDataHandler _playerPersistantDefaultDataHandler;
        #endregion

        #region public static
        #endregion

        #region propertices

        public static MainPlayerProgressData PlayerData
        {
            get
            {
                return PlayerPersistantData.GetMainPlayerProgressData();
            }

            set
            {
                Debug.Log("PlayerData " + (value == null));
                PlayerPersistantData.SetMainPlayerProgressData(value);
            }
        }

        #endregion

        #region Unity_callback

        public override void Awake()
        {
            base.Awake();
            PlayerPrefbsHelper.SaveData = true;
            _playerPersistantDefaultDataHandler.CheckForDefaultDataAssignment();
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

        public void SaveData(MainPlayerProgressData playerData)
        {
            PlayerData = playerData;
        }

        //public void TryToGetThisCurrency(int currencyID)
        //{
        //	if (currencyID == CurrencyConstant.GEMS && MainSceneUIManager.Instance != null)
        //		MainSceneUIManager.Instance.GetView<MainGemShopView>().ShowView();
        //	else if (currencyID == CurrencyConstant.COINS || currencyID == CurrencyConstant.STONE)
        //		GlobalUIManager.Instance.GetView<ToastMessageView>().ShowMessage("COMING SOON");
        //}

        public bool CanUseUndoBooster()
        {
            return PlayerData.undoBoostersCount > 0;
        }

        public void AddBoosters(int boosterType, int boostersCount)
        {
            var playerData = PlayerData;

            switch (boosterType)
            {
                case BoosterIdConstant.UNDO:
                    playerData.undoBoostersCount += boostersCount;
                    break;

                case BoosterIdConstant.EXTRA_SCREW:
                    playerData.extraScrewBoostersCount += boostersCount;
                    break;
            }
            PlayerData = playerData;
        }

        public int GetBoostersCount(int boosterType)
        {
            switch (boosterType)
            {
                case BoosterIdConstant.UNDO:
                    return PlayerData.undoBoostersCount;

                case BoosterIdConstant.EXTRA_SCREW:
                    return PlayerData.extraScrewBoostersCount;
            }

            return 0;
        }

        public bool CanUseExtraScrewBooster()
        {
            return PlayerData.extraScrewBoostersCount > 0;
        }

        public void OnPurchaseNoAdsPack(List<BaseReward> extraRewards = null)
        {
            if (extraRewards != null)
                extraRewards.ForEach(x => x.GiveReward());

            var playerData = PlayerData;
            playerData.noAdsPurchaseState = true;
            PlayerData = (playerData);

            RaiseOnNoAdsPackPurchased();
        }

        public bool CanPurchaseNoAdsPack()
        {
            return !IsNoAdsPackPurchased();
        }

        public bool IsNoAdsPackPurchased()
        {
            return PlayerPersistantData.GetMainPlayerProgressData().noAdsPurchaseState;
        }
        #endregion

        #region private Methods

        private void OnCurrencyUnload()
        {
            var currencyDict = PlayerPersistantData.GetCurrancyDictionary();
            foreach (var item in currencyDict)
            {
                item.Value.OnDestroy();
            }
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