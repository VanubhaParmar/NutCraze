using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class LoadingView : BaseView
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        [SerializeField] private Text versionNumber;
        [SerializeField] private RectFillBar fillImage;
        [SerializeField] private Text textLoadingProgress;
        private Coroutine coroutine;
        #endregion

        #region UNITY_CALLBACKS

        private void Start()
        {
            SetLoadingBar(0f, false);
        }

        public void SetLoadingBar(float amount, bool animationPlay = true, float animtionTime = 0.5f)
        {
            amount = Mathf.Clamp(amount, 0.02f, 1f);
            fillImage.Fill(amount, animationPlay ? animtionTime : 0f, true, (x) => { SetLoading(x); });
        }
        public float GetFillProgress()
        {
            return fillImage.FillAmount;
        }

        public override void Show(Action action = null, bool isForceShow = false)
        {
            if (!IsActive)
                base.Show(action);
            versionNumber.text = Constant.BuildVersionCodeString;
        }
        public override void Hide()
        {
            if (IsActive)
            {
                CoroutineRunner.Instance.Wait(0.1f, base.Hide);
            }
            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = null;
        }
        #endregion

        #region PUBLIC_FUNCTIONS

        #endregion

        #region PRIVATE_FUNCTIONS

        private void SetLoading(float amount)
        {
            textLoadingProgress.text = $"Loading ({(int)(amount * 100)}%)";
        }
        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
