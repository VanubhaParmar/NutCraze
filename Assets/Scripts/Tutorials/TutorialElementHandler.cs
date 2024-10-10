using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class TutorialElementHandler : SerializedManager<TutorialElementHandler>
    {
        #region PUBLIC_VARS

        public SpriteRenderer tutorialBackGroundImage;
        public Transform tutorialUIBlockerImage;
        public TutorialHandAnimation tutorialHandAnimation;
        public TutorialChatView tutorialChatView;
        public RectTransform mainParentRect => gameObject.GetComponent<RectTransform>();

        #endregion

        #region PRIVATE_VARS

        private Action _highlighterMouseUpAction;
        private Action _highlightercEndDragAction;
        private Action _highlightercPointerTap;
        [SerializeField] private TutorialHighlighterData[] highlighterDatas;

        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            InitHandler();
            OnLoadingDone();
        }
        #endregion

        #region PUBLIC_FUNCTIONS
        public void InitHandler()
        {
            tutorialChatView.Init();
        }

        public void SetUIBlocker(bool state)
        {
            tutorialUIBlockerImage.gameObject.SetActive(state);
        }

        public void SetActiveTapHand_ByAnchoredPosition(bool state, Vector3 anchoredPosition, bool playTranslateAnimation = false)
        {
            tutorialHandAnimation.gameObject.SetActive(state);
            if (state)
            {
                tutorialHandAnimation.HandRectTransform.anchoredPosition = anchoredPosition;
                tutorialHandAnimation.endPosition.position = tutorialHandAnimation.HandTransform.position;

                tutorialHandAnimation.startPosition.position = Vector3.zero;

                tutorialHandAnimation.OnTapAnimation(playTranslateAnimation);
            }
        }

        public void SetActiveTapHand(bool state, Transform target = null, bool playTranslateAnimation = false)
        {
            tutorialHandAnimation.gameObject.SetActive(state);
            if (state)
            {
                tutorialHandAnimation.endPosition.position = target.position;
                tutorialHandAnimation.startPosition.position = Vector3.zero;

                tutorialHandAnimation.OnTapAnimation(playTranslateAnimation);
            }
        }

        public void SetActiveTapHand(bool state, Vector3 targetPos, Vector3 rotation = default, bool playTranslateAnimation = false)
        {
            tutorialHandAnimation.gameObject.SetActive(state);
            if (state)
            {
                tutorialHandAnimation.endPosition.position = targetPos;
                tutorialHandAnimation.startPosition.position = Vector3.zero;

                tutorialHandAnimation.OnTapAnimation(playTranslateAnimation);
                tutorialHandAnimation.endPosition.localEulerAngles = rotation;
            }
        }

        public void SetActiveDragHand(bool state, Vector3 startPosition, Vector3 endPosition, bool isLoop = false)
        {
            tutorialHandAnimation.gameObject.SetActive(state);
            if (state)
            {
                tutorialHandAnimation.startPosition.position = startPosition;
                tutorialHandAnimation.endPosition.position = endPosition;
                tutorialHandAnimation.PlayHandMove(isLoop);
            }
        }

        public void RegisterOnHighlighterTap(Action highlighterMouseUpAction)
        {
            _highlightercPointerTap = highlighterMouseUpAction;
        }

        public void RegisterOnHighlighterEndDrag(Action highlightercEndDragAction)
        {
            _highlightercEndDragAction = highlightercEndDragAction;
        }

        public void RegisterOnHighlighterPointerUp(Action highlightercPointerDownAction)
        {
            _highlighterMouseUpAction = highlightercPointerDownAction;
        }

        public void DeregisterOnHighlighterActions()
        {
            _highlightercEndDragAction = null;
            _highlighterMouseUpAction = null;
            _highlightercPointerTap = null;
        }

        public void SetActivateBackGround(bool state, float alpha)
        {
            if (state)
            {
                tutorialBackGroundImage.gameObject.SetActive(true);
                if (alpha > 0)
                    StartCoroutine(BackGroundIn(alpha));
                else
                    StartCoroutine(BackGroundOut());
            }
            else
                StartCoroutine(BackGroundOut(() => tutorialBackGroundImage.gameObject.SetActive(false)));
        }

        public void SetActivateBackGround(bool state)
        {
            tutorialBackGroundImage.gameObject.SetActive(state);
        }

        public TutorialHighlighterData GetHighlighterTransformByType(TutorialHighliterType tutorialHighliterType)
        {
            for (int i = 0; i < highlighterDatas.Length; i++)
            {
                if (tutorialHighliterType == highlighterDatas[i].tutorialHighliterType)
                    return highlighterDatas[i];
            }
            return null;
        }

        public void SetTapHighlighter_ByWorldPosition(Vector3 worldPosition, TutorialHighliterType tutorialHighliterType = TutorialHighliterType.CircleHighlighter, bool isShowHighlighterImage = false, bool isEnableHighlighterTapButton = true)
        {
            var highLighter = GetHighlighterTransformByType(tutorialHighliterType);

            highLighter.mainRectTransform.gameObject.SetActive(true);
            highLighter.mainRectTransform.gameObject.transform.position = worldPosition;
            highLighter.highLighterImage.gameObject.SetActive(isShowHighlighterImage);
            highLighter.highLighterButton.interactable = isEnableHighlighterTapButton;
        }

        public void SetTapHighlighter_ByScreenPosition(Vector3 anchoredPosition, TutorialHighliterType tutorialHighliterType = TutorialHighliterType.CircleHighlighter,  bool isShowHighlighterImage = false, bool isEnableHighlighterTapButton = true)
        {
            var highLighter = GetHighlighterTransformByType(tutorialHighliterType);

            highLighter.mainRectTransform.gameObject.SetActive(true);
            highLighter.mainRectTransform.anchoredPosition = anchoredPosition;
            highLighter.highLighterImage.gameObject.SetActive(isShowHighlighterImage);
            highLighter.highLighterButton.interactable = isEnableHighlighterTapButton;
        }

        public void ResetHighLighters()
        {
            for (int i = 0; i < highlighterDatas.Length; i++)
            {
                highlighterDatas[i].mainRectTransform.gameObject.SetActive(false);
            }
        }

        public void ResetTutorialView()
        {
            tutorialChatView.Hide();
            SetUIBlocker(false);
            SetActivateBackGround(false, 0f);
            SetActiveTapHand(false);
            ResetHighLighters();
        }
        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region CO-ROUTINES

        IEnumerator BackGroundIn(float alpha)
        {
            Color color = tutorialBackGroundImage.color;
            float i = color.a;
            float rate = 1 / 0.25f;
            while (i < alpha)
            {
                i += Time.deltaTime * rate;
                color.a = i;
                tutorialBackGroundImage.color = color;
                yield return null;
            }
        }

        IEnumerator BackGroundOut(Action onDone = null)
        {
            Color color = tutorialBackGroundImage.color;
            float i = color.a;
            float rate = 1 / 0.25f;
            while (i > 0)
            {
                i -= Time.deltaTime * rate;
                color.a = i;
                tutorialBackGroundImage.color = color;
                yield return null;
            }
            onDone?.Invoke();
        }

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       
        public void OnHighlighterClicked()
        {
            _highlightercPointerTap?.Invoke();
        }
        public void OnHighlighterEndDrag()
        {
            _highlightercEndDragAction?.Invoke();
        }
        public void OnHighlighterPointerUp()
        {
            _highlighterMouseUpAction?.Invoke();
        }
        #endregion

    }

    [Serializable]
    public class TutorialHighlighterData
    {
        public TutorialHighliterType tutorialHighliterType;
        public RectTransform mainRectTransform;

        public Image highLighterImage;
        public Button highLighterButton;
    }

    public enum TutorialHighliterType
    {
        None = 0,
        CircleHighlighter = 1,
        FullScreenHighlighter = 2,
    }
}