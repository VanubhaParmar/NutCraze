using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.NutSort
{
    public class VFXManager : Manager<VFXManager>
    {
        #region PRIVATE_VARS
        [Header("Nut Raise Animation Data")]
        [SerializeField] private float screwSelectionNutRaiseHeight;
        [SerializeField] private float nutRaiseTime;
        [SerializeField] private float rotationTimeTakenForSingleRotation = 0.1f;

        [Header("Nut Transfer Animation Data")]
        [SerializeField] private float nutChangeScrewLaneTime;
        [SerializeField] private float nutRaiseTimePerHeight;
        [SerializeField] private float nutRaiseExtraTimePerHeight;

        [Header("Level Load Animation Data")]
        [SerializeField] private float nutScaleInAnimationTime;
        [SerializeField] private float delayBetweenTwoNutsInAnimations;

        #endregion

        #region PUBLIC_VARS
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        public void PlayLevelCompleteAnimation(Action actionToCallOnAnimationDone = null)
        {
            Sequence tweenSeq = DOTween.Sequence().SetId(LevelManager.Instance.LevelMainParent.transform);
            tweenSeq.AppendCallback(() => // Play Particle system
            {
                GameObject psObject = ObjectPool.Instance.Spawn(ResourceManager.BigConfettiPsPrefab, LevelManager.Instance.LevelMainParent);
                psObject.transform.position = Vector3.zero;
                psObject.transform.localEulerAngles = new Vector3(-60, 0, 0);
                psObject.gameObject.SetActive(true);

                Vibrator.HeavyFeedback();
                SoundHandler.Instance.PlaySound(SoundType.LevelComplete);
            });
            tweenSeq.AppendInterval(1f);
            tweenSeq.AppendCallback(() => actionToCallOnAnimationDone?.Invoke());

            var lastGameplayMove = GameplayManager.Instance.GameplayStateData.PeekLastGameplayMove();
            if (lastGameplayMove != null)
            {
                var toScrew = LevelManager.Instance.GetScrewOfGridCell(lastGameplayMove.moveToScrew);
                if (toScrew != null)
                {
                    List<Tween> nutsRunningTweens = DOTween.TweensById(toScrew.transform);
                    if (nutsRunningTweens != null && nutsRunningTweens.Count > 0)
                    {
                        LevelManager.Instance.LevelMainParent.transform.DOPause();
                        nutsRunningTweens.First().onComplete += () => LevelManager.Instance.LevelMainParent.transform.DOPlay();
                    }
                }
            }
        }

        public void PlayLevelLoadAnimation(Action actionToCallOnAnimationDone = null)
        {
            List<BaseScrew> allScrews = LevelManager.Instance.LevelScrews;

            int totalTweens = 0;
            for (int i = 0; i < allScrews.Count; i++)
            {
                if (allScrews[i].ScrewState == ScrewState.Interactable)
                {
                    BaseScrew screw = allScrews[i];
                    if (screw != null && !screw.IsEmpty)
                    {
                        Sequence tweenSeq = DOTween.Sequence().SetId(allScrews[i].transform);
                        float totalDelay = 0f;

                        for (int j = screw.CurrentNutCount - 1; j >= 0; j--)
                        {
                            totalTweens++;

                            BaseNut screwNut = screw.PeekNut(j);
                            BaseScrew targetScrew = allScrews[i];

                            screwNut.transform.localScale = Vector3.zero;
                            screwNut.transform.position = targetScrew.GetTopPosition();
                            float verticleMoveDistance = 1f;
                            Vector3 tweenTargetEndPosition = screw.GetMyNutPosition(screwNut);
                            float totalNutTargetPositionFitTime = (screw.MaxNutCapacity - screw.GetNutIndex(screwNut)) * nutRaiseTimePerHeight;

                            float totalNumberOfRotation = Mathf.Ceil(totalNutTargetPositionFitTime / rotationTimeTakenForSingleRotation);
                            SoundInstance nutRotateSound = null;

                            Action nutResetAction = delegate
                            {
                                screwNut.transform.localEulerAngles = Vector3.zero;
                                screwNut.transform.localScale = Vector3.one;
                                screwNut.transform.position = tweenTargetEndPosition;

                                totalTweens--;
                                if (totalTweens == 0)
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

        public void TransferThisNutFromStartScrewTopToEndScrew(BaseNut nutToTransfer, BaseScrew fromScrew, BaseScrew endScrew)
        {
            Vector3 tweenTargetMidPosition = GetScrewSelectionNutRaiseFinalPosition(endScrew);
            Vector3 tweenTargetEndPosition = endScrew.GetMyNutPosition(nutToTransfer);

            float totalNutTargetPositionFitTime = nutRaiseTime + (endScrew.MaxNutCapacity - endScrew.GetNutIndex(nutToTransfer)) * nutRaiseTimePerHeight;
            float totalNumberOfRotation = Mathf.Ceil(totalNutTargetPositionFitTime / rotationTimeTakenForSingleRotation);
            SoundInstance nutRotateSound = null;

            Action nutResetAction = delegate
            {
                nutToTransfer.transform.localEulerAngles = Vector3.zero;

                SoundHandler.Instance.PlaySound(SoundType.NutPlace);
                nutRotateSound?.Stop();
                nutToTransfer.PlaySparkParticle();
            };

            DOTween.Kill(nutToTransfer.transform);

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

            Vector3 tweenTargetStartPosition = GetScrewSelectionNutRaiseFinalPosition(startScrew);
            Vector3 tweenTargetMidPosition = GetScrewSelectionNutRaiseFinalPosition(endScrew);
            Vector3 tweenTargetEndPosition = endScrew.GetMyNutPosition(nutToTransfer);

            float totalNutMidPositionRaiseTime = nutRaiseTime + /*(startScrewNutsBehaviour.MaxNutCapacity -*/ (startNutIndex) * nutRaiseTimePerHeight;
            float totalNutTargetPositionFitTime = nutRaiseTime + (endScrew.MaxNutCapacity - endScrew.GetNutIndex(nutToTransfer)) * nutRaiseTimePerHeight;
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
        #endregion

        #region PRIVATE_FUNCTIONS
        private Vector3 GetScrewSelectionNutRaiseFinalPosition(BaseScrew baseScrew)
        {
            return baseScrew.GetTopPosition() + screwSelectionNutRaiseHeight * Vector3.up;
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
