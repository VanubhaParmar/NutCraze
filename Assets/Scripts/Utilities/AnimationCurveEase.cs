using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Tag.NutSort
{
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
}