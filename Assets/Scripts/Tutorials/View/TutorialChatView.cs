using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class TutorialChatView : BaseView
    {
        #region PUBLIC_VARS

        [SerializeField] private Text _chatText;
        [SerializeField] private GameObject ViewObject;
        [SerializeField] private Vector3 startScale = Vector3.zero;
        [SerializeField] CanvasGroup cg;
        [SerializeField] private AnimationCurve scaleCurve;
        private Vector3 endScale = Vector3.one;
        
        #endregion

        #region PRIVATE_VARS

        private Coroutine coroutine;
        
        #endregion

        #region PUBLIC_FUNCTIONS

        public void ShowView(string text)
        {
            SetChat(text);
            base.ShowView();
            ShowAnimation();
        }

        public override void Hide()
        {
            HideAnimation(() => base.Hide());
        }

        public void SetChat(string text)
        {
            _chatText.text = (text);
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        public void ShowAnimation()
        {
            if (coroutine != null)
                StopCoroutine(coroutine);

            if (ViewObject != null)
                coroutine = StartCoroutine(DoShowFx());
        }

        public void HideAnimation(Action action)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);

            if (ViewObject != null && gameObject.activeInHierarchy)
                coroutine = StartCoroutine(DoHideFx(action));
            else
                action?.Invoke();
        }

        #endregion

        #region CO-ROUTINES

        public IEnumerator DoShowFx()
        {
            float i = 0;
            float rate = 1 / 0.45f;
            Vector3 tempVec = new Vector3();
            while (i < 1)
            {
                i += rate * Time.unscaledDeltaTime;

                tempVec = Vector3.LerpUnclamped(startScale, endScale, scaleCurve.Evaluate(i));
                cg.alpha = Mathf.Lerp(0, 1, i);
                ViewObject.transform.localScale = tempVec;
                yield return 0;
            }

        }

        public IEnumerator DoHideFx(Action action)
        {
            float i = 0;
            float rate = 1 / 0.35f;
            Vector3 tempVec = new Vector3();
            while (i < 1)
            {
                i += rate * Time.unscaledDeltaTime;

                tempVec = Vector3.LerpUnclamped(startScale, endScale, scaleCurve.Evaluate(1 - i));
                cg.alpha = Mathf.Lerp(1, 0, i);
                ViewObject.transform.localScale = tempVec;
                yield return 0;
            }

            action?.Invoke();
        }

        #endregion
    }

    public enum TutorialChatLocationType
    {
        None = 0,
        Top = 1,
        Middle = 2,
        Bottom = 3
    }
}
