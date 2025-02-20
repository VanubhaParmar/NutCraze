using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Tag.NutSort {
    public class TapWithActionTutorialStep : BaseTutorialStep
    {
        #region PUBLIC_VARIABLES
        public UnityEvent OnTapAction;
        public UnityEvent OnStartStep;
        public UnityEvent OnEndStep;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void OnStartStep1()
        {
            base.OnStartStep1();
            OnStartStep?.Invoke();

            TutorialElementHandler.RegisterOnHighlighterTap(OnTap);
        }

        public override void EndStep()
        {
            TutorialElementHandler.DeregisterOnHighlighterActions();

            OnEndStep?.Invoke();
            base.EndStep();
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnTap()
        {
            OnTapAction?.Invoke();
            EndStep();
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}