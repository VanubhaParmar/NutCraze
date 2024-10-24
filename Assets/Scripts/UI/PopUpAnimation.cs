using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class PopUpAnimation : BaseUiAnimation
    {
        [SerializeField] private GameObject ViewObject;

        [SerializeField] private Vector3 startPos = new Vector3(0, -930, 0);
        [SerializeField] private CanvasGroup cg;

        //  [SerializeField] private GameObject endPos;
        [SerializeField] private AnimationCurve yCurve;
        [SerializeField] private float InSpeed = 0.4f;
        [SerializeField] private float OutSpeed = 0.15f;

        //[SerializeField] private AnimationCurve xCurve;
        private Vector3 endPos;
        private Coroutine coroutine;

        private void Awake()
        {
            endPos = ViewObject.transform.localPosition;
        }

        #region overrided methods
        [Button]
        public override void ShowAnimation(Action action)
        {
            base.ShowAnimation(action);
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }

            if (ViewObject != null)
            {
                coroutine = StartCoroutine(DoShowFx());
            }

            //SoundManager.Instance.PlaySfx(SFXType.PopupIn);
        }

        [Button]
        public override void HideAnimation(Action action)
        {
            base.HideAnimation(action);
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }

            if (ViewObject != null)
            {
                coroutine = StartCoroutine(DoHideFx());
            }
        }

        #endregion


        public IEnumerator DoShowFx()
        {
            float i = 0;
            float rate = 1 / InSpeed;
            Vector3 tempVec = new Vector3(0, 0, 0);
            Color bgColor;
            while (i < 1)
            {
                i += rate * Time.unscaledDeltaTime;
                tempVec = Vector3.LerpUnclamped(startPos, endPos, yCurve.Evaluate(i));
                bgColor.a = Mathf.LerpUnclamped(0, 1, i * 1.3f);
                if (cg != null)
                    cg.alpha = bgColor.a;
                ViewObject.transform.localPosition = tempVec;
                yield return 0;
            }
            //Debug.Log("<color=yellow>DoShowFx</color>" + startPos + "_______" + endPos);
            ViewObject.transform.localPosition = endPos;
            onShowComplete?.Invoke();
        }

        public IEnumerator DoHideFx()
        {
            float i = 0;
            float rate = 1 / OutSpeed;
            Vector3 tempVec = new Vector3(0, 0, 0);
            Color bgColor;
            while (i < 1)
            {   
                i += rate * Time.unscaledDeltaTime;
                tempVec = Vector3.LerpUnclamped(startPos, endPos, yCurve.Evaluate(1 - (i)));
                bgColor.a = Mathf.LerpUnclamped(1, 0, i);
                if (cg != null)
                    cg.alpha = bgColor.a;
                ViewObject.transform.localPosition = tempVec;
                yield return 0;
            }
            //Debug.Log("<color=yellow>DoHideFx</color>" + startPos + "_______" + endPos);
            ViewObject.transform.localPosition = endPos;
            onHideComplete?.Invoke();
        }
    }
}