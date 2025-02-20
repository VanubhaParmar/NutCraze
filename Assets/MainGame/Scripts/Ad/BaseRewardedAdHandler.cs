using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort {
    public class BaseRewardedAdHandler : SerializedMonoBehaviour
    {
        #region PUBLIC_VARS

        public int noOfAdLoad = 1;
        public static int loadAdAfterSeconds = 180;

        #endregion

        #region PRIVATE_VARS

        [SerializeField] internal List<BaseRewardedAd> _rewardedAdList = new List<BaseRewardedAd>();

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public virtual void Init()
        {
        }

        public virtual void ShowAd(Action actionWatched, Action actionShowed = null, RewardAdShowCallType rewardAdShowCallType = RewardAdShowCallType.None)
        {
            for (int i = 0; i < _rewardedAdList.Count; i++)
            {
                if (_rewardedAdList[i].IsAdLoaded())
                {
                    _rewardedAdList[i].ShowAd(actionWatched, actionShowed);
                    //DataManager.Instance.playerData.stats.totalVideoAdWatched++;
                    //DailyTaskManager.Instance.AddDailyTaskProgress(TaskType.WATCH_AD, 1);
                    Debug.Log("On Ad Task Added");
                    //DataManager.Instance.SavePlayerData();
                    //AnalyticsManager.Instance.LogEvent_New_RewardedAdInfo(rewardAdShowCallType.ToString());
                    //AnalyticsManager.Instance.LogEvent_RV_Watched();
                    return;
                }
                else
                {
                    _rewardedAdList[i].LoadAd();
                }
            }
        }

        public virtual bool IsAdLoaded()
        {
            for (int i = 0; i < _rewardedAdList.Count; i++)
            {
                if (_rewardedAdList[i].IsAdLoaded())
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void LoadAd()
        {
            for (int i = 0; i < _rewardedAdList.Count; i++)
            {
                if (!_rewardedAdList[i].IsAdLoaded())
                {
                    _rewardedAdList[i].LoadAd();
                }
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
