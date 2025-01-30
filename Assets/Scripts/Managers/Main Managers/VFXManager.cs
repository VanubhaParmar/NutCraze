using BuildReportTool.Window.Screen;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class VFXManager : SerializedManager<VFXManager>
    {
        #region PRIVATE_VARS
        [SerializeField] private List<BaseGameplayAnimator> gameplayAnimators = new List<BaseGameplayAnimator>();
        #endregion

        #region PUBLIC_VARS
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            Init();
        }
        #endregion

        #region PUBLIC_FUNCTIONS
        public void Init()
        {
            gameplayAnimators.ForEach(x => x.InitGameplayAnimator());
        }
        public T GetGameplayAnimator<T>() where T : BaseGameplayAnimator
        {
            return gameplayAnimators.Find(x => x is T) as T;
        }

        public void PlayLevelCompleteAnimation(Action onComplete)
        {
            GetGameplayAnimator<MainGameplayAnimator>().PlayLevelCompleteAnimation(onComplete);
        }

        public void TransferThisNutFromStartScrewTopToEndScrew(BaseNut nutToTransfer, BaseScrew startScrew, BaseScrew endScrew)
        {
            GetGameplayAnimator<MainGameplayAnimator>().TransferThisNutFromStartScrewTopToEndScrew(nutToTransfer, startScrew, endScrew);
        }

        public void TransferThisNutFromStartScrewToEndScrew(BaseNut nutToTransfer, int startNutIndex, BaseScrew startScrew, BaseScrew endScrew)
        {
            GetGameplayAnimator<MainGameplayAnimator>().TransferThisNutFromStartScrewToEndScrew(nutToTransfer, startNutIndex, startScrew, endScrew);
        }

        public void PlayRevealAnimationOnNut(SurpriseColorNut surpriseNextNut)
        {
            GetGameplayAnimator<MainGameplayAnimator>().PlayRevealAnimationOnNut(surpriseNextNut);
        }

        public void PlayScrewSortCompletion(BaseScrew screw)
        {
            GetGameplayAnimator<MainGameplayAnimator>().PlayScrewSortCompletion(screw);
        }

        public void LiftTheFirstSelectionNut(BaseScrew baseScrew)
        {
            GetGameplayAnimator<MainGameplayAnimator>().LiftTheFirstSelectionNut(baseScrew);
        }

        public void ResetTheFirstSelectionNut(BaseScrew baseScrew)
        {
            GetGameplayAnimator<MainGameplayAnimator>().ResetTheFirstSelectionNut(baseScrew);
        }

        public void PlayLevelLoadAnimation(Action onComplete)
        {
            GetGameplayAnimator<MainGameplayAnimator>().PlayLevelLoadAnimation(onComplete);
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

        #region EDITOR
#if UNITY_EDITOR
#endif
        #endregion
    }
}
