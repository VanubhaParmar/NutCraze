using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "GameMainDataSO", menuName = Constant.GAME_NAME + "/Managers/GameMainDataSO")]
    public class GameMainDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARIABLES
        public int undoBoostersCountToAddOnAdWatch = 5;
        public int extraScrewBoostersCountToAddOnAdWatch = 2;

        [Space]
        public int playSpecialLevelAfterEveryLevelsCount = 8;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public bool CanLoadSpecialLevel(int currentLevel)
        {
            return (currentLevel - 1) % playSpecialLevelAfterEveryLevelsCount == 0 && currentLevel > 1;
        }

        public int GetSpecialLevelNumberCountToLoad(int currentLevel)
        {
            if (CanLoadSpecialLevel(currentLevel))
                return Mathf.FloorToInt((currentLevel - 1) / playSpecialLevelAfterEveryLevelsCount);
            return 0;
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
}