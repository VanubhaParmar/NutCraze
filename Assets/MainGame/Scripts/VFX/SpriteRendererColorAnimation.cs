using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort {
    public class SpriteRendererColorAnimation : SerializedMonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private SpriteRenderer targetSpriteRenderer;
        [SerializeField] private Color startColor;
        [SerializeField] private Color endColor;
        [SerializeField] private float animationTime;
        [SerializeField] private bool isLoop;
        [SerializeField] private bool playOnEnable;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void OnEnable()
        {
            if (playOnEnable)
                OnPlayColorAnimation();
        }

        private void OnDisable()
        {
            DOTween.Kill(transform);
        }
        #endregion

        #region PUBLIC_METHODS
        public void OnPlayColorAnimation()
        {
            targetSpriteRenderer.color = startColor;
            Sequence tweenSeq = DOTween.Sequence().SetTarget(transform);
            tweenSeq.Append(targetSpriteRenderer.DOColor(endColor, animationTime));

            if (isLoop)
            {
                tweenSeq.Append(targetSpriteRenderer.DOColor(startColor, animationTime));
                tweenSeq.SetLoops(-1);
            }
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