using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        //[SerializeField] private Text textTips;
        //[SerializeField] private List<string> tips = new List<string>();
        //[SerializeField] private Image bg;
        //[SerializeField] private RectTransform bgParent;
        private Coroutine coroutine;

        #endregion

        #region UNITY_CALLBACKS

        private void Start()
        {
            //SetImageAspectRatio();
            //SetTips();
            SetLoadingBar(0f, false);
        }

        /// <summary>
        /// Set loading bar of screen
        /// </summary>
        /// <param name="amount">Value between 0 to 1.</param>
        /// <param name="animationPlay">Can play animation</param>
        public void SetLoadingBar(float amount, bool animationPlay = true, float animtionTime = 0.5f)
        {
            amount = Mathf.Clamp(amount, 0.02f, 1f);
            fillImage.Fill(amount, animationPlay ? animtionTime : 0f, true, (x) => { SetLoading(x); });
        }

        //public void SetDownloadingBar(float amount, float downloadedSize, long totalSize)
        //{
        //    downloadedSize = downloadedSize * 0.5f;
        //    totalSize = totalSize / 2;
        //    if (amount >= 0.02)
        //    {
        //        textLoadingProgress.text = "Downloading..." + downloadedSize.ConvertByteToMegaByte().ToString("0.00") + " MB" + "/" + totalSize.ConvertByteToMegaByte().ToString("0.00") + " MB";
        //        fillImage.Fill(amount);
        //    }
        //    else
        //    {
        //        textLoadingProgress.text = "Downloading..." + downloadedSize.ConvertByteToMegaByte().ToString("0.00") + " MB" + "/" + totalSize.ConvertByteToMegaByte().ToString("0.00") + " MB";
        //        fillImage.Fill(0.02f);
        //    }
        //}

        public float GetFillProgress()
        {
            return fillImage.FillAmount;
        }

        public override void Show(Action action = null, bool isForceShow = false)
        {
            SetText();
            if (!IsActive)
            {
                base.Show(action);
            }
            //SetTips();
            versionNumber.text = Constant.BuildVersionCodeString;
        }

        //private void SetTips()
        //{
        //    if (coroutine == null)
        //        coroutine = StartCoroutine(TipsCo());
        //}

        //private void SetImageAspectRatio()
        //{
        //    float videoWidth = bg.sprite.rect.width;
        //    float videoHeight = bg.sprite.rect.height;

        //    float videoAspectRatio = videoWidth / videoHeight;

        //    float parentWidth = bgParent.rect.width;
        //    float parentHeight = bgParent.rect.height;

        //    if (parentWidth / parentHeight > videoAspectRatio)
        //        bg.rectTransform.sizeDelta = new Vector2(parentWidth, parentWidth / videoAspectRatio);

        //    else
        //        bg.rectTransform.sizeDelta = new Vector2(parentHeight * videoAspectRatio, parentHeight);
        //}

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

        public void OnLoadingScreenButtonClick()
        {
            SetText();
        }

        private void SetText()
        {
            //textTips.text = GameLocalization.GetTranslate(tips[UnityEngine.Random.Range(0, tips.Count)]);
            //textTips.text = tips.GetRandomItemFromList();
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        #endregion

        #region PRIVATE_FUNCTIONS

        private void SetLoading(float amount)
        {
            textLoadingProgress.text = $"Loading ({(int)(amount * 100)}%)";
        }

        private int LoadingView_OnSomething(int someInt, float someFloat)
        {
            return someInt + (int)someFloat;
        }

        #endregion

        #region CO-ROUTINES

        //IEnumerator TipsCo()
        //{
        //    float rate = 1f / 0.6f;
        //    float timer = 0;
        //    Color targetColor = textTips.color;
        //    targetColor.a = 0;
        //    SetText();
        //    while (true)
        //    {
        //        timer = 0;
        //        targetColor.a = 0;
        //        yield return new WaitForSeconds(5f);
        //        while (timer < 1)
        //        {
        //            timer += rate * Time.deltaTime;
        //            textTips.color = Color.Lerp(textTips.color, targetColor, timer);
        //            yield return 0;
        //        }
        //        timer = 0;
        //        SetText();
        //        targetColor.a = 1;
        //        while (timer < 1)
        //        {
        //            timer += rate * Time.deltaTime;
        //            textTips.color = Color.Lerp(textTips.color, targetColor, timer);
        //            yield return 0;
        //        }
        //    }
        //}

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
