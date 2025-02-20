using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "AdManagerDataSO", menuName = Constant.GAME_NAME + "/Managers/AdManagerDataSO")]
    public class AdManagerDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARIABLES
        public int showBannerAdsAfterLevel = 3;
        public int rewardsAdHoursInterval = 4;
        public int rewardsAdsCount = 20;

        public TimeDuration refreshRewardAdsCapLocalTime;

        [Space]
        public List<InterstitialAdConfigData> interstitialAdConfigDatas;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public AdConfigData GetDefaultAdConfigData()
        {
            AdConfigData adConfigData = new AdConfigData();
            adConfigData.showBannerAdsAfterLevel = showBannerAdsAfterLevel;
            adConfigData.rewardsAdHoursInterval = rewardsAdHoursInterval;
            adConfigData.rewardsAdsCount = rewardsAdsCount;

            adConfigData.interstitialAdConfigDatas = new List<InterstitialAdConfigData>();
            adConfigData.interstitialAdConfigDatas.AddRange(interstitialAdConfigDatas);

            return adConfigData;
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