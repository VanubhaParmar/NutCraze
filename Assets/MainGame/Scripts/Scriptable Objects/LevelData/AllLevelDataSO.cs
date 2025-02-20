using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "AllLevelDataSO", menuName = Constant.GAME_NAME + "/Level Data/AllLevelDataSO")]

    public class AllLevelDataSO : SerializedScriptableObject
    {
        #region PRIVATE_VARS
        [SerializeField] private List<LevelDataSO> levelDatas = new List<LevelDataSO>();
        #endregion

        #region PUBLIC_VARS
        #endregion

        #region PROPERTIES
        public int TotalLevelCount => levelDatas.Count;
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        public LevelDataSO GetLevelData(int levelNo)
        {
            for (int i = 0; i < levelDatas.Count; i++)
            {
                if (levelDatas[i].Level == levelNo)
                    return levelDatas[i];
            }
            return levelDatas[levelDatas.Count - 1];
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region EDITOR
#if UNITY_EDITOR
#endif
        #endregion
    }
}
