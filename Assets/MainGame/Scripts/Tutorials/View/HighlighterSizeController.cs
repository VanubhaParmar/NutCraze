using UnityEngine;
using UnityEngine.Events;

namespace Tag.NutSort {
    public class HighlighterSizeController : MonoBehaviour
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        [Header("Tap Highlighter")]
        [SerializeField] private TutorialHighliterType _highlighterType;
        [SerializeField] private Transform _highlighterTargetTransform;
        [SerializeField] private Vector2 _scale;
        [SerializeField] private Vector3 _offset;
        [SerializeField] private bool canShowHighlighterImage;
        [SerializeField] private bool enableButton;
        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public void ActiveHighlighter()
        {
            if (_highlighterType != TutorialHighliterType.None)
            {
                var highlighter = TutorialElementHandler.Instance.GetHighlighterTransformByType(_highlighterType);

                highlighter.mainRectTransform.localScale = _scale;
                highlighter.mainRectTransform.position = _highlighterTargetTransform.position + _offset;
                highlighter.mainRectTransform.gameObject.SetActive(true);

                highlighter.highLighterImage.gameObject.SetActive(canShowHighlighterImage);
                highlighter.highLighterButton.gameObject.SetActive(enableButton);
            }
        }

        public void DeactivateHighlighter()
        {
            TutorialElementHandler.Instance.GetHighlighterTransformByType(_highlighterType).mainRectTransform.gameObject.SetActive(false);
            TutorialElementHandler.Instance.DeregisterOnHighlighterActions();
        }

        public void SetHighlighterTransform(Transform targetTransform)
        {
            _highlighterTargetTransform = targetTransform;
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion
    }
}
