using GameAnalyticsSDK;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.NutSort
{
    public class LevelManager : SerializedManager<LevelManager>
    {
        #region PRIVATE_VARIABLES
        [SerializeField] private Dictionary<ABTestType, int> repeatLevelMapping = new Dictionary<ABTestType, int>();
        [SerializeField] private Dictionary<ABTestType, int> specialLevelIntervelMapping = new Dictionary<ABTestType, int>();

        [SerializeField] private LevelArrangementsListDataSO levelArrangementsListDataSO;
        [SerializeField] private Transform levelMainParent;
        [SerializeField] private Transform levelScrewsParent;
        [SerializeField] private Transform levelNutsParent;

        [Space]
        [ShowInInspector, ReadOnly] private LevelData currentLevelData;
        [ShowInInspector, ReadOnly] private LevelStage currentLevelStage;
        [ShowInInspector, ReadOnly] private List<BaseScrew> levelScrews = new List<BaseScrew>();
        [ShowInInspector] private ABTestType currentABType;

        private LevelSaveData saveData;
        private ColorThemeSO colorThemeSO;
        private const string RandomLevelGenerationSeedPrefsKey = "RandomLevelGenerationSeedPrefs";
        private const string LastGenerationSeedLevelNumberPrefsKey = "LastGenerationSeedLevelNumberPrefs";

        private List<Action> onLevelLoad = new List<Action>();
        private List<Action> onLevelComplete = new List<Action>();
        private List<Action> onLevelUnload = new List<Action>();
        private List<Action> onLevelReload = new List<Action>();

        #endregion

        #region PUBLIC_VARIABLES
        #endregion

        #region PROPERTIES
        private int RandomLevelsGenerationSeed
        {
            get { return PlayerPrefbsHelper.GetInt(RandomLevelGenerationSeedPrefsKey, Utility.GetNewRandomSeed()); }
            set { PlayerPrefbsHelper.SetInt(RandomLevelGenerationSeedPrefsKey, value); }
        }

        private int LastGenerationSeedLevelNumber
        {
            get { return PlayerPrefbsHelper.GetInt(LastGenerationSeedLevelNumberPrefsKey, 0); }
            set { PlayerPrefbsHelper.SetInt(LastGenerationSeedLevelNumberPrefsKey, value); }
        }

        private ColorThemeSO ColorThemeSO
        {
            get
            {
                if (colorThemeSO == null)
                    colorThemeSO = ResourceManager.Instance.GetColorTheme(currentABType);
                return colorThemeSO;
            }
        }

        public Transform LevelMainParent => levelMainParent;
        public LevelData CurrentLevelData => currentLevelData;
        public LevelStage CurrentLevelStage => currentLevelStage;
        public List<BaseScrew> LevelScrews => levelScrews;
        public ABTestType CurrentABType => currentABType;

        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            AssignABVariant();
            LoadSaveData();
        }
        #endregion

        #region PUBLIC_METHODS
        public void InitLevelFlow()
        {
            if (LevelProgressManager.Instance.IsLevelProgressDataExist)
            {
                if (LevelProgressManager.Instance.CurrentABTestType == this.currentABType)
                {
                    LoadLevelFromSaveData();
                }
                else
                {
                    LevelProgressManager.Instance.ResetLevelProgress();
                    StartNewLevelSequence();
                }
            }
            else
            {
                StartNewLevelSequence();
            }
        }

        private void StartNewLevelSequence()
        {
            int nextLevelNumber = DataManager.PlayerLevel;
            LevelType nextLevelType = LevelType.NORMAL_LEVEL;

            int levelToLoadNumber = GetLevelNumberToLoad(nextLevelNumber, nextLevelType);
            if (levelToLoadNumber <= 0)
                return;

            LevelData nextLevelData = ProtoLevelDataFactory.GetLevelData(currentABType, nextLevelType, levelToLoadNumber);

            if (nextLevelData == null)
            {
                return;
            }
            LevelSaveData initialSaveData = LevelProgressManager.Instance.CreateAndSaveInitialLevelProgress(nextLevelData, currentABType);
            LoadLevelFromSaveData();
        }

        private void LoadLevelFromSaveData()
        {
            LevelSaveData saveData = LevelProgressManager.Instance.LevelSaveData;
            if (saveData == null)
                return;

            InstantiateLevelFromSave(saveData, () =>
            {
                InvokeOnLevelLoad();
                VFXManager.Instance?.PlayLevelLoadAnimation();
            });
        }

        public void AdvanceToNextLevelStage()
        {
            if (!LevelProgressManager.Instance.IsLevelProgressDataExist)
            {
                Debug.LogError("Cannot advance stage - no level progress data.");
                return;
            }

            int currentStageIndex = LevelProgressManager.Instance.CurrentLevelStageIndex;
            int totalStages = LevelProgressManager.Instance.LevelSaveData?.pendingLevelStage?.Length ?? 0;

            if (currentStageIndex >= totalStages - 1)
            {
                Debug.LogError($"Tried to advance past the last stage ({currentStageIndex}). Should trigger level complete.");
                return;
            }

            Debug.Log($"Advancing from stage {currentStageIndex} to stage {currentStageIndex + 1}");

            LevelProgressManager.Instance.AdvanceLevelStage();

            InstantiateLevelFromSave(LevelProgressManager.Instance.LevelSaveData, () =>
            {
                // Play stage transition effect?
            });
        }

        public bool CanLoadSpecialLevel(out LevelData levelData)
        {
            levelData = null;
            if (!LevelProgressManager.Instance.IsLevelProgressDataExist)
                return false;

            int currentPlayerLevel = DataManager.PlayerLevel;
            int specialLevelNumber = GetSpecialLevelNumberToLoad(currentPlayerLevel);
            if (specialLevelNumber <= 0)
                return false;

            bool isPlayingThisSpecialLevel = LevelProgressManager.Instance.CurrentLevelType == LevelType.SPECIAL_LEVEL &&
                                          LevelProgressManager.Instance.CurrentLevel == specialLevelNumber;

            if (isPlayingThisSpecialLevel)
                return false;

            levelData = ProtoLevelDataFactory.GetLevelData(currentABType, LevelType.SPECIAL_LEVEL, specialLevelNumber);
            return levelData != null;
        }

        public int GetSpecialLevelNumberToLoad(int playerLevel)
        {
            if (!specialLevelIntervelMapping.ContainsKey(currentABType))
                return 0;
            int interval = specialLevelIntervelMapping[currentABType];
            if (interval <= 0)
                return 0;

            if (playerLevel > 1 && (playerLevel - 1) % interval == 0)
            {
                int specialLevelIndex = (playerLevel - 1) / interval;
                if (ProtoLevelDataFactory.GetTotalLevelCount(currentABType, LevelType.SPECIAL_LEVEL) >= specialLevelIndex)
                {
                    return specialLevelIndex;
                }
            }
            return 0;
        }

        private void InstantiateLevelFromSave(LevelSaveData saveData, Action onLoaded)
        {
            //RecycleAllLevelElements();

            //if (saveData == null || saveData.pendingLevelStage == null)
            //{
            //    onLoaded?.Invoke();
            //    return;
            //}

            //int stageIdx = saveData.currentStage;
            //if (stageIdx < 0 || stageIdx >= saveData.pendingLevelStage.Length)
            //{
            //    RecycleAllLevelElements();
            //    onLoaded?.Invoke();
            //    return;
            //}

            //LevelStageConfig stageSave = saveData.pendingLevelStage[stageIdx];
            //LevelArrangementConfigDataSO arrangementConfig = levelArrangementsListDataSO.GetLevelArrangementConfig(stageSave.arrangementId);

            //if (arrangementConfig == null)
            //{
            //    Debug.LogError($"Cannot instantiate stage {stageIdx}: Arrangement config ID {stageSave.arrangementId} not found.");
            //    RecycleAllLevelElements();
            //    onLoaded?.Invoke();
            //    return;
            //}

            //if (stageSave.screwConfigs != null)
            //{
            //    for (int i = 0; i < stageSave.screwConfigs.Length; i++)
            //    {
            //        ScrewConfig screwSave = stageSave.screwConfigs[i];
            //        GridCellId cellId = screwSave.cellId;
            //        Vector3 position = arrangementConfig.GetCellPosition(cellId);

            //        BaseScrew screw = ObjectPool.Instance.Spawn(ResourceManager.Instance.GetScrew(screwSave.screwType), levelScrewsParent);
            //        screw.transform.position = position;
            //        screw.gameObject.SetActive(true);
            //        screw.Init(screwSave);
            //        levelScrews.Add(screw);
            //    }
            //}
            //InvokeOnNewLevelStageLoad();
            //onLoaded?.Invoke();
        }

        private int GetLevelNumberToLoad(int targetPlayerLevel, LevelType levelType)
        {
            if (levelType == LevelType.SPECIAL_LEVEL)
            {
                return targetPlayerLevel;
            }

            int totalNormalLevels = ProtoLevelDataFactory.GetTotalLevelCount(currentABType, LevelType.NORMAL_LEVEL);
            int levelToLoad;
            if (targetPlayerLevel > totalNormalLevels)
            {
                if (repeatLevelMapping.TryGetValue(currentABType, out int repeatCount) && repeatCount > 0)
                {
                    levelToLoad = GetCappedLevel(targetPlayerLevel, totalNormalLevels, repeatCount);
                }
                else
                {
                    levelToLoad = totalNormalLevels;
                }
            }
            else
            {
                levelToLoad = targetPlayerLevel;
            }
            return levelToLoad;
        }



        private void CheckForLoadSavedLevelData()
        {
            if (currentABType != saveData.aBTestType)
            {
                CheckForStartNewLevel();
                return;
            }

            if (saveData.levelType == LevelType.SPECIAL_LEVEL)
            {
                //GameplayManager.Instance.LoadSpecailLevel(saveData);
            }
            else
            {
                //GameplayManager.Instance.LoadNormalLevel(saveData);
            }
        }

        private void CheckForStartNewLevel()
        {
        }

        public void LoadCurrentLevel(Action onLoad)
        {
            LoadCurrentLevelData();
            InstantiateCurrentLevel(onLoad);
        }

        public void LoadSpecialLevel(LevelData levelData, Action onLoad)
        {
            currentLevelData = levelData;
            InstantiateCurrentLevel(onLoad);
        }

        public void OnReloadCurrentLevel()
        {
            if (GameplayManager.Instance.IsPlayingLevel)
            {
                GameplayStateData gameplayStateData = GameplayManager.Instance.GameplayStateData;
                gameplayStateData.gameplayStateType = GameplayStateType.NONE;
                LevelProgressManager.Instance.ResetLevelProgress();

                if (CurrentLevelData.levelType == LevelType.SPECIAL_LEVEL)
                {
                    GameplayManager.Instance.LoadSpecailLevel(CurrentLevelData);
                    AnalyticsManager.Instance.LogSpecialLevelDataEvent(AnalyticsConstants.LevelData_RestartTrigger, CurrentLevelData.level);
                }
                else
                {
                    GameplayManager.Instance.LoadNormalLevel();
                    AnalyticsManager.Instance.LogLevelDataEvent(AnalyticsConstants.LevelData_RestartTrigger);
                    AnalyticsManager.Instance.LogProgressionEvent(GAProgressionStatus.Fail);
                }

                InvokeOnLevelReload();
            }
        }

        //public bool CanLoadSpecialLevel(out LevelData levelData)
        //{
        //    int currentPlayerLevel = DataManager.PlayerLevel;
        //    int specialLevelNumber = GetSpecialLevelNumberCountToLoad(currentPlayerLevel);
        //    bool isPlayingSpecialLevel = CurrentLevelData.levelType == LevelType.SPECIAL_LEVEL && CurrentLevelData.level == specialLevelNumber;
        //    levelData = LevelDataFactory.GetLevelData(currentABType, LevelType.SPECIAL_LEVEL, specialLevelNumber);
        //    return !isPlayingSpecialLevel && levelData != null;
        //}

        // Use this for Level Editor Purpose Only
        public void LoadLevel(LevelData levelDataSO, Action onLoad)
        {
            currentLevelData = levelDataSO;
            InstantiateCurrentLevel(onLoad);
        }

        public void InstantiateCurrentLevel(Action onLoad = null)
        {
            RecycleAllLevelElements();
            InstantiateCurrentLevelScrews();
            InstantiateCurrentLevelNuts();
            onLoad?.Invoke();
            InvokeOnLevelLoad();
            VFXManager.Instance.PlayLevelLoadAnimation(/*onLoad*/);
        }

        public BaseScrew GetScrewOfGridCell(GridCellId gridCellId)
        {
            return levelScrews.Find(x => x.CellId == gridCellId);
        }

        public BaseScrew GetScrew(int screwId)
        {
            return levelScrews.Find(x => x.Id == screwId);
        }

        public LevelArrangementConfigDataSO GetCurrentLevelArrangementConfig()
        {
            return null;
           // return levelArrangementsListDataSO.GetLevelArrangementConfig(currentLevelStage.arrangementId);
        }

        public ColorThemeConfig GetNutTheme(int nutColorId)
        {
            return ColorThemeSO.GetColorTheme(nutColorId);
        }

        public void UnLoadLevel()
        {
            RecycleAllLevelElements();
            InvokeOnLevelUnLoad();
        }

        public void OnLevelComplete()
        {
            GiveLevelCompleteReward();
            InvokeOnLevelComplete();
            DataManager.Instance.IncreasePlayerLevel();

            void GiveLevelCompleteReward()
            {
                BaseReward levelCompleteReward = GameManager.Instance.GameMainDataSO.levelCompleteReward;
                levelCompleteReward.GiveReward();
                if (levelCompleteReward.GetRewardId() == CurrencyConstant.COINS)
                    GameStatsCollector.Instance.OnGameCurrencyChanged(CurrencyConstant.COINS, levelCompleteReward.GetAmount(), GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_SYSTEM);
            }


        }

        public void RegisterOnLevelLoad(Action action)
        {
            if (!onLevelLoad.Contains(action))
                onLevelLoad.Add(action);
        }

        public void DeRegisterOnLevelLoad(Action action)
        {
            if (onLevelLoad.Contains(action))
                onLevelLoad.Remove(action);
        }

        public void RegisterOnLevelComplete(Action action)
        {
            if (!onLevelComplete.Contains(action))
                onLevelComplete.Add(action);
        }

        public void DeRegisterOnLevelComplete(Action action)
        {
            if (onLevelComplete.Contains(action))
                onLevelComplete.Remove(action);
        }

        public void RegisterOnLevelUnlod(Action action)
        {
            if (!onLevelUnload.Contains(action))
                onLevelUnload.Add(action);
        }

        public void DeRegisterOnLevelUnload(Action action)
        {
            if (onLevelUnload.Contains(action))
                onLevelUnload.Remove(action);
        }

        public void RegisterOnLevelReload(Action action)
        {
            if (!onLevelReload.Contains(action))
                onLevelReload.Add(action);
        }

        public void DeRegisterOnLevelReload(Action action)
        {
            if (onLevelReload.Contains(action))
                onLevelReload.Remove(action);
        }
        #endregion

        #region PRIVATE_METHODS
        private void LoadCurrentLevelData()
        {
            int currentLevel = DataManager.PlayerLevel;
            int totalLevel = ProtoLevelDataFactory.GetTotalLevelCount(currentABType, LevelType.NORMAL_LEVEL);

            if (currentLevel > totalLevel)
            {
                if (repeatLevelMapping.TryGetValue(currentABType, out int repeatLastLevelsCountAfterGameFinish))
                    currentLevel = GetCappedLevel(currentLevel, totalLevel, repeatLastLevelsCountAfterGameFinish);
                else
                    currentLevel = 0;
            }
            currentLevelData = ProtoLevelDataFactory.GetLevelData(currentABType, LevelType.NORMAL_LEVEL, currentLevel);
        }

        public int GetSpecialLevelNumberCountToLoad(int currentLevel)
        {
            if (!specialLevelIntervelMapping.ContainsKey(currentABType))
            {
                return 0;
            }
            int specialLevelInterval = specialLevelIntervelMapping[currentABType];

            if (CanLoadSpecialLevel(currentLevel, specialLevelInterval))
                return Mathf.FloorToInt((currentLevel - 1) / specialLevelInterval);
            return 0;

            bool CanLoadSpecialLevel(int currentLevel, int specialLevelInterval)
            {
                return (currentLevel - 1) % specialLevelInterval == 0 && currentLevel > 1;
            }
        }

        private void InvokeOnLevelLoad()
        {
            for (int i = 0; i < onLevelLoad.Count; i++)
                onLevelLoad[i]?.Invoke();
        }

        private void InvokeOnLevelComplete()
        {
            for (int i = 0; i < onLevelComplete.Count; i++)
                onLevelComplete[i]?.Invoke();
        }

        private void InvokeOnLevelUnLoad()
        {
            for (int i = 0; i < onLevelUnload.Count; i++)
                onLevelUnload[i]?.Invoke();
        }

        private void InvokeOnLevelReload()
        {
            for (int i = 0; i < onLevelReload.Count; i++)
                onLevelReload[i]?.Invoke();
        }

        private void AssignABVariant()
        {
            StartCoroutine(WaitForABTestManagerToInitilize(() =>
            {
                ABTestType aBTestType = ABTestManager.Instance.GetAbTestType(ABTestSystemType.Level);
                Debug.Log("AssignABVariant 0");
                if (!ProtoLevelDataFactory.IsABTestTypeExist(aBTestType))
                {
                    Debug.Log("AssignABVariant 1 ");
                    List<ABTestType> availableVariants = ProtoLevelDataFactory.GetAvailableABTestTypes();
                    ABTestManager.Instance.UpdateNewABTestType(ABTestSystemType.Level, availableVariants, out aBTestType);
                }
                currentABType = aBTestType;
                Debug.Log("AssignABVariant 2 " + currentABType);
                OnLoadingDone();
            }));
        }



        public int GetCappedLevel(int currentLevel, int totalLevels, int repeatLastLevelsCountAfterGameFinish)
        {
            if (currentLevel > totalLevels)
            {
                int index = (currentLevel - totalLevels) % repeatLastLevelsCountAfterGameFinish;

                if ((index == 0 && LastGenerationSeedLevelNumber != currentLevel) || LastGenerationSeedLevelNumber == 0)
                {
                    RandomLevelsGenerationSeed = Utility.GetNewRandomSeed();
                    LastGenerationSeedLevelNumber = currentLevel;
                    Debug.Log("<color=red>Set New Seed : " + RandomLevelsGenerationSeed + " " + LastGenerationSeedLevelNumber + "</color>");
                }

                return GetCappedRandomLevel(index, totalLevels, repeatLastLevelsCountAfterGameFinish);
            }

            return currentLevel;
        }

        private int GetCappedRandomLevel(int index, int totalLevels, int repeatLastLevelsCountAfterGameFinish)
        {
            int randomSeed = RandomLevelsGenerationSeed;

            Debug.Log("<color=red>Set Seed : " + randomSeed + "</color>");
            UnityEngine.Random.InitState(randomSeed);

            List<int> levels = Enumerable.Range(totalLevels - repeatLastLevelsCountAfterGameFinish + 1, repeatLastLevelsCountAfterGameFinish).ToList();
            levels.Shuffle();
            int randomLevel = index >= 0 && index < levels.Count ? levels[index] : levels.GetRandomItemFromList();

            UnityEngine.Random.InitState(Utility.GetNewRandomSeed());
            return randomLevel;
        }

        private void InstantiateCurrentLevelScrews()
        {
            //LevelArrangementConfigDataSO levelArrangementConfigDataSO = GetCurrentLevelArrangementConfig();
            //for (int i = 0; i < currentLevelData.stages[0].screwDatas.Length; i++)
            //{
            //    if (i >= levelArrangementConfigDataSO.arrangementCellIds.Count)
            //    {
            //        Debug.LogError("No Position Arrangement Data Found ! Please Check Arrangement !");
            //        break;
            //    }

            //    BaseScrew myScrew = ObjectPool.Instance.Spawn(ResourceManager.Instance.GetScrew(currentLevelData.levelScrewDataInfos[i].screwType), levelScrewsParent);
            //    GridCellId myGridCellId = levelArrangementConfigDataSO.arrangementCellIds[i];

            //    myScrew.transform.position = levelArrangementConfigDataSO.GetCellPosition(myGridCellId);
            //    myScrew.gameObject.SetActive(true);

            //    myScrew.InitScrew(myGridCellId, currentLevelData.levelScrewDataInfos[i]);

            //    levelScrews.Add(myScrew);
            //}
        }

        private void InstantiateCurrentLevelNuts()
        {
            //for (int i = 0; i < currentLevelData.screwNutsLevelDataInfos.Count; i++)
            //{
            //    GridCellId screwId = currentLevelData.screwNutsLevelDataInfos[i].targetScrewGridCellId;
            //    BaseScrew targetScrew = GetScrewOfGridCell(screwId);

            //    for (int j = currentLevelData.screwNutsLevelDataInfos[i].levelNutDataInfos.Count - 1; j >= 0; j--) // Reverse loop for setting nuts in screw
            //    {
            //        BaseNutLevelDataInfo nutScrewData = currentLevelData.screwNutsLevelDataInfos[i].levelNutDataInfos[j];
            //        BaseNut myNut = ObjectPool.Instance.Spawn(ResourceManager.Instance.GetNut(nutScrewData.nutType), levelNutsParent);

            //        myNut.gameObject.SetActive(true);
            //        myNut.InitNut(nutScrewData);

            //        levelNuts.Add(myNut);
            //        targetScrew.AddNut(myNut);
            //    }
            //}
        }

        private void LoadSaveData()
        {
            saveData = PlayerPersistantData.GetLevelSaveData();
        }

        private void SaveData()
        {
            PlayerPersistantData.SetLevelSaveData(saveData);
        }

        private void RecycleAllLevelElements()
        {
            levelScrews.ForEach(x => x.Recycle());
            levelScrews.Clear();
        }

        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        private IEnumerator WaitAndStartNextLevel(float delay = 0.5f) // Example delay
        {
            yield return new WaitForSeconds(delay);
            StartNewLevelSequence();
        }
        private IEnumerator WaitForABTestManagerToInitilize(Action onInitialize)
        {
            WaitUntil waitUntil = new WaitUntil(() => ABTestManager.Instance.IsInitialized);
            yield return waitUntil;
            onInitialize.Invoke();
        }
        #endregion

        #region UI_CALLBACKS
        #endregion
    }

}