using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.tag.nut_sort {
    public class UserPromptView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Text userMsgText;
        private Action actionToCallOnOk;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void Show(string userMsg, Action actionToCallOnOkButtonClick = null)
        {
            userMsgText.text = userMsg;
            this.actionToCallOnOk = actionToCallOnOkButtonClick;
            Show();
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_OK()
        {
            Hide();
            actionToCallOnOk?.Invoke();
        }
        #endregion
    }
}