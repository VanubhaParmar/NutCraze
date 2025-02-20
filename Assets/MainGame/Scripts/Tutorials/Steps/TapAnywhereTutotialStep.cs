using System;
using UnityEngine;
using UnityEngine.Events;

namespace Tag.NutSort {
    public class TapAnywhereTutotialStep : BaseTutorialStep
    {
        #region PUBLIC_VARS

        public UnityEvent OnTapAction;
        public UnityEvent OnStartStep;
        public UnityEvent OnEndStep;

        #endregion

        #region PRIVATE_VARS

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public override void OnStartStep1()
        {
            base.OnStartStep1();
            OnStartStep?.Invoke();

            var highLighter = TutorialElementHandler.GetHighlighterTransformByType(TutorialHighliterType.FullScreenHighlighter);

            highLighter.mainRectTransform.gameObject.SetActive(true);
            highLighter.highLighterButton.interactable = true;
            TutorialElementHandler.RegisterOnHighlighterTap(OnTap);
        }

        public override void EndStep()
        {
            TutorialElementHandler.GetHighlighterTransformByType(TutorialHighliterType.FullScreenHighlighter).mainRectTransform.gameObject.SetActive(false);
            TutorialElementHandler.DeregisterOnHighlighterActions();

            OnEndStep?.Invoke();
            base.EndStep();
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        private void OnTap()
        {
            OnTapAction?.Invoke();
            EndStep();
        }
        
        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
