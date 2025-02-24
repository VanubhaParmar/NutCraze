using I2.Loc;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class InGameLoadingView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Localize mainMessageText;
        [SerializeField] private Localize extraMessageText;
        [SerializeField] private List<RectTransform> updateLayoutRects;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void Show(string mainMessage = "Please Wait !", string extraMessage = "")
        {
            mainMessageText.SetTerm(mainMessage);
            extraMessageText.SetTerm(extraMessage);
            extraMessageText.gameObject.SetActive(!string.IsNullOrEmpty(extraMessage));

            updateLayoutRects.ForEach(x => LayoutRebuilder.ForceRebuildLayoutImmediate(x));
            UIUtilityEvents.RaiseOnRefreshUIRects();
            base.Show();
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