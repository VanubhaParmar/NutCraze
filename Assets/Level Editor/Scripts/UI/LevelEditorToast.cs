using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort.Editor {
    public class LevelEditorToast : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public Text messageText;
        public CanvasGroup messageCanvasGroup;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void ShowToast(string message)
        {
            gameObject.SetActive(true);

            messageText.text = message;
            RectTransform currentRect = gameObject.GetComponent<RectTransform>();
            currentRect.anchoredPosition = Vector2.zero;
            messageCanvasGroup.alpha = 1f;

            Sequence toastTween = DOTween.Sequence();
            toastTween.Append(currentRect.DOAnchorPos(Vector2.up * 600f, 2f).SetEase(Ease.OutQuad));
            toastTween.Insert(1f, messageCanvasGroup.DOFade(0f, 1f));
            toastTween.onComplete += () => {
                gameObject.SetActive(false);
            };
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