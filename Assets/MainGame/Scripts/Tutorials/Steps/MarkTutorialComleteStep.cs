using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort {
    public class MarkTutorialComleteStep : BaseTutorialStep
    {
        #region PUBLIC_VARIABLES
        public Tutorial targetTutorial;
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
            targetTutorial.MarkComplete();
            EndStep();
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