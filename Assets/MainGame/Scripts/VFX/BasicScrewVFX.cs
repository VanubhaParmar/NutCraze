using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.NutSort
{
    public class BasicScrewVFX : MonoBehaviour
    {
        #region PRIVATE_VARS
        [Header("Nut Raise Animation Data")]
        [SerializeField] private float screwSelectionNutRaiseHeight;
        [SerializeField] private float nutRaiseTime;
        [SerializeField] private float rotationTimeTakenForSingleRotation = 0.1f;
        [SerializeField] private AnimationCurve raiseAnimationCurve;

        [Header("Screw Sort Complete Animation Data")]
        [SerializeField] private float nutCapRaiseHeight;
        [SerializeField] private float nutCapPlaceTime;

        [Header("Selected Nut Hover Animation Data")]
        [SerializeField] private float hoverAnimationDuration;
        [SerializeField] private Vector3 startRotation;
        [SerializeField] private Vector3 maxRotation;
        [SerializeField] private AnimationCurve xRotationCurve;
        [SerializeField] private AnimationCurve zRotationCurve;

        private AnimationCurveEase raiseAnimationCurveEase;
        private BaseScrew myScrew;
        #endregion

        #region PUBLIC_VARS
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        public void Init(BaseScrew baseScrew)
        {
            myScrew = baseScrew;
            raiseAnimationCurveEase = new AnimationCurveEase(raiseAnimationCurve);
        }

        public void PlayScrewSortCompletion()
        {
            NutsHolderScrewBehaviour startScrewNutsBehaviour = myScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();

            Vector3 tweenTargetMidPosition = myScrew.GetScrewCapPosition() + nutCapRaiseHeight * Vector3.up;
            MeshRenderer screwCap = myScrew.ScrewTopRenderer;

            screwCap.transform.position = tweenTargetMidPosition;
            screwCap.gameObject.SetActive(true);
            screwCap.transform.localScale = Vector3.zero;

            Action screwCapResetAction = delegate
            {
                screwCap.transform.position = myScrew.GetScrewCapPosition();
                screwCap.transform.localScale = Vector3.one * myScrew.ScrewDimensions.screwCapScale;
            };

            Sequence tweenSeq = DOTween.Sequence().SetId(myScrew.transform);
            tweenSeq.AppendCallback(() =>
            {
                myScrew.PlayStackFullParticlesByID(startScrewNutsBehaviour.PeekNut().GetNutColorType());
                myScrew.PlayStackFullIdlePS();
                Vibrator.MediumFeedback();
                SoundHandler.Instance.PlaySound(SoundType.ScrewSorted);
                GameManager.Instance.MainCameraShakeAnimation.DoShake();
            });
            tweenSeq.Append(screwCap.transform.DOScale(Vector3.one * myScrew.ScrewDimensions.screwCapScale, nutCapPlaceTime * 0.5f));
            tweenSeq.Append(screwCap.transform.DOMove(myScrew.GetScrewCapPosition(), nutCapPlaceTime).SetEase(raiseAnimationCurveEase.EaseFunction));
            tweenSeq.onComplete += screwCapResetAction.Invoke;
            tweenSeq.onKill += screwCapResetAction.Invoke;
            BaseNut lastNutInAnimation = startScrewNutsBehaviour.PeekNut();
            List<Tween> runningTweens = DOTween.TweensById(lastNutInAnimation.transform);
            if (runningTweens != null && runningTweens.Count > 0)
            {
                myScrew.transform.DOPause();
                runningTweens.First().onComplete += () => myScrew.transform.DOPlay();
            }
        }

        public void LiftTheFirstNutAnimation()
        {
            if (myScrew.TryGetScrewBehaviour(out NutsHolderScrewBehaviour nutsHolderScrewBehaviour))
            {
                BaseNut selectionNut = nutsHolderScrewBehaviour.PeekNut();

                Vector3 tweenTargetPosition = myScrew.GetTopPosition() + screwSelectionNutRaiseHeight * Vector3.up;
                float totalNumberOfRotation = Mathf.Ceil(nutRaiseTime / rotationTimeTakenForSingleRotation);
                SoundInstance nutRotateSound = null;

                Action nutResetAction = delegate
                {
                    selectionNut.transform.localEulerAngles = Vector3.zero;
                    nutRotateSound?.Stop();
                };

                DOTween.Kill(selectionNut.transform);
                selectionNut.transform.localScale = Vector3.one;
                Sequence tweenSeq = DOTween.Sequence().SetId(selectionNut.transform);
                tweenSeq.AppendCallback(() => { nutRotateSound = SoundHandler.Instance.PlaySoundWithNewInstance(SoundType.NutRotate); });
                tweenSeq.Append(selectionNut.transform.DOMove(tweenTargetPosition, nutRaiseTime).SetEase(raiseAnimationCurveEase.EaseFunction));
                tweenSeq.Join(selectionNut.transform.DORotate(Vector3.down * (totalNumberOfRotation * 360), nutRaiseTime, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetRelative(true));
                tweenSeq.AppendCallback(() => { PlayNutHoverAnimation(selectionNut); });

                tweenSeq.onComplete += nutResetAction.Invoke;
                tweenSeq.onKill += nutResetAction.Invoke;

                SoundHandler.Instance.PlaySound(SoundType.NutSelect);
            }
        }

        public void PutBackFirstSelectedNutAnimation()
        {
            if (myScrew.TryGetScrewBehaviour(out NutsHolderScrewBehaviour nutsHolderScrewBehaviour))
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
                tweenSeq.Append(selectionNut.transform.DOMove(tweenTargetPosition, nutRaiseTime).SetEase(raiseAnimationCurveEase.EaseFunction));
                tweenSeq.Join(selectionNut.transform.DORotate(Vector3.up * (totalNumberOfRotation * 360), nutRaiseTime, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetRelative(true));
                tweenSeq.onComplete += nutResetAction.Invoke;
                tweenSeq.onKill += nutResetAction.Invoke;
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

        #endregion

        #region PRIVATE_FUNCTIONS
        private void PlayNutHoverAnimation(BaseNut selectionNut)
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
