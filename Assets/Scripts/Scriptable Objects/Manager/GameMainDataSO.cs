using Sirenix.OdinInspector;
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
        public BaseReward levelCompleteReward;

        [Space]
        public string playStoreLink;
        public string privacyPolicyLink;
        public string termsLink;

        [Space]
        public List<int> showRateUsAtLevels;

        #endregion

        #region PRIVATE_VARIABLES

        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public bool CanShowRateUsPopUp()
        {
            int currentLevel = PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel;
            return showRateUsAtLevels.Contains(currentLevel - 1);
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

        #region UNITY_EDITOR_FUNCTIONS
#if UNITY_EDITOR
#endif
        #endregion
    }
}