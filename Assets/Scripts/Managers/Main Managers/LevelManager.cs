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
        [SerializeField] private LevelArrangementsListDataSO levelArrangementsListDataSO;
        [SerializeField] private Transform levelMainParent;
        [SerializeField] private Transform levelScrewsParent;
        [SerializeField] private Transform levelNutsParent;

        [Space]
        [ShowInInspector, ReadOnly] private LevelDataSO currentLevelDataSO;
        [ShowInInspector, ReadOnly] private List<BaseScrew> levelScrews = new List<BaseScrew>();
        [ShowInInspector, ReadOnly] private List<BaseNut> levelNuts = new List<BaseNut>();

        private const string RandomLevelGenerationSeedPrefsKey = "RandomLevelGenerationSeedPrefs";
        private const string LastGenerationSeedLevelNumberPrefsKey = "LastGenerationSeedLevelNumberPrefs";
        private const string LEVEL_AB_TEST_TYPE_PREFKEY = "LevelABTestTypePrefsKey";

        private List<Action> onLevelLoad = new List<Action>();
        private List<Action> onLevelComplete = new List<Action>();
        private List<Action> onLevelUnload = new List<Action>();
        private List<Action> onLevelReload = new List<Action>();

        public static bool CanPlayLevelLoadAnimation = true;
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

        public Transform LevelMainParent => levelMainParent;
        public LevelDataSO CurrentLevelDataSO => currentLevelDataSO;
        public List<BaseScrew> LevelScrews => levelScrews;
        public List<BaseNut> LevelNuts => levelNuts;

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
                {
                    Debug.LogError("Reset Level Data due to LevelABTestType Change" + value);
                    DataManager.Instance.ResetLevelProgressData();
                }
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
            LoadCurrentLevelData();
            InstantiateCurrentLevel(onLoad);
        }

        public void LoadSpecialLevel(int specialLevelNumber, Action onLoad)
        {
            LoadSpecialLevelData(specialLevelNumber);
            InstantiateCurrentLevel(onLoad);
        }

        public void OnReloadCurrentLevel()
        {
            GameplayManager.Instance.RestartGamePlay();
            InvokeOnLevelReload();
        }

        public bool CanLoadSpecialLevel(out int specialLevelNumber)
        {
            int currentPlayerLevel = DataManager.PlayerLevel;
            specialLevelNumber = CurrentVariant.GetSpecialLevelNumberCountToLoad(currentPlayerLevel);
            bool isPlayingSpecialLevel = CurrentLevelDataSO.levelType == LevelType.SPECIAL_LEVEL && CurrentLevelDataSO.level == specialLevelNumber;
            return !isPlayingSpecialLevel && HasSpecialLevel(specialLevelNumber);
        }

        public bool HasSpecialLevel(int specialLevelNumber)
        {
            return CurrentVariant.HasSpecialLevel(specialLevelNumber);
        }

        // Use this for Level Editor Purpose Only
        public void LoadLevel(LevelDataSO levelDataSO, Action onLoad)
        {
            currentLevelDataSO = levelDataSO;
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
            return levelScrews.Find(x => x.GridCellId == gridCellId);
        }

        public LevelArrangementConfigDataSO GetCurrentLevelArrangementConfig()
        {
            return levelArrangementsListDataSO.GetLevelArrangementConfig(currentLevelDataSO.ArrangementId);
        }

        public NutColorThemeInfo GetNutTheme(int nutColorId)
        {
            return CurrentVariant.GetNutTheme(nutColorId);
        }

        public void UnLoadLevel()
        {
            RecycleAllLevelElements();
            InvokeOnLevelUnLoad();
        }

        public void OnLevelComplete()
        {
            if (CurrentLevelDataSO.levelType == LevelType.NORMAL_LEVEL)
                DataManager.Instance.IncreasePlayerLevel();
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
        private LevelABTestType GetDefaultLevelABTestType()
        {
            return (LevelABTestType)levelABTestRemoteConfig.GetDefaultValue<int>();
        }

        private void LoadCurrentLevelData()
        {
            int currentLevel = DataManager.PlayerLevel;
            int totalLevel = CurrentVariant.GetNormalLevelCount();
            int repeatLastLevelsCountAfterGameFinish = CurrentVariant.RepeatLastLevelsCountAfterGameFinish;

            if (currentLevel > totalLevel)
                currentLevel = GetCappedLevel(currentLevel, totalLevel, repeatLastLevelsCountAfterGameFinish);

            currentLevelDataSO = CurrentVariant.GetNormalLevel(currentLevel);
        }

        private void LoadSpecialLevelData(int specialLevelNumber)
        {
            currentLevelDataSO = CurrentVariant.GetSpecialLevel(specialLevelNumber);
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

        private void InstantiateCurrentLevelScrews()
        {
            LevelArrangementConfigDataSO levelArrangementConfigDataSO = GetCurrentLevelArrangementConfig();
            if (levelArrangementConfigDataSO == null)
                return;

            for (int i = 0; i < currentLevelDataSO.levelScrewDataInfos.Count; i++)
            {
                if (i >= levelArrangementConfigDataSO.arrangementCellIds.Count)
                {
                    Debug.LogError("No Position Arrangement Data Found ! Please Check Arrangement !");
                    break;
                }

                BaseScrew myScrew = ObjectPool.Instance.Spawn(ResourceManager.Instance.GetScrew(currentLevelDataSO.levelScrewDataInfos[i].screwType), levelScrewsParent);
                GridCellId myGridCellId = levelArrangementConfigDataSO.arrangementCellIds[i];

                myScrew.transform.position = levelArrangementConfigDataSO.GetCellPosition(myGridCellId);
                myScrew.gameObject.SetActive(true);

                myScrew.InitScrew(myGridCellId, currentLevelDataSO.levelScrewDataInfos[i]);

                levelScrews.Add(myScrew);
            }
        }

        private void InstantiateCurrentLevelNuts()
        {
            for (int i = 0; i < currentLevelDataSO.screwNutsLevelDataInfos.Count; i++)
            {
                GridCellId screwId = currentLevelDataSO.screwNutsLevelDataInfos[i].targetScrewGridCellId;
                BaseScrew targetScrew = GetScrewOfGridCell(screwId);
                for (int j = currentLevelDataSO.screwNutsLevelDataInfos[i].levelNutDataInfos.Count - 1; j >= 0; j--) // Reverse loop for setting nuts in screw
                {
                    BaseNutLevelDataInfo nutScrewData = currentLevelDataSO.screwNutsLevelDataInfos[i].levelNutDataInfos[j];
                    BaseNut myNut = ObjectPool.Instance.Spawn(ResourceManager.Instance.GetNut(nutScrewData.nutType), levelNutsParent);

                    myNut.gameObject.SetActive(true);
                    myNut.InitNut(nutScrewData);

                    levelNuts.Add(myNut);
                    targetScrew.AddNut(myNut);
                }
            }
        }

        private void RecycleAllLevelElements()
        {
            levelScrews.ForEach(x => x?.Recycle());
            LevelNuts.ForEach(x => x?.Recycle());
            levelScrews.Clear();
            levelNuts.Clear();
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        private IEnumerator WaitForRCValuesFetched(Action onComplete)
        {
#if UNITY_EDITOR
            onComplete.Invoke();
#endif

            while (!GameAnalyticsManager.Instance.IsRCValuesFetched)
            {
                yield return null;
            }
            onComplete.Invoke();
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