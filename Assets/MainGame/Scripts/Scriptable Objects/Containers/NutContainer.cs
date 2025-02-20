using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "NutContainer", menuName = Constant.GAME_NAME + "/Container/NutContainer")]
    public class NutContainer : SerializedScriptableObject
    {
        #region PRIVATE_VARS
        [SerializeField] private Dictionary<int, BaseNut> nuts = new Dictionary<int, BaseNut>();
        #endregion

        #region PUBLIC_VARS
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        public BaseNut GetNut(int nutId)
        {
            if (nuts.ContainsKey(nutId))
                return nuts[nutId];
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
        public void AddScrew(List<BaseNut> nuts)
        {
            for (int i = 0; i < nuts.Count; i++)
            {
                if (!this.nuts.ContainsKey(nuts[i].NutType))
                {
                    this.nuts.Add(nuts[i].NutType, nuts[i]);
                }
            }
        }
#endif
        #endregion
    }
}
