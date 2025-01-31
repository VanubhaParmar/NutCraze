using System.Collections.Generic;
using UnityEngine;

namespace com.tag.nut_sort {
    [CreateAssetMenu(fileName = "BaseNotificationSO", menuName = Constant.GAME_NAME + "/Local Notification/BaseNotificationSO")]
    public class BaseNotificationSO : ScriptableObject
    {
        #region PUBLIC_VARS

        //public string channelId;
        //public string channelName;
        //public string channelDiscription;
        //public string smallIconName;
        //public string largeIconName;
        public string title;
        public List<string> text;
        //public int minSecondToFire;

        #endregion

        #region PRIVATE_VARS

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS
        public virtual string GetNotificationText()
        {
            return text.GetRandomItemFromList();
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
    }
}