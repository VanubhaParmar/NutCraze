using System;
using UnityEngine;

namespace Tag.NutSort
{
    [Serializable]
    public class UndoBooster : BaseBooster
    {
        public UndoBooster()
        {
            boosterId = BoosterIdConstant.UNDO;
            boosterName = AnalyticsConstants.AdsData_UndoBoosterName;
            cannotUseMessage = UserPromptMessageConstants.CantUseUndoBoosterMessage;
            rewardAdPlace = AnalyticsConstants.GA_UndoRewardedBoosterAdPlace;
            rewardAdType = RewardAdShowCallType.Undo_Booster_Ad;
        }

        public override bool HasBooster()
        {
            return DataManager.Instance.CanUseBooster(BoosterId);
        }

        public override bool CanUse()
        {
            GameplayStateData gameplayStateData = GameplayManager.Instance.GameplayStateData;
            return HasBooster() && gameplayStateData.gameplayMoveInfos.Count > 0;
        }

        public override int GetBoosterCount()
        {
            return DataManager.Instance.GetBoostersCount(BoosterId);
        }

        public override void Use()
        {
            DataManager.Instance.UseBooster(BoosterId);
            NutTransferHelper.Instance.ResetToLastMovedScrew();
            BoosterManager.Instance.InvokeOnBoosterUse(BoosterId);
        }

        public override void OnAdWatchSuccess()
        {
            DataManager.Instance.AddBoosters(BoosterId, boostersToAddOnAdWatch);
            var gameplayView = MainSceneUIManager.Instance.GetView<GameplayView>();
            gameplayView.SetView();
            FireBoosterAdWatchEvent();
            Vector3 startPos = gameplayView.UndoBoosterParent.position;
            MainSceneUIManager.Instance.GetView<VFXView>().PlayBoosterClaimAnimation(BoosterId, boostersToAddOnAdWatch, startPos);
        }

        public override void FireBoosterAdWatchEvent()
        {
            AnalyticsManager.Instance.LogAdsDataEvent(boosterName);
        }
    }
}
