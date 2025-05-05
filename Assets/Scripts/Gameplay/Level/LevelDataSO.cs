using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "LevelDataSO", menuName = Constant.GAME_NAME + "/Level Data/LevelDataSO")]
    public class LevelDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARIABLES

        public int level;
        public LevelType levelType;
        [ValidateInput("ValidateDefaultVariant", "Must contain Default AB Type variant!", InfoMessageType.Warning)]
        [SerializeField, ScrewArrangementId] private int arrangementId;
        public List<BaseScrewLevelDataInfo> levelScrewDataInfos;
        public List<ScrewNutsLevelDataInfo> screwNutsLevelDataInfos;

        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        public int ArrangementId { get => arrangementId; set => arrangementId = value; }
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void CloneTo(LevelDataSO levelDataSOToCloneTo)
        {
            levelDataSOToCloneTo.level = this.level;

            levelDataSOToCloneTo.ArrangementId = this.ArrangementId;

            levelDataSOToCloneTo.levelScrewDataInfos = new List<BaseScrewLevelDataInfo>();
            levelDataSOToCloneTo.levelScrewDataInfos.AddRange(this.levelScrewDataInfos.Select(x => x.Clone()).ToList());

            levelDataSOToCloneTo.screwNutsLevelDataInfos = new List<ScrewNutsLevelDataInfo>();
            levelDataSOToCloneTo.screwNutsLevelDataInfos.AddRange(this.screwNutsLevelDataInfos.Select(x => x.Clone()).ToList());
        }
        #endregion

        #region PRIVATE_METHODS
        private bool ValidateDefaultVariant(int id)
        {
            return id != 0;
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region EDITOR
#if UNITY_EDITOR

        [Button]
        public void VarifyBoosterScrewCapacity()
        {
            List<BaseScrewLevelDataInfo> baseScrewLevelDataInfos = levelScrewDataInfos.FindAll(x => x.screwType == ScrewTypeIdConstant.SIMPLE_SCREW);
            BaseScrewLevelDataInfo minCapacityScrewInfo = baseScrewLevelDataInfos.OrderBy(x => x.screwNutsCapacity).FirstOrDefault();
            int capacity = minCapacityScrewInfo.screwNutsCapacity;

            foreach (var screwData in levelScrewDataInfos)
            {
                if (screwData.screwType == ScrewTypeIdConstant.BOOSTER_ACTIVATED_SCREW)
                {
                    capacity = Mathf.Clamp(capacity, 1, Constant.MAX_BOOSTER_CAPACITY);
                    screwData.screwNutsCapacity = capacity;
                }
            }
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
        }

        public bool ValidateLevelData()
        {
            bool issueFound = false;
            List<int> screwCapacity = new List<int>();
            Dictionary<int, int> nutsDataDict = new Dictionary<int, int>();

            foreach (var screwData in levelScrewDataInfos)
            {
                if (screwData.screwType == 1 && !screwCapacity.Contains(screwData.screwNutsCapacity))
                    screwCapacity.Add(screwData.screwNutsCapacity);
            }

            foreach (var nutsBunchData in screwNutsLevelDataInfos)
            {
                foreach (var nutsData in nutsBunchData.levelNutDataInfos)
                {
                    if (!nutsDataDict.ContainsKey(nutsData.nutColorTypeId))
                        nutsDataDict.Add(nutsData.nutColorTypeId, 1);
                    else
                        nutsDataDict[nutsData.nutColorTypeId]++;
                }
            }

            foreach (var kvp in nutsDataDict)
            {
                if (!screwCapacity.Contains(kvp.Value))
                {
                    Debug.LogError("Issue found in Level : " + level + " With Nut Color : " + kvp.Key + "-" + kvp.Value);
                    issueFound = true;
                }
            }
            return !issueFound;
        }
#endif
        #endregion
    }

    public enum LevelType
    {
        NORMAL_LEVEL = 0,
        SPECIAL_LEVEL = 1,
    }

    public class BaseScrewLevelDataInfo
    {
        [ScrewTypeId] public int screwType;
        public int screwNutsCapacity = 3;

        public virtual BaseScrewLevelDataInfo Clone()
        {
            return new BaseScrewLevelDataInfo() { screwType = this.screwType, screwNutsCapacity = this.screwNutsCapacity };
        }
    }

    public class ScrewNutsLevelDataInfo
    {
        public GridCellId targetScrewGridCellId;
        public List<BaseNutLevelDataInfo> levelNutDataInfos;

        public virtual ScrewNutsLevelDataInfo Clone()
        {
            var newData = new ScrewNutsLevelDataInfo() { targetScrewGridCellId = this.targetScrewGridCellId };

            newData.levelNutDataInfos = new List<BaseNutLevelDataInfo>();
            levelNutDataInfos.ForEach(x => newData.levelNutDataInfos.Add(x.Clone()));

            return newData;
        }
    }

    public class BaseNutLevelDataInfo
    {
        [NutTypeId] public int nutType;
        [NutColorId] public int nutColorTypeId;

        public virtual BaseNutLevelDataInfo Clone()
        {
            return new BaseNutLevelDataInfo() { nutType = this.nutType, nutColorTypeId = this.nutColorTypeId };
        }
    }
}