using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace Tag.NutSort
{
    public class ToastMessage : MonoBehaviour
    {
        #region PUBLIC_VARS

        [SerializeField] private Text message;
        [SerializeField] private CanvasGroup cg;
        [SerializeField] private float animationTime;
        [SerializeField] private AnimationCurve positionCurve;
        [SerializeField] private AnimationCurve alphaCurve;
        [SerializeField] private Vector3 startPosOffset;
        [SerializeField] private Vector3 endPosOffset;

        #endregion

        #region PRIVATE_VARS

        private Vector3 startPos;
        private Vector3 endPos;

        private Action endAction;

        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS

        public void ShowToastMessage(string message, Vector3 startPosition, Action endAction = null)
        {
            gameObject.SetActive(true);
            this.message.text = message;
            this.endAction = endAction;
            this.startPos = startPosition + startPosOffset;
            endPos = startPos + endPosOffset;

            StartCoroutine(DoToastMessageAnimation());
        }

        #endregion

        #region PRIVATE_FUNCTIONS
        #endregion

        #region CO-ROUTINES

        IEnumerator DoToastMessageAnimation()
        {
            float i = 0;
            float rate = 1 / animationTime;

            while (i < 1)
            {
                i += Time.deltaTime * rate;
                transform.position = Vector3.LerpUnclamped(startPos, endPos, positionCurve.Evaluate(i));
                cg.alpha = Mathf.LerpUnclamped(0, 1, alphaCurve.Evaluate(i));
                yield return null;
            }
            endAction?.Invoke();
            gameObject.SetActive(false);
        }

        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}
