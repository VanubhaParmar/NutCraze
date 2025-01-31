using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.tag.nut_sort {
    public class InGameLoadingView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Text mainMessageText;
        [SerializeField] private Text extraMessageText;
        [SerializeField] private List<RectTransform> updateLayoutRects;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void Show(string mainMessage = "Please Wait !", string extraMessage = "")
        {
            mainMessageText.text = mainMessage;
            extraMessageText.text = extraMessage;
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