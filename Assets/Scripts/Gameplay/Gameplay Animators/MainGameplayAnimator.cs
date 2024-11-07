using DG.Tweening;
using Sirenix.OdinInspector;
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

        [Header("Selected Nut Hover Animation Data")]
        [SerializeField] private float hoverAnimationDuration;
        [SerializeField] private Vector3 startRotation;
        [SerializeField] private Vector3 maxRotation;
        [SerializeField] private AnimationCurve xRotationCurve;
        [SerializeField] private AnimationCurve zRotationCurve;

        private Coroutine nutHoverCoroutine;

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
            Sequence tweenSeq = DOTween.Sequence().SetId(LevelManager.Instance.LevelMainParent.transform);
            //tweenSeq.AppendInterval(0.5f);
            tweenSeq.AppendCallback(() => // Play Particle system
            {
                GameObject psObject = ObjectPool.Instance.Spawn(PrefabsHolder.Instance.BigConfettiPsPrefab, LevelManager.Instance.LevelMainParent);
                psObject.transform.position = Vector3.zero;
                psObject.transform.localEulerAngles = new Vector3(-60, 0, 0);
                psObject.gameObject.SetActive(true);

                Vibrator.Vibrate(Vibrator.hugeIntensity);
                SoundHandler.Instance.PlaySound(SoundType.LevelComplete);
            });
            tweenSeq.AppendInterval(2f);
            tweenSeq.AppendCallback(() => actionToCallOnAnimationDone?.Invoke());

            var allScrews = LevelManager.Instance.LevelScrews;
            foreach (var screw in allScrews)
            {
                NutsHolderScrewBehaviour startScrewNutsBehaviour = screw.GetScrewBehaviour<NutsHolderScrewBehaviour>();
                if (startScrewNutsBehaviour == null || startScrewNutsBehaviour.IsEmpty)
                    continue;

                List<Tween> nutsRunningTweens = DOTween.TweensById(startScrewNutsBehaviour.PeekNut().transform);
                if (nutsRunningTweens != null && nutsRunningTweens.Count > 0)
                {
                    LevelManager.Instance.LevelMainParent.transform.DOPause();
                    nutsRunningTweens.First().onComplete += () => LevelManager.Instance.LevelMainParent.transform.DOPlay();
                    break;
                }
            }
        }

        public void PlayRevealAnimationOnNut(SurpriseColorNut surpriseNextNut)
        {
            Sequence tweenAnimSeq = DOTween.Sequence().SetId(surpriseNextNut.transform);
            tweenAnimSeq.Append(surpriseNextNut.transform.DoScaleWithReverseOvershoot(Vector3.one * 0.7f, 0.3f, 0.2f, 0.1f).SetEase(Ease.Linear));
            tweenAnimSeq.AppendCallback(() =>
            {
                surpriseNextNut.OnRevealColorOfNut();
            });
            tweenAnimSeq.Append(surpriseNextNut.transform.DoScaleWithOvershoot(Vector3.zero, Vector3.one, 0.25f, 0.2f, 0.1f).SetEase(Ease.Linear));
            tweenAnimSeq.onComplete += () =>
            {
                surpriseNextNut.transform.localScale = Vector3.one;
            };
            tweenAnimSeq.onKill += () =>
            {
                surpriseNextNut.OnRevealColorOfNut();
                surpriseNextNut.transform.localScale = Vector3.one;
            };
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
                startScrew.PlayStackFullParticlesByID(startScrewNutsBehaviour.PeekNut().GetNutColorType());
                startScrew.PlayStackFullIdlePS();
                //GameObject psObject = ObjectPool.Instance.Spawn(PrefabsHolder.Instance.SmallConfettiPsPrefab, LevelManager.Instance.LevelMainParent);
                //psObject.transform.position = startScrew.transform.position + new Vector3(0, -0.2f, -1f);
                //psObject.gameObject.SetActive(true);

                Vibrator.Vibrate(Vibrator.averageIntensity);
                SoundHandler.Instance.PlaySound(SoundType.ScrewSorted);
                GameManager.Instance.MainCameraShakeAnimation.DoShake();
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
                    //StartNutHoverAnimation(selectionNut);
                    nutRotateSound?.Stop();
                };

                DOTween.Kill(selectionNut.transform);
                selectionNut.transform.localScale = Vector3.one;
                Sequence tweenSeq = DOTween.Sequence().SetId(selectionNut.transform);
                tweenSeq.AppendCallback(() => { nutRotateSound = SoundHandler.Instance.PlaySoundWithNewInstance(SoundType.NutRotate); });
                tweenSeq.Append(selectionNut.transform.DOMove(tweenTargetPosition, nutRaiseTime).SetEase(raiseAnimationCurveEaseFunction.EaseFunction));
                tweenSeq.Join(selectionNut.transform.DORotate(Vector3.down * (totalNumberOfRotation * 360), nutRaiseTime, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetRelative(true));
                tweenSeq.AppendCallback(() => { PlayNutHoverAnimation(selectionNut); });

                tweenSeq.onComplete += nutResetAction.Invoke;
                tweenSeq.onKill += nutResetAction.Invoke;

                SoundHandler.Instance.PlaySound(SoundType.NutSelect);
            }
        }

        public void PlayNutHoverAnimation(BaseNut selectionNut)
        {
            Action nutResetAction = delegate
            {
                selectionNut.transform.localEulerAngles = Vector3.zero;
            };

            DOTween.Kill(selectionNut.transform);

            var xFloatAnimation = new FloatDotweenerAnimation(0f, maxRotation.x, hoverAnimationDuration, xRotationCurve);
            var zFloatAnimation = new FloatDotweenerAnimation(0f, maxRotation.z, hoverAnimationDuration, zRotationCurve);

            Sequence tweenSeq = DOTween.Sequence().SetId(selectionNut.transform);

            tweenSeq.Append(selectionNut.transform.DORotate(startRotation, 0.5f));
            tweenSeq.AppendCallback(() =>
            {
                var xTween = xFloatAnimation.StartDotweenAnimation(selectionNut.transform, (x) =>
                {
                    Vector3 currentEuler = selectionNut.transform.eulerAngles;
                    currentEuler.x = x;
                    selectionNut.transform.eulerAngles = currentEuler;
                }, true);

                var zTween = zFloatAnimation.StartDotweenAnimation(selectionNut.transform, (z) =>
                {
                    Vector3 currentEuler = selectionNut.transform.eulerAngles;
                    currentEuler.z = z;
                    selectionNut.transform.eulerAngles = currentEuler;
                }, true);

                xTween.onKill += nutResetAction.Invoke;
            });

            //tweenSeq.Append(xFloatAnimation.StartDotweenAnimation(selectionNut.transform, (x) => 
            //{
            //    Vector3 currentEuler = selectionNut.transform.eulerAngles;
            //    currentEuler.x = x;
            //    selectionNut.transform.eulerAngles = currentEuler;
            //}, true));

            //tweenSeq.Join(zFloatAnimation.StartDotweenAnimation(selectionNut.transform, (z) =>
            //{
            //    Vector3 currentEuler = selectionNut.transform.eulerAngles;
            //    currentEuler.z = z;
            //    selectionNut.transform.eulerAngles = currentEuler;
            //}, true));

            //tweenSeq.onComplete += nutResetAction.Invoke;
            //tweenSeq.onKill += nutResetAction.Invoke;
        }

        [Button]
        public void TOTest()
        {
            float i = 0f;
            Sequence tweenSeq = DOTween.Sequence().SetLoops(-1);
            tweenSeq.AppendCallback(() => { i = 0f; });
            tweenSeq.Append(DOTween.To(() => i, x => i = x, 1f, 4f).SetEase(Ease.Linear)).OnUpdate(() =>
            {
                Debug.Log("I : " + i);
            });
        }

        public void ResetTheFirstSelectionNut(BaseScrew baseScrew)
        {
            if (baseScrew.TryGetScrewBehaviour(out NutsHolderScrewBehaviour nutsHolderScrewBehaviour))
            {
                BaseNut selectionNut = nutsHolderScrewBehaviour.PeekNut();
                //StopNutHoverAnimation(selectionNut);
                Vector3 tweenTargetPosition = nutsHolderScrewBehaviour.GetTopScrewPosition();
                float totalNumberOfRotation = Mathf.Ceil(nutRaiseTime / rotationTimeTakenForSingleRotation);
                SoundInstance nutRotateSound = null;

                Action nutResetAction = delegate
                {
                    selectionNut.transform.localEulerAngles = Vector3.zero;

                    SoundHandler.Instance.PlaySound(SoundType.NutPlace);
                    nutRotateSound?.Stop();
                };

                DOTween.Kill(selectionNut.transform);

                Sequence tweenSeq = DOTween.Sequence().SetId(selectionNut.transform);
                tweenSeq.AppendCallback(() => { nutRotateSound = SoundHandler.Instance.PlaySoundWithNewInstance(SoundType.NutRotate); });
                tweenSeq.Append(selectionNut.transform.DOMove(tweenTargetPosition, nutRaiseTime).SetEase(raiseAnimationCurveEaseFunction.EaseFunction));
                tweenSeq.Join(selectionNut.transform.DORotate(Vector3.up * (totalNumberOfRotation * 360), nutRaiseTime, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetRelative(true));
                tweenSeq.onComplete += nutResetAction.Invoke;
                tweenSeq.onKill += nutResetAction.Invoke;
            }
        }

        //private void StartNutHoverAnimation(BaseNut selectedNut)
        //{
        //    if (nutHoverCoroutine != null)
        //        StopCoroutine(nutHoverCoroutine);
        //    nutHoverCoroutine = StartCoroutine(DoHoverAnimation(selectedNut));
        //}

        //private void StopNutHoverAnimation(BaseNut selectedNut)
        //{
        //    if (nutHoverCoroutine != null)
        //        StopCoroutine(nutHoverCoroutine);
        //    selectedNut.transform.rotation = Quaternion.identity;
        //}
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

        //IEnumerator DoHoverAnimation(BaseNut selectedNut)
        //{
        //    float i = 0;
        //    float rate = 1 / hoverAnimationDiration;

        //    Vector3 startRotation = Vector3.zero;
        //    Vector3 endRotation = maxRotation;

        //    Vector3 tempRotation = Vector3.zero;

        //    while (i < 1)
        //    {
        //        i += Time.deltaTime * rate;

        //        tempRotation.x = Mathf.LerpUnclamped(startRotation.x, endRotation.x, xRotationCurve.Evaluate(i));
        //        tempRotation.z = Mathf.LerpUnclamped(startRotation.z, endRotation.z, zRotationCurve.Evaluate(i));

        //        selectedNut.transform.localEulerAngles = tempRotation;

        //        yield return null;
        //    }
        //    StartNutHoverAnimation(selectedNut);
        //}

        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}