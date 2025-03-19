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
        [SerializeField, LevelArrangementId] private int arrangementId;
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
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region EDITOR
#if UNITY_EDITOR
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