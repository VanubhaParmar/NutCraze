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
        [SerializeField] private LevelABTestRemoteConfig levelABTestRemoteConfig;
        [SerializeField] private ScrewArrangementsDataSO screwArrangementsDataSO;

        [ShowInInspector, ReadOnly] private LevelDataSO currentLevelDataSO;
        private const string RandomLevelGenerationSeedPrefsKey = "RandomLevelGenerationSeedPrefs";
        private const string LastGenerationSeedLevelNumberPrefsKey = "LastGenerationSeedLevelNumberPrefs";
        private const string LEVEL_AB_TEST_TYPE_PREFKEY = "LevelABTestTypePrefsKey";

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

        [ShowInInspector]
        public LevelABTestType CurrentTestingType
        {
            get
            {
                if (!PlayerPrefbsHelper.HasKey(LEVEL_AB_TEST_TYPE_PREFKEY))
                {
                    int defaultValue = (int)GetDefaultLevelABTestType();
                    PlayerPrefbsHelper.SetInt(LEVEL_AB_TEST_TYPE_PREFKEY, defaultValue);
                }
                return (LevelABTestType)PlayerPrefbsHelper.GetInt(LEVEL_AB_TEST_TYPE_PREFKEY);
            }
            set
            {
                if (CurrentTestingType != value)
                    DataManager.Instance.ResetLevelProgressData();
                PlayerPrefbsHelper.SetInt(LEVEL_AB_TEST_TYPE_PREFKEY, (int)value);
            }
        }

        public LevelVariantSO CurrentVariant
        {
            get => ResourceManager.Instance.GetLevelVariant(CurrentTestingType);
        }
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            StartCoroutine(WaitForRCValuesFetched(() =>
            {
                Debug.Log($"levelABTestRemoteConfig.GetValue<int>() {levelABTestRemoteConfig.GetValue<int>()}  {(LevelABTestType)levelABTestRemoteConfig.GetValue<int>()}");
                CurrentTestingType = (LevelABTestType)levelABTestRemoteConfig.GetValue<int>();
                OnLoadingDone();
            }));
        }

        #endregion

        #region PUBLIC_METHODS
        public void LoadCurrentLevel(Action onLoad)
        {
            int currentLevel = DataManager.PlayerLevel;
            int totalLevel = CurrentVariant.GetNormalLevelCount();
            int repeatLastLevelsCountAfterGameFinish = CurrentVariant.RepeatLastLevelsCountAfterGameFinish;

            if (currentLevel > totalLevel)
                currentLevel = GetCappedLevel(currentLevel, totalLevel, repeatLastLevelsCountAfterGameFinish);
            Debug.Log($"LoadCurrentLevel: {currentLevel} {totalLevel} {repeatLastLevelsCountAfterGameFinish}");
            currentLevelDataSO = CurrentVariant.GetNormalLevel(currentLevel);
            LevelSaveData levelSaveData = LevelProgressManager.Instance.CreateAndSaveInitialLevelProgress(currentLevelDataSO);
            InstatiateLevel(levelSaveData, onLoad);
        }

        public void LoadSpecialLevel(int specialLevelNumber, Action onLoad)
        {
            currentLevelDataSO = CurrentVariant.GetSpecialLevel(specialLevelNumber);
            LevelSaveData levelSaveData = LevelProgressManager.Instance.CreateAndSaveInitialLevelProgress(currentLevelDataSO);
            InstatiateLevel(levelSaveData, onLoad);
        }

        public void LoadSavedLevel(Action onLoad)
        {
            LevelSaveData levelSaveData = LevelProgressManager.Instance.LevelSaveData;
            if (levelSaveData.levelType == LevelType.NORMAL_LEVEL)
                currentLevelDataSO = CurrentVariant.GetNormalLevel(levelSaveData.level);
            else
                currentLevelDataSO = CurrentVariant.GetSpecialLevel(levelSaveData.level);
            InstatiateLevel(levelSaveData, onLoad);
        }

        public void LoadLevel(LevelDataSO levelDataSO, Action onLoad)
        {
            currentLevelDataSO = levelDataSO;
            LevelSaveData levelSaveData = LevelProgressManager.Instance.CreateAndSaveInitialLevelProgress(currentLevelDataSO);
            InstatiateLevel(levelSaveData, onLoad);

        }

        public void OnLevelRestart()
        {
            InvokeOnLevelReload();
        }

        public bool HasSpecialLevel(int specialLevelNumber)
        {
            return CurrentVariant.HasSpecialLevel(specialLevelNumber);
        }

        public bool CanLoadSpecialLevel(int playerLevel)
        {
            return CurrentVariant.CanLoadSpecialLevel(playerLevel);
        }

        public ScrewArrangementConfigSO GetCurrentArrangementConfigSO(int arrangementId)
        {
            return screwArrangementsDataSO.GetScrewArrangementConfigSO(arrangementId);
        }

        public NutColorThemeInfo GetNutTheme(int nutColorId)
        {
            return CurrentVariant.GetNutTheme(nutColorId);
        }

        public void UnLoadLevel()
        {
            ScrewManager.Instance.RecycleAllElements();
            InvokeOnLevelUnLoad();
        }

        public void OnLevelComplete()
        {
            if (LevelProgressManager.Instance.CurrentLevelType == LevelType.NORMAL_LEVEL)
                DataManager.Instance.IncreasePlayerLevel();
            else
                DataManager.Instance.IncreasePlayerSpecialLevel();
            InvokeOnLevelComplete();
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
        private void InstatiateLevel(LevelSaveData saveData, Action onLoad)
        {
            if (saveData == null)
                return;
            ScrewManager.Instance.RecycleAllElements();
            ScrewManager.Instance.InitScrews(saveData);
            onLoad?.Invoke();
            ScrewManager.Instance.CheckForImmediateScrewSortCompletion();
            InvokeOnLevelLoad();
            VFXManager.Instance?.PlayLevelLoadAnimation();
        }

        private LevelABTestType GetDefaultLevelABTestType()
        {
            return (LevelABTestType)levelABTestRemoteConfig.GetDefaultValue<int>();
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
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        private IEnumerator WaitForRCValuesFetched(Action onComplete)
        {
#if UNITY_EDITOR
            yield return null;
            onComplete.Invoke();
#elif !UNITY_EDITOR
            yield return new WaitForGARemoteConfigToLoad();
            onComplete.Invoke();
#endif
        }
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region TESTING_METHODS
        [Button]
        public void StartAISolver()
        {
            LevelSolver.Instance.StartAISolver();
        }
        [Button]
        public void StopAISolver()
        {
            LevelSolver.Instance.StopAISolver();
        }
        #endregion
    }

    public enum LevelABTestType
    {
        Default = 0,
        WaterSort = 1,// Water Sort (AB1)
        ColorBallSort = 2,// Color Ball Sort (AB2)
        EasyLevels = 3,// Removed levels (30,45,95,106) from Default Variant (AB3)
        Build_AB = 4,// Build_AB (AB4)
    }
}