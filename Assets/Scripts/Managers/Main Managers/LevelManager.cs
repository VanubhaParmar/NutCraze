using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class LevelManager : SerializedManager<LevelManager>
    {
        #region PUBLIC_VARIABLES
        public Transform LevelMainParent => levelMainParent;
        public LevelDataSO CurrentLevelDataSO => currentLevelDataSO;
        public NutColorThemeTemplateDataSO NutColorThemeTemplateDataSO => _nutColorThemeTemplateDataSO;
        public List<BaseScrew> LevelScrews => levelScrews;
        public List<BaseNut> LevelNuts => levelNuts;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Transform levelMainParent;
        [SerializeField] private Transform levelScrewsParent;
        [SerializeField] private Transform levelNutsParent;
        [SerializeField] private NutColorThemeTemplateDataSO _nutColorThemeTemplateDataSO;

        [Space]
        [ShowInInspector, ReadOnly] private LevelDataSO currentLevelDataSO;
        [ShowInInspector, ReadOnly] private List<BaseScrew> levelScrews = new List<BaseScrew>();
        [ShowInInspector, ReadOnly] private List<BaseNut> levelNuts = new List<BaseNut>();
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            InitLevelManager();
            OnLoadingDone();
        }
        #endregion

        #region PUBLIC_METHODS
        public void InitLevelManager()
        {
            LoadCurrentLevel();
        }

        public void LoadCurrentLevel()
        {
            LoadCurrentLevelData();
            InstantiateCurrentLevel();
        }

        public void LoadCurrentLevelData()
        {
            int currentLevel = PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel;
            currentLevelDataSO = Utility.LoadResourceAsset<LevelDataSO>(ResourcesConstants.LEVELS_PATH + string.Format(ResourcesConstants.LEVEL_SO_NAME_FORMAT, currentLevel));
        }

        public void InstantiateCurrentLevel()
        {
            ResetLevelGeneration();

            InstantiateCurrentLevelScrews();
            InstantiateCurrentLevelNuts();
        }

        public BaseScrew GetScrewOfGridCell(GridCellId gridCellId)
        {
            return levelScrews.Find(x => x.GridCellId.IsEqual(gridCellId));
        }

        public bool DoesLevelExist(int level)
        {
            var levelData = Utility.LoadResourceAsset<LevelDataSO>(ResourcesConstants.LEVELS_PATH + string.Format(ResourcesConstants.LEVEL_SO_NAME_FORMAT, level));
            bool result = levelData != null;

            Resources.UnloadAsset(levelData);
            levelData = null;

            return result;
        }
        #endregion

        #region PRIVATE_METHODS
        private void InstantiateCurrentLevelScrews()
        {
            for (int i = 0; i < currentLevelDataSO.levelScrewDataInfos.Count; i++)
            {
                if (i >= currentLevelDataSO.levelArrangementConfigDataSO.arrangementCellIds.Count)
                {
                    Debug.LogError("No Position Arrangement Data Found ! Please Check Arrangement !");
                    break;
                }

                BaseScrew myScrew = ObjectPool.Instance.Spawn(PrefabsHolder.Instance.GetScrewPrefab(currentLevelDataSO.levelScrewDataInfos[i].screwType), levelScrewsParent);
                GridCellId myGridCellId = currentLevelDataSO.levelArrangementConfigDataSO.arrangementCellIds[i];

                myScrew.transform.position = currentLevelDataSO.levelArrangementConfigDataSO.GetCellPosition(myGridCellId);
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
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}