using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "LevelArrangementsDataSO", menuName = Constant.GAME_NAME + "/Level Data/LevelArrangementsListDataSO")]
    public class LevelArrangementsListDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARIABLES
        [SerializeField] private Dictionary<int, LevelArrangementConfigDataSO> arrangementConfigMapping = new Dictionary<int, LevelArrangementConfigDataSO>();
        private List<LevelArrangementConfigDataSO> levelArrangementConfigDataSOs;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        public List<LevelArrangementConfigDataSO> LevelArrangementConfigDataSOs
        {
            get
            {
                if (levelArrangementConfigDataSOs == null || levelArrangementConfigDataSOs.Count == 0)
                    levelArrangementConfigDataSOs = arrangementConfigMapping.Values.ToList();
                return levelArrangementConfigDataSOs;
            }
        }
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public LevelArrangementConfigDataSO GetLevelArrangementConfig(int arrangementId)
        {
            if (arrangementConfigMapping.ContainsKey(arrangementId))
                return arrangementConfigMapping[arrangementId];
            return null;
        }

        public List<int> GetAllArrangementIds()
        {
            return arrangementConfigMapping.Keys.ToList();
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
        [Button]
        public void AddArrangementConfig(List<LevelArrangementConfigDataSO> list)
        {
            arrangementConfigMapping.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                arrangementConfigMapping.Add(list[i].ArrangementId, list[i]);
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        #endregion
    }
}