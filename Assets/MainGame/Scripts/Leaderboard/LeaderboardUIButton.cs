using UnityEngine;
using UnityEngine.UI;

namespace com.tag.nut_sort {
    public class LeaderboardUIButton : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private GameObject buttonParent;
        [SerializeField] private GameObject notificationObject;

        [SerializeField] private Text leaderBoardTimerText;
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
            UnregisterLeaderBoardTimer();
        }
        #endregion

        #region PUBLIC_METHODS
        #endregion

        #region PRIVATE_METHODS
        private void CheckForLeaderboardButton()
        {
            bool isLeaderboardUnlocked = LeaderboardManager.Instance.IsLeaderboardUnlocked();
            UnregisterLeaderBoardTimer();

            if (isLeaderboardUnlocked)
                RegisterLeaderBoardTimer();

            buttonParent.gameObject.SetActive(isLeaderboardUnlocked);
            RefreshNotificationObject();
        }
        private void RegisterLeaderBoardTimer()
        {
            if (LeaderboardManager.Instance.IsSystemInitialized && LeaderboardManager.Instance.LeaderboardRunTimer != null)
                LeaderboardManager.Instance.LeaderboardRunTimer.RegisterTimerTickEvent(UpdateLeaderBoardTimer);
        }
        private void UnregisterLeaderBoardTimer()
        {
            if (LeaderboardManager.Instance.IsSystemInitialized && LeaderboardManager.Instance.LeaderboardRunTimer != null)
                LeaderboardManager.Instance.LeaderboardRunTimer.UnregisterTimerTickEvent(UpdateLeaderBoardTimer);
        }

        private void UpdateLeaderBoardTimer()
        {
            leaderBoardTimerText.text = LeaderboardManager.Instance.LeaderboardRunTimer.GetRemainingTimeSpan().ParseTimeSpan(2);
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