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
        [SerializeField] private LevelVariantMasterSO levelVariantMaster;
        [SerializeField] private LevelArrangementsListDataSO levelArrangementsListDataSO;
        [SerializeField] private Transform levelMainParent;
        [SerializeField] private Transform levelScrewsParent;
        [SerializeField] private Transform levelNutsParent;
        [SerializeField] private NutColorThemeTemplateDataSO _nutColorThemeTemplateDataSO;

        [Space]
        [ShowInInspector, ReadOnly] private LevelDataSO currentLevelDataSO;
        [ShowInInspector, ReadOnly] private List<BaseScrew> levelScrews = new List<BaseScrew>();
        [ShowInInspector, ReadOnly] private List<BaseNut> levelNuts = new List<BaseNut>();
        [ShowInInspector] private ABTestType currentABType;
        [ShowInInspector] private LevelVariantSO currentVariant;

        private const string RandomLevelGenerationSeedPrefsKey = "RandomLevelGenerationSeedPrefs";
        private const string LastGenerationSeedLevelNumberPrefsKey = "LastGenerationSeedLevelNumberPrefs";

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
        public NutColorThemeTemplateDataSO NutColorThemeTemplateDataSO => _nutColorThemeTemplateDataSO;
        public List<BaseScrew> LevelScrews => levelScrews;
        public List<BaseNut> LevelNuts => levelNuts;
        #endregion

        #region UNITY_CALLBACKS
        private void OnEnable()
        {
            GameplayManager.onLevelRecycle += GameplayManager_onLevelRecycle;
        }

        private void OnDisable()
        {
            GameplayManager.onLevelRecycle -= GameplayManager_onLevelRecycle;
        }

        public override void Awake()
        {
            base.Awake();
            AssignABVariant();
            OnLoadingDone();
        }
        #endregion

        #region PUBLIC_METHODS
        public void LoadCurrentLevel()
        {
            LoadCurrentLevelData();
            InstantiateCurrentLevel();
        }
        public void LoadSpecialLevel(int specialLevelNumber)
        {
            LoadSpecialLevelData(specialLevelNumber);
            InstantiateCurrentLevel();
        }

        public bool CanLoadSpecialLevel(out int specialLevelNumber)
        {
            int currentPlayerLevel = PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel;
            specialLevelNumber = currentVariant.GetSpecialLevelNumberCountToLoad(currentPlayerLevel);
            bool isPlayingSpecialLevel = CurrentLevelDataSO.levelType == LevelType.SPECIAL_LEVEL && CurrentLevelDataSO.level == specialLevelNumber;
            return !isPlayingSpecialLevel && currentVariant.HasSpecialLevel(specialLevelNumber);

        }
        public void LoadCurrentLevelData()
        {
            int currentLevel = PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel;
            int totalLevel = currentVariant.GetNormalLevelCount();
            int repeatLastLevelsCountAfterGameFinish = currentVariant.RepeatLastLevelsCountAfterGameFinish;

            if (currentLevel > totalLevel)
                currentLevel = GetCappedLevel(currentLevel, totalLevel, repeatLastLevelsCountAfterGameFinish);

            currentLevelDataSO = currentVariant.GetNormalLevel(currentLevel);
        }

        public void LoadSpecialLevelData(int specialLevelNumber)
        {
            currentLevelDataSO = currentVariant.GetSpecialLevel(specialLevelNumber);
        }

        // Use this for Level Editor Purpose Only
        public void LoadLevel(LevelDataSO levelDataSO)
        {
            currentLevelDataSO = levelDataSO;
            InstantiateCurrentLevel();
        }

        public void InstantiateCurrentLevel()
        {
            ResetLevelGeneration();

            InstantiateCurrentLevelScrews();
            InstantiateCurrentLevelNuts();

            RaiseOnLevelLoadOver();
        }

        public BaseScrew GetScrewOfGridCell(GridCellId gridCellId)
        {
            return levelScrews.Find(x => x.GridCellId.IsEqual(gridCellId));
        }

        public LevelArrangementConfigDataSO GetCurrentLevelArrangementConfig()
        {
            return levelArrangementsListDataSO.GetLevelArrangementConfig(currentLevelDataSO.ArrangementId);
        }
        #endregion

        #region PRIVATE_METHODS
        private void AssignABVariant()
        {
            StartCoroutine(WaitForABTestManagerToInitilize(() =>
            {
                ABTestType aBTestType = ABTestManager.Instance.GetAbTestType(ABTestSystemType.Level);
                Debug.Log("AssignABVariant0- ");
                if (!levelVariantMaster.IsVariantExist(aBTestType))
                {
                    Debug.Log("AssignABVariant1- ");
                    ABTestManager.Instance.UpdateNewABTestType(ABTestSystemType.Level, out aBTestType);
                }

                levelVariantMaster.GetLevelVariant(aBTestType, out currentABType, out currentVariant);
                Debug.Log("AssignABVariant2- " + currentABType + " " + currentVariant.GetNormalLevelCount() + " " + currentVariant.GetSpecailLevelCount());
                if (aBTestType != currentABType)
                    ABTestManager.Instance.SetABTestType(ABTestSystemType.Level, currentABType);
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
            LevelArrangementConfigDataSO levelArrangementConfigDataSO = GetCurrentLevelArrangementConfig();
            for (int i = 0; i < currentLevelDataSO.levelScrewDataInfos.Count; i++)
            {
                if (i >= levelArrangementConfigDataSO.arrangementCellIds.Count)
                {
                    Debug.LogError("No Position Arrangement Data Found ! Please Check Arrangement !");
                    break;
                }

                BaseScrew myScrew = ObjectPool.Instance.Spawn(PrefabsHolder.Instance.GetScrewPrefab(currentLevelDataSO.levelScrewDataInfos[i].screwType), levelScrewsParent);
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
                    BaseNut myNut = ObjectPool.Instance.Spawn(PrefabsHolder.Instance.GetNutPrefab(nutScrewData.nutType), levelNutsParent);

                    myNut.gameObject.SetActive(true);
                    myNut.InitNut(nutScrewData);

                    levelNuts.Add(myNut);

                    if (targetScrew.TryGetScrewBehaviour(out NutsHolderScrewBehaviour behaviour))
                    {
                        behaviour.AddNut(myNut);
                    }
                }
            }
        }

        private void ResetLevelGeneration()
        {
            levelScrews.ForEach(x => x.Recycle());
            LevelNuts.ForEach(x => x.Recycle());

            levelScrews.Clear();
            levelNuts.Clear();
        }
        #endregion

        #region EVENT_HANDLERS
        public delegate void LevelManagerVoidEvent();
        public static event LevelManagerVoidEvent onLevelLoadOver;
        public static void RaiseOnLevelLoadOver()
        {
            if (onLevelLoadOver != null)
                onLevelLoadOver();
        }

        private void GameplayManager_onLevelRecycle()
        {
            ResetLevelGeneration();
        }
        #endregion

        #region COROUTINES
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