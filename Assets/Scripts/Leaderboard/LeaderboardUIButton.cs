using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class LeaderboardUIButton : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private GameObject buttonParent;
        [SerializeField] private GameObject notificationObject;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void OnEnable()
        {
            CheckForLeaderboardButton();
            LeaderboardManager.onLeaderboardEventStateChanged += LeaderboardManager_onLeaderboardEventStateChanged;
        }

        private void OnDisable()
        {
            LeaderboardManager.onLeaderboardEventStateChanged -= LeaderboardManager_onLeaderboardEventStateChanged;
        }
        #endregion

        #region PUBLIC_METHODS
        #endregion

        #region PRIVATE_METHODS
        private void CheckForLeaderboardButton()
        {
            buttonParent.gameObject.SetActive(LeaderboardManager.Instance.IsLeaderboardUnlocked());
            RefreshNotificationObject();
        }
        #endregion

        #region EVENT_HANDLERS
        private void LeaderboardManager_onLeaderboardEventStateChanged()
        {
            CheckForLeaderboardButton();
        }

        private void RefreshNotificationObject()
        {
            notificationObject.gameObject.SetActive(false);
            if (!LeaderboardManager.Instance.IsCurrentLeaderboardEventActive() && LeaderboardManager.Instance.IsLastEventResultReadyToShow())
                notificationObject.gameObject.SetActive(true);
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_Leaderboard()
        {
            if (LeaderboardManager.Instance.CanOpenLeaderboardUI())
                MainSceneUIManager.Instance.GetView<LeaderboardView>().Show();
            else
                ToastMessageView.Instance.ShowMessage(UserPromptMessageConstants.NextLeaderboardEventMessage + LeaderboardManager.Instance.LeaderboardRunTimer.GetRemainingTimeSpan().ParseTimeSpan(2));
        }
        #endregion
    }
}