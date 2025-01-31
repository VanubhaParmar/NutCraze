using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.tag.nut_sort {
    public class DataManager : SerializedManager<DataManager>
    {
        #region PRIVATE_VARIABLES
        [SerializeField] private PlayerPersistantDefaultDataHandler _playerPersistantDefaultDataHandler;
        private const string FirstSessionStartTime_PrefsKey = "FirstSessioStartTimePrefsData";
        private const string InstallTime_PrefsKey = "InstallTimePrefsData";
        #endregion

        #region PUBLIC_VARIABLES

        public bool isFirstSession;

        #endregion


        #region PROPERTIES
        private string FirstSessionStartTime
        {
            get { return PlayerPrefbsHelper.GetString(FirstSessionStartTime_PrefsKey, TimeManager.Now.GetPlayerPrefsSaveString()); }
            set { PlayerPrefbsHelper.SetString(FirstSessionStartTime_PrefsKey, value); }
        }

        private string InstallTime
        {
            get { return PlayerPrefbsHelper.GetString(InstallTime_PrefsKey, Utility.GetUnixTimestamp().ToString()); }
            set { PlayerPrefbsHelper.SetString(InstallTime_PrefsKey, value); }
        }

        public DateTime FirstSessionStartDateTime
        {
            get
            {
                FirstSessionStartTime.TryParseDateTime(out DateTime firstSessionDT);
                return firstSessionDT;
            }
        }

        public long InstallUnixTime
        {
            get
            {
                if (long.TryParse(InstallTime, out long installTime))
                    return installTime;

                return Utility.GetUnixTimestamp();
            }
        }
        #endregion

        #region Unity_callback

        public override void Awake()
        {
            base.Awake();
            PlayerPrefbsHelper.SaveData = true;
            isFirstSession = PlayerPersistantData.GetMainPlayerProgressData() == null;
            if (isFirstSession)
            {
                FirstSessionStartTime = TimeManager.Now.GetPlayerPrefsSaveString();
                InstallTime = Utility.GetUnixTimestamp().ToString();
            }

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

        //public void TryToGetThisCurrency(int currencyID)
        //{
        //	if (currencyID == CurrencyConstant.GEMS && MainSceneUIManager.Instance != null)
        //		MainSceneUIManager.Instance.GetView<MainGemShopView>().ShowView();
        //	else if (currencyID == CurrencyConstant.COINS || currencyID == CurrencyConstant.STONE)
        //		GlobalUIManager.Instance.GetView<ToastMessageView>().ShowMessage("COMING SOON");
        //}

        public bool CanUseUndoBooster()
        {
            var playerData = PlayerPersistantData.GetMainPlayerProgressData();
            return playerData.undoBoostersCount > 0;
        }

        public void AddBoosters(BoosterType boosterType, int boostersCount)
        {
            var playerData = PlayerPersistantData.GetMainPlayerProgressData();

            switch (boosterType)
            {
                case BoosterType.UNDO:
                    playerData.undoBoostersCount += boostersCount;
                    break;

                case BoosterType.EXTRA_BOLT:
                    playerData.extraScrewBoostersCount += boostersCount;
                    break;
            }

            PlayerPersistantData.SetMainPlayerProgressData(playerData);
        }

        public int GetBoostersCount(BoosterType boosterType)
        {
            var playerData = PlayerPersistantData.GetMainPlayerProgressData();

            switch (boosterType)
            {
                case BoosterType.UNDO:
                    return playerData.undoBoostersCount;

                case BoosterType.EXTRA_BOLT:
                    return playerData.extraScrewBoostersCount;
            }

            return 0;
        }

        public bool CanUseExtraScrewBooster()
        {
            var playerData = PlayerPersistantData.GetMainPlayerProgressData();
            return playerData.extraScrewBoostersCount > 0;
        }

        public void OnPurchaseNoAdsPack(List<BaseReward> extraRewards = null)
        {
            if (extraRewards != null)
                extraRewards.ForEach(x => x.GiveReward());

            var playerData = PlayerPersistantData.GetMainPlayerProgressData();
            playerData.noAdsPurchaseState = true;
            PlayerPersistantData.SetMainPlayerProgressData(playerData);

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