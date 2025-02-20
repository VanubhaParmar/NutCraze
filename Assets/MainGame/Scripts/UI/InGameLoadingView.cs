using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Tag.NutSort
{
    public class InGameLoadingView : BaseView
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        public TMP_Text progressText;
        [SerializeField] private RectFillBar fillImage;
        [SerializeField] private TMP_Text versionNumber;
        [SerializeField] List<string> tips = new List<string>();
        [SerializeField] private TMP_Text textTips;
        private Action onLoadingDone;
        private Coroutine coroutineTip;
        private Coroutine coroutineLoading;
        private float defalutTime = 2f;

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);
            SetTips();
            SetLoding(defalutTime);
            versionNumber.text = "V" + Application.version;
        }

        public void ShowView(float time = 2f, Action onLoadingDone = null)
        {
            base.Show();
            SetTips();
            versionNumber.text = "V" + Application.version;
            this.onLoadingDone = onLoadingDone;
            SetLoding(time);
            //CoroutineRunner.Instance.Wait(time, () =>
            //{
            //    this.onLoadingDone?.Invoke();
            //    Hide();
            //});
        }

        public override void OnBackButtonPressed()
        {
        }

        public override void Hide()
        {
            base.Hide();
        }

        public override void OnViewShowDone()
        {
            base.OnViewShowDone();
        }

        public override void OnHideComplete()
        {
            base.OnHideComplete();
            if (MainSceneUIManager.Instance.GetView<MainView>().IsActive)
            {
                //SoundHandler.Instance.PlayMetaMusic();
                AutoOpenPopupHandler.Instance.OnCheckForAutoOpenPopUps();
            }
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        private void SetTips()
        {
            if (coroutineTip == null)
                coroutineTip = StartCoroutine(TipsCo());
        }

        private void SetLoding(float time)
        {
            //if (coroutineLoading == null)
            {
                coroutineLoading = StartCoroutine(DoLoading(time));
            }
        }

        private void SetText()
        {
            textTips.text = tips.GetRandomItemFromList();
        }

        public void SetProgressBar(float progress)
        {
            fillImage.Fill(progress);
            progressText.text = $"{(int)((float)progress * 100)} %";
        }

        #endregion

        #region CO-ROUTINES

        private IEnumerator DoLoading(float time)
        {
            float rate = 0;
            float j = 1 / time;
            while (rate < 1)
            {
                rate += Time.deltaTime * j;
                SetProgressBar(Mathf.Lerp(0, 1, rate));
                yield return null;
            }
            if (onLoadingDone != null)
            {
                this.onLoadingDone?.Invoke();
                Hide();
            }
            coroutineLoading = null;
        }

        IEnumerator TipsCo()
        {
            float rate = 1f / 0.6f;
            float timer = 0;
            Color targetColor = textTips.color;
            targetColor.a = 0;
            SetText();
            while (true)
            {
                timer = 0;
                targetColor.a = 0;
                yield return new WaitForSeconds(5f);
                while (timer < 1)
                {
                    timer += rate * Time.deltaTime;
                    textTips.color = Color.Lerp(textTips.color, targetColor, timer);
                    yield return 0;
                }
                timer = 0;
                SetText();
                targetColor.a = 1;
                while (timer < 1)
                {
                    timer += rate * Time.deltaTime;
                    textTips.color = Color.Lerp(textTips.color, targetColor, timer);
                    yield return 0;
                }
            }
        }

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS

        public void OnLoadingScreenButtonClick()
        {
            SetText();
        }

        #endregion
    }
}