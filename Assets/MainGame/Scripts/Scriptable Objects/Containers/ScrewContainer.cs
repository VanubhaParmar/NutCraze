using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "ScrewContainer", menuName = Constant.GAME_NAME + "/Container/ScrewContainer")]
    public class ScrewContainer : SerializedScriptableObject
    {
        #region PRIVATE_VARS
        [SerializeField] private Dictionary<int, BaseScrew> screws = new Dictionary<int, BaseScrew>();
        #endregion

        #region PUBLIC_VARS
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        public BaseScrew GetScrew(int screwId)
        {
            if (screws.ContainsKey(screwId))
                return screws[screwId];
            return null;
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
        [Button]
        public void AddScrew(List<BaseScrew> baseScrews)
        {
            for (int i = 0; i < baseScrews.Count; i++)
            {
                if (!screws.ContainsKey(baseScrews[i].ScrewType))
                {
                    screws.Add(baseScrews[i].ScrewType, baseScrews[i]);
                }
            }
        }
#endif
        #endregion
    }
}
