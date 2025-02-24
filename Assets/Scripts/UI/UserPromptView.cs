using I2.Loc;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class UserPromptView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Localize userMsgText;
        private Action actionToCallOnOk;
        private Text text;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void Show(string userMsg, Action actionToCallOnOkButtonClick = null, bool canLocalize = true)
        {
            if (canLocalize)
            {
                userMsgText.SetTerm(userMsg);
            }
            else
            {
                if (text == null)
                    text = userMsgText.GetComponent<Text>();
                if (text != null)
                    text.text = userMsg;
            }
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