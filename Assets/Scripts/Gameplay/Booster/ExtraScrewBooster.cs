using System;
using UnityEngine;

namespace Tag.NutSort
{
    [Serializable]
    public class ExtraScrewBooster : BaseBooster
    {
        public ExtraScrewBooster()
        {
            boosterId = BoosterIdConstant.EXTRA_SCREW;
            boosterName = AnalyticsConstants.AdsData_ExtraBoltBoosterName;
            cannotUseMessage = UserPromptMessageConstants.CantUseExtraBoltBoosterMessage;
            rewardAdPlace = AnalyticsConstants.GA_ExtraBoltRewardedBoosterAdPlace;
            rewardAdType = RewardAdShowCallType.Extra_Booster_Ad;
        }

        public override bool HasBooster()
        {
            return DataManager.Instance.CanUseBooster(BoosterId);
        }

        public override bool CanUse()
        {
            var boosterActivatedScrew = LevelManager.Instance.LevelScrews.Find(x => x is BoosterActivatedScrew) as BoosterActivatedScrew;

            if (boosterActivatedScrew == null)
                return false;

            return HasBooster() && boosterActivatedScrew.CanExtendScrew();
        }

        public override int GetBoosterCount()
        {
            return DataManager.Instance.GetBoostersCount(boosterId);
        }

        public override void Use()
        {
            var boosterActivatedScrew = LevelManager.Instance.LevelScrews.Find(x => x is BoosterActivatedScrew) as BoosterActivatedScrew;

            if (boosterActivatedScrew != null && boosterActivatedScrew.CanExtendScrew())
            {
                DataManager.Instance.UseBooster(boosterId);
                boosterActivatedScrew.ExtendScrew(true);
                GameplayManager.Instance.GameplayStateData.CalculatePossibleNumberOfMoves();
                BoosterManager.Instance.InvokeOnBoosterUse(BoosterId);
            }
        }

        public override void OnAdWatchSuccess()
        {
            GameplayLevelProgressManager.Instance.OnWatchAdSuccess();
            DataManager.Instance.AddBoosters(BoosterId, boostersToAddOnAdWatch);
            var gameplayView = MainSceneUIManager.Instance.GetView<GameplayView>();
            gameplayView.SetView();
            FireBoosterAdWatchEvent();
            Vector3 startPos = gameplayView.ExtraScrewBoosterParent.position;
            MainSceneUIManager.Instance.GetView<VFXView>().PlayBoosterClaimAnimation(BoosterId, boostersToAddOnAdWatch, startPos);
        }

        public override void FireBoosterAdWatchEvent()
        {
            AnalyticsManager.Instance.LogAdsDataEvent(boosterName);
        }


    }
}
