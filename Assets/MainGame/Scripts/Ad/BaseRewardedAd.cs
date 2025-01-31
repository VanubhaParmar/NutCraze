using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tag.nut_sort {
    [System.Serializable]
    public class BaseRewardedAd
    {
        #region PUBLIC_VARS

        public float lastTimeRewardedLoadTime = 0;

        #endregion

        #region PRIVATE_VARS

        internal bool isRewardAdWatched = false;
        internal Action actionWatched;
        internal Action actionShowed;

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public virtual void Init()
        {
            LoadAd();
        }

        public virtual void LoadAd()
        {
            //AnalyticsManager.Instance.LogEvent_New_RewardedAdRequested();
        }

        public virtual void ShowAd(Action actionWatched, Action actionShowed = null)
        {
            this.actionShowed = actionShowed;
            this.actionWatched = actionWatched;
        }

        public virtual bool IsAdLoaded()
        {
            return false;
        }

        public void OnVideoAdDismiss()
        {
            //SoundManager.Instance.UnMuteMusicAndSFX();
            if (actionWatched != null && isRewardAdWatched)
            {
                actionWatched();
            }
            else if (actionShowed != null)
            {
                actionShowed();
            }
            isRewardAdWatched = false;
            ResetActions();
            LoadAd();
        }

        public void OnVideoReady()
        {
            lastTimeRewardedLoadTime = Time.time;
            //AnalyticsManager.Instance.LogEvent_New_RewardedAdFilled();
        }

        public void OnVideoFail()
        {
            //AnalyticsManager.Instance.LogEvent_New_RewardedAdFailed();
        }

        public void OnVideoShow()
        {
            //AnalyticsManager.Instance.LogEvent_New_RewardedAdShown((int)(Time.time - lastTimeRewardedLoadTime));
        }

        public void OnVideoComplete()
        {
            isRewardAdWatched = true;
        }

        public void OnVideoFailToShow()
        {
            LoadAd();
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        private void ResetActions()
        {
            actionShowed = actionWatched = null;
        }

        #endregion

        #region EVENT_CALLBACKS

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
