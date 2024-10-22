using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.NutSort
{
    public class MainGameplayAnimator : BaseGameplayAnimator
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [Header("Nut Raise Animation Data")]
        [SerializeField] private float screwSelectionNutRaiseHeight;
        [SerializeField] private float nutRaiseTime;
        //[SerializeField] private int numberOfRotations = 4;
        [SerializeField] private float rotationTimeTakenForSingleRotation = 0.1f;
        [SerializeField] private AnimationCurve raiseAnimationCurve;

        [Header("Nut Transfer Animation Data")]
        [SerializeField] private float nutChangeScrewLaneTime;
        [SerializeField] private float nutRaiseTimePerHeight;
        [SerializeField] private float nutRaiseExtraTimePerHeight;

        [Header("Screw Sort Complete Animation Data")]
        [SerializeField] private float nutCapRaiseHeight;
        [SerializeField] private float nutCapPlaceTime;

        [Header("Level Load Animation Data")]
        [SerializeField] private float nutScaleInAnimationTime;
        [SerializeField] private float delayBetweenTwoNutsInAnimations;

        private AnimationCurveEase raiseAnimationCurveEaseFunction;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void InitGameplayAnimator()
        {
            base.InitGameplayAnimator();

            raiseAnimationCurveEaseFunction = new AnimationCurveEase(raiseAnimationCurve);
        }

        public void PlayLevelCompleteAnimation(Action actionToCallOnAnimationDone = null)
        {
            Sequence tweenSeq = DOTween.Sequence();
            tweenSeq.AppendInterval(0.5f);
            tweenSeq.AppendCallback(() => // Play Particle system
            {
                GameObject psObject = ObjectPool.Instance.Spawn(PrefabsHolder.Instance.BigConfettiPsPrefab, LevelManager.Instance.LevelMainParent);
                psObject.transform.position = Vector3.zero;
                psObject.transform.localEulerAngles = new Vector3(-60, 0, 0);
                psObject.gameObject.SetActive(true);

                Vibrator.Vibrate(Vibrator.hugeIntensity);
                SoundHandler.Instance.PlaySound(SoundType.LevelComplete);
            });
            tweenSeq.AppendInterval(3f);
            tweenSeq.AppendCallback(() => actionToCallOnAnimationDone?.Invoke());
        }

        public void PlayLevelLoadAnimation(Action actionToCallOnAnimationDone = null)
        {
            List<BaseScrew> allScrews = LevelManager.Instance.LevelScrews;

            int totalTweens = 0;
            for (int i = 0; i < allScrews.Count; i++)
            {
                if (allScrews[i].ScrewInteractibilityState == ScrewInteractibilityState.Interactable)
                {
                    NutsHolderScrewBehaviour screwNutsBehaviour = allScrews[i].GetScrewBehaviour<NutsHolderScrewBehaviour>();
                    if (screwNutsBehaviour != null && !screwNutsBehaviour.IsEmpty)
                    {
                        Sequence tweenSeq = DOTween.Sequence().SetId(allScrews[i].transform);
                        float totalDelay = 0f;

                        for (int j = screwNutsBehaviour.CurrentNutCount - 1; j >= 0; j--)
                        {
                            totalTweens++;

                            BaseNut screwNut = screwNutsBehaviour.PeekNut(j);
                            BaseScrew targetScrew = allScrews[i];

                            screwNut.transform.localScale = Vector3.zero;
                            screwNut.transform.position = targetScrew.GetTopPosition();
                            float verticleMoveDistance = 1f;
                            Vector3 tweenTargetEndPosition = screwNutsBehaviour.GetMyNutPosition(screwNut);
                            float totalNutTargetPositionFitTime = (screwNutsBehaviour.MaxNutCapacity - screwNutsBehaviour.GetNutIndex(screwNut)) * nutRaiseTimePerHeight;

                            float totalNumberOfRotation = Mathf.Ceil(totalNutTargetPositionFitTime / rotationTimeTakenForSingleRotation);
                            SoundInstance nutRotateSound = null;

                            Action nutResetAction = delegate
                            {
                                screwNut.transform.localEulerAngles = Vector3.zero;
                                screwNut.transform.localScale = Vector3.one;
                                screwNut.transform.position = tweenTargetEndPosition;

                                totalTweens--;
                                if (totalTweens <= 0)
                                    actionToCallOnAnimationDone?.Invoke();

                                SoundHandler.Instance.PlaySound(SoundType.NutPlace);
                                nutRotateSound?.Stop();
                            };

                            tweenSeq.Insert(totalDelay, screwNut.transform.DOScale(Vector3.one, nutScaleInAnimationTime));
                            tweenSeq.Insert(totalDelay, screwNut.transform.DOMove(targetScrew.GetTopPosition() + Vector3.up * verticleMoveDistance, nutScaleInAnimationTime * 0.5f).SetEase(Ease.InQuad));
                            tweenSeq.Insert(totalDelay + nutScaleInAnimationTime * 0.5f, screwNut.transform.DOMove(targetScrew.GetTopPosition(), nutScaleInAnimationTime * 0.5f).SetEase(Ease.OutQuad));

                            tweenSeq.InsertCallback(totalDelay + nutScaleInAnimationTime, () => { nutRotateSound = SoundHandler.Instance.PlaySoundWithNewInstance(SoundType.NutRotate); });

                            tweenSeq.Insert(totalDelay + nutScaleInAnimationTime, screwNut.transform.DOMove(tweenTargetEndPosition, totalNutTargetPositionFitTime).SetEase(Ease.Linear));
                            tweenSeq.Insert(totalDelay + nutScaleInAnimationTime, screwNut.transform.DORotate(Vector3.up * (totalNumberOfRotation * 360), totalNutTargetPositionFitTime, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetRelative(true));
                            tweenSeq.onComplete += nutResetAction.Invoke;
                            tweenSeq.onKill += nutResetAction.Invoke;

                            totalDelay += delayBetweenTwoNutsInAnimations;
                        }
                    }
                }
            }
        }

        public void OnPlayScrewSortCompletion(BaseScrew startScrew)
        {
            NutsHolderScrewBehaviour startScrewNutsBehaviour = startScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();

            Vector3 tweenTargetMidPosition = GetScrewCapRaiseFinalPosition(startScrew);
            MeshRenderer screwCap = startScrew.ScrewTopRenderer;

            screwCap.transform.position = tweenTargetMidPosition;
            screwCap.gameObject.SetActive(true);
            screwCap.transform.localScale = Vector3.zero;

            Action screwCapResetAction = delegate
            {
                screwCap.transform.position = startScrew.GetScrewCapPosition();
                screwCap.transform.localScale = Vector3.one * startScrew.ScrewDimensions.screwCapScale;
            };

            Sequence tweenSeq = DOTween.Sequence().SetId(startScrew.transform);
            tweenSeq.AppendCallback(() => // Play Particle system
            {
                //startScrew.PlayStackFullParticlesByID(startScrewNutsBehaviour.PeekNut().GetNutColorType());
                startScrew.PlayStackFullPS();

                //GameObject psObject = ObjectPool.Instance.Spawn(PrefabsHolder.Instance.SmallConfettiPsPrefab, LevelManager.Instance.LevelMainParent);
                //psObject.transform.position = startScrew.transform.position + new Vector3(0, -0.2f, -1f);
                //psObject.gameObject.SetActive(true);

                Vibrator.Vibrate(Vibrator.averageIntensity);
                SoundHandler.Instance.PlaySound(SoundType.ScrewSorted);
            });
            tweenSeq.Append(screwCap.transform.DOScale(Vector3.one * startScrew.ScrewDimensions.screwCapScale, nutCapPlaceTime * 0.5f));
            tweenSeq.Append(screwCap.transform.DOMove(startScrew.GetScrewCapPosition(), nutCapPlaceTime).SetEase(raiseAnimationCurveEaseFunction.EaseFunction));
            tweenSeq.onComplete += screwCapResetAction.Invoke;
            tweenSeq.onKill += screwCapResetAction.Invoke;
            BaseNut lastNutInAnimation = startScrewNutsBehaviour.PeekNut();
            List<Tween> runningTweens = DOTween.TweensById(lastNutInAnimation.transform);
            if (runningTweens != null && runningTweens.Count > 0)
            {
                startScrew.transform.DOPause();
                runningTweens.First().onComplete += () => startScrew.transform.DOPlay();
            }
        }

        public void TransferThisNutFromStartScrewTopToEndScrew(BaseNut nutToTransfer, BaseScrew startScrew, BaseScrew endScrew)
        {
            NutsHolderScrewBehaviour startScrewNutsBehaviour = startScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();
            NutsHolderScrewBehaviour endScrewNutsBehaviour = endScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();

            Vector3 tweenTargetMidPosition = GetScrewSelectionNutRaiseFinalPosition(endScrew);
            Vector3 tweenTargetEndPosition = endScrewNutsBehaviour.GetMyNutPosition(nutToTransfer);

            float totalNutTargetPositionFitTime = nutRaiseTime + (endScrewNutsBehaviour.MaxNutCapacity - endScrewNutsBehaviour.GetNutIndex(nutToTransfer)) * nutRaiseTimePerHeight;
            float totalNumberOfRotation = Mathf.Ceil(totalNutTargetPositionFitTime / rotationTimeTakenForSingleRotation);
            SoundInstance nutRotateSound = null;

            Action nutResetAction = delegate
            {
                nutToTransfer.transform.localEulerAngles = Vector3.zero;

                SoundHandler.Instance.PlaySound(SoundType.NutPlace);
                nutRotateSound?.Stop();
                nutToTransfer.PlaySparkParticle();
            };

            Sequence tweenSeq = DOTween.Sequence().SetId(nutToTransfer.transform);
            tweenSeq.Append(nutToTransfer.transform.DOMove(tweenTargetMidPosition, nutChangeScrewLaneTime).SetEase(Ease.Linear));
            tweenSeq.AppendCallback(() => { nutRotateSound = SoundHandler.Instance.PlaySoundWithNewInstance(SoundType.NutRotate); });
            tweenSeq.Append(nutToTransfer.transform.DOMove(tweenTargetEndPosition, totalNutTargetPositionFitTime).SetEase(Ease.Linear));
            tweenSeq.Join(nutToTransfer.transform.DORotate(Vector3.up * (totalNumberOfRotation * 360), totalNutTargetPositionFitTime, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetRelative(true));
            tweenSeq.onComplete += nutResetAction.Invoke;
            tweenSeq.onKill += nutResetAction.Invoke;
        }

        public void TransferThisNutFromStartScrewToEndScrew(BaseNut nutToTransfer, int startNutIndex, BaseScrew startScrew, BaseScrew endScrew)
        {
            NutsHolderScrewBehaviour startScrewNutsBehaviour = startScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();
            NutsHolderScrewBehaviour endScrewNutsBehaviour = endScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();

            Vector3 tweenTargetStartPosition = GetScrewSelectionNutRaiseFinalPosition(startScrew);
            Vector3 tweenTargetMidPosition = GetScrewSelectionNutRaiseFinalPosition(endScrew);
            Vector3 tweenTargetEndPosition = endScrewNutsBehaviour.GetMyNutPosition(nutToTransfer);

            float totalNutMidPositionRaiseTime = nutRaiseTime + /*(startScrewNutsBehaviour.MaxNutCapacity -*/ (startNutIndex) * nutRaiseTimePerHeight;
            float totalNutTargetPositionFitTime = nutRaiseTime + (endScrewNutsBehaviour.MaxNutCapacity - endScrewNutsBehaviour.GetNutIndex(nutToTransfer)) * nutRaiseTimePerHeight;
            float totalNumberOfRotation = Mathf.Ceil(totalNutTargetPositionFitTime / rotationTimeTakenForSingleRotation);

            SoundInstance nutRotateSound = null;

            Action nutResetAction = delegate
            {
                nutToTransfer.transform.localEulerAngles = Vector3.zero;
                SoundHandler.Instance.PlaySound(SoundType.NutPlace);
                nutRotateSound?.Stop();
                nutToTransfer.PlaySparkParticle();
            };

            Sequence tweenSeq = DOTween.Sequence().SetId(nutToTransfer.transform);
            tweenSeq.AppendInterval(/*(startScrewNutsBehaviour.MaxNutCapacity - */(startNutIndex) * nutRaiseExtraTimePerHeight);
            tweenSeq.AppendCallback(() => { nutRotateSound = SoundHandler.Instance.PlaySoundWithNewInstance(SoundType.NutRotate); });
            tweenSeq.Append(nutToTransfer.transform.DOMove(tweenTargetStartPosition, totalNutMidPositionRaiseTime).SetEase(Ease.Linear));
            tweenSeq.Join(nutToTransfer.transform.DORotate(Vector3.down * (totalNumberOfRotation * 360), totalNutMidPositionRaiseTime, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetRelative(true));
            tweenSeq.AppendCallback(() => { nutRotateSound?.Stop(); });
            tweenSeq.Append(nutToTransfer.transform.DOMove(tweenTargetMidPosition, nutChangeScrewLaneTime).SetEase(Ease.Linear));
            tweenSeq.AppendCallback(() => { nutRotateSound?.Play(); });
            tweenSeq.Append(nutToTransfer.transform.DOMove(tweenTargetEndPosition, totalNutTargetPositionFitTime).SetEase(Ease.Linear));
            tweenSeq.Join(nutToTransfer.transform.DORotate(Vector3.up * (totalNumberOfRotation * 360), totalNutTargetPositionFitTime, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetRelative(true));
            tweenSeq.onComplete += nutResetAction.Invoke;
            tweenSeq.onKill += nutResetAction.Invoke;
        }

        public void LiftTheFirstSelectionNut(BaseScrew baseScrew)
        {
            if (baseScrew.TryGetScrewBehaviour(out NutsHolderScrewBehaviour nutsHolderScrewBehaviour))
            {
                BaseNut selectionNut = nutsHolderScrewBehaviour.PeekNut();

                Vector3 tweenTargetPosition = GetScrewSelectionNutRaiseFinalPosition(baseScrew);
                float totalNumberOfRotation = Mathf.Ceil(nutRaiseTime / rotationTimeTakenForSingleRotation);
                SoundInstance nutRotateSound = null;

                Action nutResetAction = delegate
                {
                    selectionNut.transform.localEulerAngles = Vector3.zero;

                    nutRotateSound?.Stop();
                };

                DOTween.Kill(selectionNut.transform);

                Sequence tweenSeq = DOTween.Sequence().SetId(selectionNut.transform);
                tweenSeq.AppendCallback(() => { nutRotateSound = SoundHandler.Instance.PlaySoundWithNewInstance(SoundType.NutRotate); });
                tweenSeq.Append(selectionNut.transform.DOMove(tweenTargetPosition, nutRaiseTime).SetEase(raiseAnimationCurveEaseFunction.EaseFunction));
                tweenSeq.Join(selectionNut.transform.DORotate(Vector3.down * (totalNumberOfRotation * 360), nutRaiseTime, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetRelative(true));
                tweenSeq.onComplete += nutResetAction.Invoke;
                tweenSeq.onKill += nutResetAction.Invoke;

                SoundHandler.Instance.PlaySound(SoundType.NutSelect);
            }
        }

        public void ResetTheFirstSelectionNut(BaseScrew baseScrew)
        {
            if (baseScrew.TryGetScrewBehaviour(out NutsHolderScrewBehaviour nutsHolderScrewBehaviour))
            {
                BaseNut selectionNut = nutsHolderScrewBehaviour.PeekNut();

                Vector3 tweenTargetPosition = nutsHolderScrewBehaviour.GetTopScrewPosition();
                float totalNumberOfRotation = Mathf.Ceil(nutRaiseTime / rotationTimeTakenForSingleRotation);
                SoundInstance nutRotateSound = null;

                Action nutResetAction = delegate
                {
                    selectionNut.transform.localEulerAngles = Vector3.zero;

                    SoundHandler.Instance.PlaySound(SoundType.NutPlace);
                    nutRotateSound?.Stop();
                };

                Sequence tweenSeq = DOTween.Sequence().SetId(selectionNut.transform);
                tweenSeq.AppendCallback(() => { nutRotateSound = SoundHandler.Instance.PlaySoundWithNewInstance(SoundType.NutRotate); });
                tweenSeq.Append(selectionNut.transform.DOMove(tweenTargetPosition, nutRaiseTime).SetEase(raiseAnimationCurveEaseFunction.EaseFunction));
                tweenSeq.Join(selectionNut.transform.DORotate(Vector3.up * (totalNumberOfRotation * 360), nutRaiseTime, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetRelative(true));
                tweenSeq.onComplete += nutResetAction.Invoke;
                tweenSeq.onKill += nutResetAction.Invoke;
            }
        }
        #endregion

        #region PRIVATE_METHODS
        private Vector3 GetScrewSelectionNutRaiseFinalPosition(BaseScrew baseScrew)
        {
            return baseScrew.GetTopPosition() + screwSelectionNutRaiseHeight * Vector3.up;
        }

        private Vector3 GetScrewCapRaiseFinalPosition(BaseScrew baseScrew)
        {
            return baseScrew.GetScrewCapPosition() + nutCapRaiseHeight * Vector3.up;
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}