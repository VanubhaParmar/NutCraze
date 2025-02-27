using DG.Tweening;
using Sirenix.OdinInspector;
using System;
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

        [SerializeField] private float minFillValue;

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
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * GetFillAmountToMultiply());
            if (axis == RectTransform.Axis.Vertical)
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * GetFillAmountToMultiply());
        }

        private float GetFillAmountToMultiply()
        {
            //return FillAmount;
            return FillAmount > 0.001f ? Mathf.Max(FillAmount, minFillValue) : 0f;
        }

        private void SetFillImage(float size)
        {
            if (axis == RectTransform.Axis.Horizontal)
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * size);
            if (axis == RectTransform.Axis.Vertical)
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * size);
        }

        private float GetFillAmount(float targetfill)
        {
            return targetfill > 0.001f ? Mathf.Max(targetfill, minFillValue) : 0f;
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

            float startFillVal = GetFillAmount(startVal);
            float endFillVal = GetFillAmount(finalVal);

            if (tweenAnim != null && !tweenAnim.IsActive())
                tweenAnim = null;
            else if (tweenAnim != null && tweenAnim.IsPlaying() && killCurrentTween)
            {
                tweenAnim.Kill();
                tweenAnim = null;
            }

            if (tweenAnim == null)
                tweenAnim = DOTween.To(() => FillAmount, (x) =>
                {
                    float iv = Mathf.InverseLerp(startVal, finalVal, x);
                    float fill = Mathf.Lerp(startFillVal, endFillVal, iv);

                    Fill(x);
                    SetFillImage(fill);

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