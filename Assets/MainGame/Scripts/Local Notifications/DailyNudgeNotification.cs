using System;
using UnityEngine;

namespace com.tag.nut_sort {
    public class DailyNudgeNotification : BaseNotification
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        [SerializeField] private DailyNudgeNotificationSO[] _dailyNudgeNotificationSO;

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public override void GenerateNotification()
        {
            for (int i = 0; i < _dailyNudgeNotificationSO.Length; i++)
            {
                double time = _dailyNudgeNotificationSO[i].timeOfNotification * 3600 - TimeManager.Now.TimeOfDay.TotalSeconds;
                time = time < 0 ? (time + 24 * 3600) : time;
                LocalNotificationManager.Instance.SendNotification(_dailyNudgeNotificationSO[i], (int)time);
            }
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
