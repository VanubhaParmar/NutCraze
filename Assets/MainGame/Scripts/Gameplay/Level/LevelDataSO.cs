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
        [SerializeField] private int level;
        [SerializeField] private LevelType levelType;

        [Header("Arrangement Config")]
        public LevelArrangementConfigDataSO levelArrangementConfigDataSO;

        [Header("Screw Data")]
        public List<BaseScrewLevelDataInfo> levelScrewDataInfos;

        [Header("Nuts Data")]
        public List<ScrewNutsLevelDataInfo> screwNutsLevelDataInfos;

        public int Level { get => level; set => level = value; }
        public LevelType LevelType { get => levelType; set => levelType = value; }
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void CloneTo(LevelDataSO levelDataSOToCloneTo)
        {
            levelDataSOToCloneTo.Level = this.Level;

            levelDataSOToCloneTo.levelArrangementConfigDataSO = this.levelArrangementConfigDataSO;

            levelDataSOToCloneTo.levelScrewDataInfos = new List<BaseScrewLevelDataInfo>();
            levelDataSOToCloneTo.levelScrewDataInfos.AddRange(this.levelScrewDataInfos.Select(x => x.Clone()).ToList());

            levelDataSOToCloneTo.screwNutsLevelDataInfos = new List<ScrewNutsLevelDataInfo>();
            levelDataSOToCloneTo.screwNutsLevelDataInfos.AddRange(this.screwNutsLevelDataInfos.Select(x => x.Clone()).ToList());
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
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