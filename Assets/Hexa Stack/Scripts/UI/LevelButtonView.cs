using System;
using TMPro;
using UnityEngine;

namespace Tag.NutSort
{
    public class LevelButtonView : MonoBehaviour
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        [SerializeField] private TMP_Text levelButtonText;
        private Action onClick;

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public void SetView(Action onClick)
        {
            gameObject.SetActive(true);
            this.onClick = onClick;
            levelButtonText.text = $"Level {DataManager.PlayerLevel.Value}";
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS

        public void OnClick()
        {
            onClick?.Invoke();
        }

        #endregion
    }
}
