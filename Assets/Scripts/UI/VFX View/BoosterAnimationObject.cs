using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class BoosterAnimationObject : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Image boosterImage;
        [SerializeField] private Text boosterCountText;
        [SerializeField] private CanvasGroup objectCg;
        [SerializeField] private float animationTime;
        [SerializeField] private AnimationCurve movementAnimationCurve;
        [SerializeField] private AnimationCurve alphaAnimationCurve;
        [SerializeField] private Vector3 startPosOffset;
        [SerializeField] private Vector3 endPosOffset;

        private Action endAction;
        private Vector3 startPos;
        private Vector3 endPos;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void PlayAnimation(int boosterType, int count, Vector3 startPosition, Action endAction = null)
        {
            gameObject.SetActive(true);

            boosterImage.sprite = ResourceManager.Instance.GetBoosterSprite(boosterType);
            boosterCountText.text = "+" + count;

            this.endAction = endAction;
            this.startPos = startPosition + startPosOffset;
            endPos = startPos + endPosOffset;

            StartCoroutine(DoAnimation());
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        IEnumerator DoAnimation()
        {
            float i = 0;
            float rate = 1 / animationTime;

            while (i < 1)
            {
                i += Time.deltaTime * rate;
                transform.position = Vector3.LerpUnclamped(startPos, endPos, movementAnimationCurve.Evaluate(i));
                objectCg.alpha = Mathf.LerpUnclamped(0, 1, alphaAnimationCurve.Evaluate(i));
                yield return null;
            }
            endAction?.Invoke();
            gameObject.SetActive(false);
        }
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}