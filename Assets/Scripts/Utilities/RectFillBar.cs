using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort
{
    public class RectFillBar : MonoBehaviour
    {
        #region private veriables

        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private RectTransform.Axis axis;

        [SerializeField] private float width;
        [SerializeField] private float height;

        #endregion

        #region propertice

        public float FillAmount { get; set; }
        private Tweener tweenAnim = null;

        #endregion

        #region public methods

        [Button]
        public void Fill(float fillAmount)
        {
            fillAmount = Mathf.Clamp(fillAmount, 0, 1);
            FillAmount = fillAmount;
            if (axis == RectTransform.Axis.Horizontal)
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * fillAmount);
            if (axis == RectTransform.Axis.Vertical)
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * fillAmount);
        }

        [Button]
        public void Fill(float fillAmount, float animationTime, bool killCurrentTween = true, Action<float> setterX = null)
        {
            if (animationTime <= 0f || fillAmount - FillAmount == 0f)
            {
                Fill(fillAmount);
                setterX?.Invoke(fillAmount);
                return;
            }

            float startVal = FillAmount;
            float finalVal = fillAmount;

            if (tweenAnim != null && tweenAnim.IsPlaying() && killCurrentTween)
            {
                tweenAnim.Kill();
                tweenAnim = null;
            }
            else if(tweenAnim != null && !tweenAnim.IsPlaying())
                tweenAnim = null;

            if (tweenAnim == null)
                tweenAnim = DOTween.To(() => FillAmount, (x) =>
                {
                    Fill(x);
                    setterX?.Invoke(x);
                }, fillAmount, animationTime);
        }

        [Button]
        public void SetWith()
        {
            width = rectTransform.rect.width;
        }

        #endregion
    }
}