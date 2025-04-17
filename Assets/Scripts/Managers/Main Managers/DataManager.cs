using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class DataManager : SerializedManager<DataManager>
    {
        #region private veriables
        [SerializeField] private List<CurrencyMappingData> allCurrency = new List<CurrencyMappingData>();
        [SerializeField] private MainPlayerProgressData defaultPlayerData = new MainPlayerProgressData();
        private Dictionary<int, Currency> currencyMapping = new Dictionary<int, Currency>();
        private MainPlayerProgressData playerData;
        private const string BUNDLE_VERSION_CODE_PREF_KEY = "BundleVersionCodePrefKey";
        #endregion

        #region public static
        #endregion

        #region propertices
        public static int PlayerLevel => Instance.playerData.playerGameplayLevel;
        private int CurrentBundleVersionCode
        {
            get => PlayerPrefbsHelper.GetInt(BUNDLE_VERSION_CODE_PREF_KEY, 0);
            set => PlayerPrefbsHelper.SetInt(BUNDLE_VERSION_CODE_PREF_KEY, value);
        }
        #endregion

        #region Unity_callback

        public override void Awake()
        {
            base.Awake();
            PlayerPrefbsHelper.SaveData = true;
            MapCurrency();
            CurrencyInit();
            LoadSaveData();
            if (CurrentBundleVersionCode != VersionCodeFetcher.GetBundleVersionCode())
            {
                Debug.LogError("Reset Level Data Due To New Build Chanages");
                ResetLevelProgressData();
            }
            CurrentBundleVersionCode = VersionCodeFetcher.GetBundleVersionCode();
            OnLoadingDone();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            OnCurrencyUnload();
        }

        public Currency GetCurrency(int currencyID)
        {
            if (currencyMapping.ContainsKey(currencyID))
                return currencyMapping[currencyID];
            return null;
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

        public void ResetLevelProgressData()
        {
            PlayerPersistantData.SetPlayerLevelProgressData(null);
        }

        public Dictionary<string, string> GetAllDataForServer()
        {
            Dictionary<string, string> dataDictionary = PlayerPersistantData.GetAllDataForServer();
            dataDictionary.Add(PlayerPrefsKeys.Currancy_Data_Key, SerializeUtility.SerializeObject(GetAllCurrencyDataForServer()));
            return dataDictionary;
        }

        public void SetServerData(Dictionary<string, string> serverData)
        {
            if (serverData.TryGetValue(PlayerPrefsKeys.Currancy_Data_Key, out string currencyDataJson))
            {
                Dictionary<string, string> currencyData = SerializeUtility.DeserializeObject<Dictionary<string, string>>(currencyDataJson);
                SetPlayerPersistantCurrancyData(currencyData);
            }
            PlayerPersistantData.SetServerData(serverData);
        }
        #endregion

        #region private Methods
        private void MapCurrency()
        {
            currencyMapping.Clear();
            for (int i = 0; i < allCurrency.Count; i++)
            {
                currencyMapping.Add(allCurrency[i].currencyID, allCurrency[i].currency);
            }
        }

        private void CurrencyInit()
        {
            foreach (var item in currencyMapping)
                item.Value.Init();

        }
        private void LoadSaveData()
        {
            playerData = PlayerPersistantData.GetMainPlayerProgressData();
            if (playerData == null)
                playerData = defaultPlayerData;
            SaveData();
        }

        private void SaveData()
        {
            PlayerPersistantData.SetMainPlayerProgressData(playerData);
        }

        private void OnCurrencyUnload()
        {
            foreach (var item in currencyMapping)
                item.Value.OnDestroy();
        }

        private Dictionary<string, string> GetAllCurrencyDataForServer()
        {
            Dictionary<string, string> dataDictionary = new Dictionary<string, string>();
            foreach (var pair in currencyMapping)
            {
                dataDictionary.Add(pair.Value.key, pair.Value.Value.ToString());
            }
            return dataDictionary;
        }

        private void SetPlayerPersistantCurrancyData(Dictionary<string, string> currancyData)
        {
            foreach (var pair in currancyData)
            {
                foreach (var values in currencyMapping.Values)
                {
                    if (values.key == pair.Key && int.TryParse(pair.Value, out int currancyVal))
                    {
                        values.SetValue(currancyVal);
                        break;
                    }
                }
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
#endif
        #endregion
    }
    public class CurrencyMappingData
    {
        [CurrencyId] public int currencyID;
        public Currency currency;
    }
}