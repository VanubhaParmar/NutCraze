using Sirenix.OdinInspector;
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
            MapCurrency();
            CurrencyInit();
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

        [Button]
        public bool IsABTestTypeExist(ABTestType aBTestType)
        {
            return LevelDataFactory.IsABTestTypeExist(aBTestType);
        }


        [Button]
        public void AddLevelData()
        {
            List<LevelData> levelDatas = new List<LevelData>();
            for (int i = 1; i <= 1000; i++)
            {
                levelDatas.Add(MakeTestLevelData(LevelType.NORMAL_LEVEL, i));
                levelDatas.Add(MakeTestLevelData(LevelType.SPECIAL_LEVEL, i + 1));
            }
            for (int i = 0; i < levelDatas.Count; i++)
            {
                LevelDataFactory.SaveLevelData(ABTestType.Default, levelDatas[i]);
                LevelDataFactory.SaveLevelData(ABTestType.AB1, levelDatas[i]);
                LevelDataFactory.SaveLevelData(ABTestType.AB2, levelDatas[i]);
                LevelDataFactory.SaveLevelData(ABTestType.AB3, levelDatas[i]);
            }
            UnityEditor.AssetDatabase.Refresh();
        }

        [Button]
        public void EditLevelData(ABTestType aBTestType, LevelData levelData)
        {
            LevelDataFactory.SaveLevelData(aBTestType, levelData);
        }

        public LevelData MakeTestLevelData(LevelType levelType, int level)
        {
            LevelData levelData = new LevelData();
            levelData.level = level;
            levelData.levelType = levelType;
            levelData.stages = new LevelStage[1];
            levelData.stages[0] = new LevelStage();
            levelData.stages[0].arrangementId = 1;
            levelData.stages[0].screwDatas = new ScrewData[5];
            for (int i = 0; i < 100; i++)
            {
                levelData.stages[0].screwDatas[i] = new ScrewData();
                levelData.stages[0].screwDatas[i].id = 0;
                levelData.stages[0].screwDatas[i].screwType = 0;
                levelData.stages[0].screwDatas[i].capacity = 4;
                levelData.stages[0].screwDatas[i].screwStages = new ScrewStage[1];
                levelData.stages[0].screwDatas[i].screwStages[0] = new ScrewStage();
                levelData.stages[0].screwDatas[i].screwStages[0].nutDatas = new NutData[4];
                for (int j = 0; j < 4; j++)
                {
                    levelData.stages[0].screwDatas[i].screwStages[0].nutDatas[j] = new NutData();
                    levelData.stages[0].screwDatas[i].screwStages[0].nutDatas[j].nutType = 0;
                    levelData.stages[0].screwDatas[i].screwStages[0].nutDatas[j].nutColorTypeId = 0;
                }
            }
            return levelData;
        }

        [Button]
        public List<LevelData> GetLevelDatas(ABTestType aBTestType, LevelType levelType)
        {
            return LevelDataFactory.GetLevelsByType(aBTestType, levelType);
        }

        [Button]
        public LevelData GetLevelData(ABTestType aBTestType, LevelType levelType, int levelNumber)
        {
            return LevelDataFactory.GetLevelData(aBTestType, levelType, levelNumber);
        }

        [Button]
        public int GetTotalLevelCount(ABTestType aBTestType, LevelType levelType)
        {
            return LevelDataFactory.GetTotalLevelCount(aBTestType, levelType);
        }
#endif
        #endregion
    }

    public class CurrencyMappingData
    {
        [CurrencyId] public int currencyID;
        public Currency currency;
    }
}