using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class LevelManager : SerializedManager<LevelManager>
    {
        #region PRIVATE_VARIABLES
        [SerializeField] private Transform levelMainParent;
        [SerializeField] private Transform levelScrewsParent;
        [SerializeField] private Transform levelNutsParent;
        [SerializeField] private NutColorThemeTemplateDataSO _nutColorThemeTemplateDataSO;

        [ShowInInspector, ReadOnly] private LevelDataSO currentLevelDataSO;
        [ShowInInspector, ReadOnly] private List<BaseScrew> levelScrews = new List<BaseScrew>();
        [ShowInInspector, ReadOnly] private List<BaseNut> levelNuts = new List<BaseNut>();
        private Level loadedLevel;

        private List<Action> onLevelLoad = new List<Action>();
        private List<Action> onLevelComplete = new List<Action>();
        private List<Action> onLevelUnload = new List<Action>();
        #endregion

        #region PUBLIC_VARIABLES
        public Transform LevelMainParent => levelMainParent;
        public LevelDataSO CurrentLevelDataSO => currentLevelDataSO;
        public NutColorThemeTemplateDataSO NutColorThemeTemplateDataSO => _nutColorThemeTemplateDataSO;
        public List<BaseScrew> LevelScrews => levelScrews;
        public List<BaseNut> LevelNuts => levelNuts;
        public Level LoadedLevel { get => loadedLevel; private set => loadedLevel = value; }
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            OnLoadingDone();
        }
        #endregion

        #region PUBLIC_METHODS
        public void LoadLevel(LevelDataSO level, Action onLoad = null)
        {
            LoadedLevel = new Level(level);
            InstantiateCurrentLevel(onLoad);
        }

        public LevelDataSO GetLevelData(int level)
        {
            if (level > GameManager.Instance.GameMainDataSO.totalLevelsInBuild)
                level = GameManager.Instance.GameMainDataSO.GetCappedLevel(level);
            return ResourceManager.Instance.GetLevelData(level);
        }

        public void InstantiateCurrentLevel(Action onLoad)
        {
            RecycleAllLevelElements();

            InstantiateCurrentLevelScrews();
            InstantiateCurrentLevelNuts();
            InvokeOnLevelLoad();
            VFXManager.Instance.PlayLevelLoadAnimation(onLoad);
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

        public bool DoesSpecialLevelExist(int level)
        {
            var levelData = Utility.LoadResourceAsset<LevelDataSO>(ResourcesConstants.SPECIAL_LEVELS_PATH + string.Format(ResourcesConstants.LEVEL_SO_NAME_FORMAT, level));
            bool result = levelData != null;

            Resources.UnloadAsset(levelData);
            levelData = null;

            return result;
        }

        public void UnLoadLevel()
        {
            RecycleAllLevelElements();
            InvokeOnLevelRecycle();
        }

        public void OnLevelComplete()
        {
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
        #endregion

        #region PRIVATE_METHODS
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

        private void InvokeOnLevelRecycle()
        {
            for (int i = 0; i < onLevelUnload.Count; i++)
                onLevelUnload[i]?.Invoke();
        }


        private void InstantiateCurrentLevelScrews()
        {
            for (int i = 0; i < currentLevelDataSO.levelScrewDataInfos.Count; i++)
            {
                if (i >= currentLevelDataSO.levelArrangementConfigDataSO.arrangementCellIds.Count)
                {
                    Debug.LogError("No Position Arrangement Data Found ! Please Check Arrangement !");
                    break;
                }

                BaseScrew myScrew = ObjectPool.Instance.Spawn(ResourceManager.Instance.GetScrew(currentLevelDataSO.levelScrewDataInfos[i].screwType), levelScrewsParent);
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
                    BaseNut myNut = ObjectPool.Instance.Spawn(ResourceManager.Instance.GetNut(nutScrewData.nutType), levelNutsParent);

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

        private void RecycleAllLevelElements()
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

    [Serializable]
    public class Level
    {
        public List<BaseScrew> screws = new List<BaseScrew>();
        public List<BaseNut> nuts = new List<BaseNut>();
        public LevelDataSO levelData;
        public int Value => levelData.Level;
        public LevelType Type => levelData.LevelType;

        public Level(LevelDataSO levelData)
        {
            this.levelData = levelData;
        }
    }
}