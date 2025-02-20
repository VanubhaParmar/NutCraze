using DG.Tweening;
using System;
using UnityEngine;

namespace Tag.NutSort {
    public class AnimationCurveEase
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        private AnimationCurve myAnimationCurve;
        #endregion

        #region PROPERTIES
        #endregion

        #region CONSTRUCTORS
        public AnimationCurveEase() { }
        public AnimationCurveEase(AnimationCurve animationCurve)
        {
            myAnimationCurve = animationCurve;
        }
        #endregion

        #region PUBLIC_METHODS
        public float EaseFunction(float time, float duration, float overshootOrAmplitude, float period)
        {
            return myAnimationCurve.Evaluate(Mathf.InverseLerp(0f, duration, time));
        }

        public float RevereseEaseFunction(float time, float duration, float overshootOrAmplitude, float period)
        {
            float i = Mathf.Clamp(duration - time, 0f, 1f);
            return myAnimationCurve.Evaluate(Mathf.InverseLerp(0f, duration, i));
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

    public class Vector3DotweenerAnimation
    {
        #region PUBLIC_VARIABLES
        public Vector3 startValue;
        public Vector3 endValue;
        public float duration;
        #endregion

        #region PRIVATE_VARIABLES
        private float i;
        private AnimationCurve myAnimationCurve;
        #endregion

        #region PROPERTIES
        #endregion

        #region CONSTRUCTORS
        public Vector3DotweenerAnimation() { }
        public Vector3DotweenerAnimation(Vector3 startValue, Vector3 endValue, float duration, AnimationCurve animationCurve)
        {
            this.startValue = startValue;
            this.endValue = endValue;
            this.duration = duration;
            myAnimationCurve = animationCurve;
            i = 0f;
        }
        #endregion

        #region PUBLIC_METHODS
        public Tween StartDotweenAnimation(Transform target, Action<Vector3> setter, bool isLoop = false, bool isClamped = false)
        {
            Sequence tweenSeq = DOTween.Sequence().SetId(target).SetLoops(isLoop ? -1 : 1);
            tweenSeq.Append(DOTween.To(() => i, x => i = x, 1f, duration).SetEase(Ease.Linear)).OnUpdate(() =>
            {
                Vector3 targetVector = isClamped ? Vector3.Lerp(startValue, endValue, myAnimationCurve.Evaluate(i)) : Vector3.LerpUnclamped(startValue, endValue, myAnimationCurve.Evaluate(i));
                setter?.Invoke(targetVector);
            });

            return tweenSeq;
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

    public class FloatDotweenerAnimation
    {
        #region PUBLIC_VARIABLES
        public float startValue;
        public float endValue;
        public float duration;
        #endregion

        #region PRIVATE_VARIABLES
        private float i;
        private AnimationCurve myAnimationCurve;
        #endregion

        #region PROPERTIES
        #endregion

        #region CONSTRUCTORS
        public FloatDotweenerAnimation() { }
        public FloatDotweenerAnimation(float startValue, float endValue, float duration, AnimationCurve animationCurve)
        {
            this.startValue = startValue;
            this.endValue = endValue;
            this.duration = duration;
            myAnimationCurve = animationCurve;
            i = 0f;
        }
        #endregion

        #region PUBLIC_METHODS
        public Tween StartDotweenAnimation(Transform target, Action<float> setter, bool isLoop = false, bool isClamped = false)
        {
            Sequence tweenSeq = DOTween.Sequence().SetId(target).SetLoops(isLoop ? -1 : 1);
            tweenSeq.AppendCallback(() => { i = 0f; });
            tweenSeq.Append(DOTween.To(() => i, x => i = x, 1f, duration).SetEase(Ease.Linear)).OnUpdate(() =>
            {
                float targetFloat = isClamped ? Mathf.Lerp(startValue, endValue, myAnimationCurve.Evaluate(i)) : Mathf.LerpUnclamped(startValue, endValue, myAnimationCurve.Evaluate(i));
                setter?.Invoke(targetFloat);
            });

            return tweenSeq;
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