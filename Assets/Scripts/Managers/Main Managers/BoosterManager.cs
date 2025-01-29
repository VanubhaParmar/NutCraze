using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class BoosterManager : SerializedManager<BoosterManager>
    {
        #region PRIVATE_VARS
        private List<Action<BoosterType>> onBoosterUse = new List<Action<BoosterType>>();
        #endregion

        #region PUBLIC_VARS
        #endregion

        #region PROPERTIES
        private GameplayView GameplayView => MainSceneUIManager.Instance.GetView<GameplayView>();
        private ShopView ShopView => MainSceneUIManager.Instance.GetView<ShopView>();
        private VFXView VFXView => MainSceneUIManager.Instance.GetView<VFXView>();
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        public void OnExtraScrewButtonClick()
        {
            if (CanUseExtraScrewBooster())
                UseExtraScrewBooster();
            else if (!DataManager.Instance.CanUseExtraScrewBooster())
            {
                if (AdManager.Instance.CanShowRewardedAd())
                    AdManager.Instance.ShowRewardedAd(OnExtraBoostersWatchAdSuccess, RewardAdShowCallType.Extra_Booster_Ad, AnalyticsConstants.GA_ExtraBoltRewardedBoosterAdPlace);
                else
                    ShopView.Show();
            }
            else
                ToastMessageView.Instance.ShowMessage(UserPromptMessageConstants.CantUseExtraBoltBoosterMessage);
        }

        public void OnUndoButtonClick()
        {
            if (CanUseUndoBooster())
                UseUndoBooster();
            else if (!DataManager.Instance.CanUseUndoBooster())
            {
                if (AdManager.Instance.CanShowRewardedAd())
                    AdManager.Instance.ShowRewardedAd(OnUndoBoostersWatchAdSuccess, RewardAdShowCallType.Undo_Booster_Ad, AnalyticsConstants.GA_UndoRewardedBoosterAdPlace);
                else
                    ShopView.Show();
            }
            else
                ToastMessageView.Instance.ShowMessage(UserPromptMessageConstants.CantUseUndoBoosterMessage);
        }

        public void RegisterOnBoosterUse(Action<BoosterType> action)
        {
            if (!onBoosterUse.Contains(action))
                onBoosterUse.Add(action);
        }

        public void DeRegisterOnBoosterUse(Action<BoosterType> action)
        {
            if (onBoosterUse.Contains(action))
                onBoosterUse.Remove(action);
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private void InvokeOnBoosterUse(BoosterType boosterType)
        {
            for (int i = 0; i < onBoosterUse.Count; i++)
                onBoosterUse[i]?.Invoke(boosterType);
        }

        private bool CanUseUndoBooster()
        {
            GameplayStateData gameplayStateData = GameplayManager.Instance.GameplayStateData;
            return DataManager.Instance.CanUseUndoBooster() && gameplayStateData.gameplayMoveInfos.Count > 0;
        }

        private bool CanUseExtraScrewBooster()
        {
            var boosterActivatedScrew = LevelManager.Instance.LevelScrews.Find(x => x is BoosterActivatedScrew) as BoosterActivatedScrew;

            if (boosterActivatedScrew == null)
                return false;

            return DataManager.Instance.CanUseExtraScrewBooster() && boosterActivatedScrew.CanExtendScrew();
        }

        private void OnExtraBoostersWatchAdSuccess()
        {
            GameManager.Instance.AddWatchAdRewardExtraScrewBoosters();
            GameplayView.SetView();

            FireBoosterAdWatchEvent(RewardAdShowCallType.Extra_Booster_Ad);
            Vector3 startPos = GameplayView.ExtraScrewBoosterParent.position;
            VFXView.PlayBoosterClaimAnimation(BoosterType.EXTRA_BOLT, GameManager.Instance.GameMainDataSO.extraScrewBoostersCountToAddOnAdWatch, startPos);
        }

        private void FireBoosterAdWatchEvent(RewardAdShowCallType rewardAdShowCallType)
        {
            string boosterName = rewardAdShowCallType == RewardAdShowCallType.Undo_Booster_Ad ? AnalyticsConstants.AdsData_UndoBoosterName : AnalyticsConstants.AdsData_ExtraBoltBoosterName;
            AnalyticsManager.Instance.LogAdsDataEvent(boosterName);
        }

        private void UseExtraScrewBooster()
        {
            var boosterActivatedScrew = LevelManager.Instance.LevelScrews.Find(x => x is BoosterActivatedScrew) as BoosterActivatedScrew;

            if (boosterActivatedScrew != null && boosterActivatedScrew.CanExtendScrew())
            {
                var playerData = PlayerPersistantData.GetMainPlayerProgressData();
                playerData.extraScrewBoostersCount = Mathf.Max(playerData.extraScrewBoostersCount - 1, 0);
                PlayerPersistantData.SetMainPlayerProgressData(playerData);
                GameplayLevelProgressManager.Instance.OnBoosterScrewStateUpgrade();
                boosterActivatedScrew.ExtendScrew();
                GameplayManager.Instance.CalculatePossibleNumberOfMoves();
                InvokeOnBoosterUse(BoosterType.EXTRA_BOLT);
            }
        }

        private void OnUndoBoostersWatchAdSuccess()
        {
            GameManager.Instance.AddWatchAdRewardUndoBoosters();
            MainSceneUIManager.Instance.GetView<GameplayView>().SetView();
            FireBoosterAdWatchEvent(RewardAdShowCallType.Undo_Booster_Ad);
            Vector3 startpos = GameplayView.UndoBoosterParent.position;
            MainSceneUIManager.Instance.GetView<VFXView>().PlayBoosterClaimAnimation(BoosterType.UNDO, GameManager.Instance.GameMainDataSO.undoBoostersCountToAddOnAdWatch, startpos);
        }

        private void UseUndoBooster()
        {
            var playerData = PlayerPersistantData.GetMainPlayerProgressData();
            playerData.undoBoostersCount = Mathf.Max(playerData.undoBoostersCount - 1, 0);
            PlayerPersistantData.SetMainPlayerProgressData(playerData);

            GameplayManager.Instance.ResetToLastMovedScrew(out var lastMoveState);

            BaseScrew currentSelectedScrew = GameplayManager.Instance.CurrentSelectedScrew;

            bool isSortedScrew = GameplayManager.Instance.IsScrewSortCompleted(currentSelectedScrew);
            if (isSortedScrew) // Reset all data when undoing sorted screw
            {
                DOTween.Kill(lastMoveState.moveToScrew); // kill all tweens on target screw and reset cap
                currentSelectedScrew.ScrewTopRenderer.gameObject.SetActive(false);
                currentSelectedScrew.SetScrewInteractableState(ScrewInteractibilityState.Interactable);
                currentSelectedScrew.StopStackFullIdlePS();

                NutsHolderScrewBehaviour currentSelectedScrewNutsHolder = currentSelectedScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();
                int firstNutColorId = currentSelectedScrewNutsHolder.PeekNut().GetNutColorType();
                GameplayManager.Instance.GameplayStateData.levelNutsUniqueColorsSortCompletionState[firstNutColorId] = false;
            }

            GameplayLevelProgressManager.Instance.OnUndoBoosterUsed();
            RetransferNutFromCurrentSelectedScrewTo(LevelManager.Instance.GetScrewOfGridCell(lastMoveState.moveFromScrew), lastMoveState.transferredNumberOfNuts);

            GameplayManager.Instance.CalculatePossibleNumberOfMoves();

            InvokeOnBoosterUse(BoosterType.UNDO);
        }


        private void RetransferNutFromCurrentSelectedScrewTo(BaseScrew baseScrew, int nutsCountToTransfer)
        {
            BaseScrew currentSelectedScrew = GameplayManager.Instance.CurrentSelectedScrew;
            NutsHolderScrewBehaviour currentSelectedScrewNutsHolder = currentSelectedScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();
            NutsHolderScrewBehaviour targetScrewNutsHolder = baseScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();

            BaseNut lastNut = currentSelectedScrewNutsHolder.PopNut();
            targetScrewNutsHolder.AddNut(lastNut, false);

            MainGameplayAnimator nutSelectionGameplayAnimator = GameplayManager.Instance.GetGameplayAnimator<MainGameplayAnimator>(); // Transfer target nut first
            nutSelectionGameplayAnimator.TransferThisNutFromStartScrewTopToEndScrew(lastNut, currentSelectedScrew, baseScrew);

            int extraNutIndex = 0;
            nutsCountToTransfer--;

            while (nutsCountToTransfer > 0)
            {
                BaseNut extraNut = currentSelectedScrewNutsHolder.PopNut();
                targetScrewNutsHolder.AddNut(extraNut, false);

                nutSelectionGameplayAnimator.TransferThisNutFromStartScrewToEndScrew(extraNut, extraNutIndex, currentSelectedScrew, baseScrew); // Transfer all other nuts
                extraNutIndex++;
                nutsCountToTransfer--;
            }

            GameplayManager.Instance.ResetCurrentSelectedScrew();
        }
        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region EDITOR
#if UNITY_EDITOR
#endif
        #endregion
    }
}
