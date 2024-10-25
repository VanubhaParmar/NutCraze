using System;
using System.Collections;
using System.Collections.Generic;
using Tag.Ad;
using UnityEngine;

namespace Tag
{
    public class RewardedAdHandlerApplovinMax : BaseRewardedAdHandler
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public override void Init()
        {
            base.Init();
            Debug.Log("<APPLOVIN MAX> RewardedAd Init");
            _rewardedAdList = new List<BaseRewardedAd>();
            for (int i = 0; i < noOfAdLoad; i++)
            {
                RewardedAdApplovinMax rewardedAd = new RewardedAdApplovinMax();
                rewardedAd.Init();
                _rewardedAdList.Add(rewardedAd);
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
