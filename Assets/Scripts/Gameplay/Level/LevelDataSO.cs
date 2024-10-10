using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "LevelDataSO", menuName = Constant.GAME_NAME + "/Level Data/LevelDataSO")]
    public class LevelDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARIABLES
        public int level;

        [Header("Arrangement Config")]
        public LevelArrangementConfigDataSO levelArrangementConfigDataSO;

        [Header("Screw Data")]
        public List<BaseScrewLevelDataInfo> levelScrewDataInfos;

        [Header("Nuts Data")]
        public List<ScrewNutsLevelDataInfo> screwNutsLevelDataInfos;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
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

    public abstract class BaseScrewLevelDataInfo
    {
        [ScrewTypeId] public int screwType;
        public int screwNutsCapacity = 3;
    }

    public class ScrewNutsLevelDataInfo
    {
        public GridCellId targetScrewGridCellId;
        public List<BaseNutLevelDataInfo> levelNutDataInfos;
    }

    public abstract class BaseNutLevelDataInfo
    {
        [NutTypeId] public int nutType;
        [NutColorId] public int nutColorTypeId;
    }
}