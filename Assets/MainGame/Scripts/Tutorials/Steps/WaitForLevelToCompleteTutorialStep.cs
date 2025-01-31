using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace com.tag.nut_sort {
    public class WaitForLevelToCompleteTutorialStep : BaseTutorialStep
    {
        #region PUBLIC_VARIABLES
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
            StartCoroutine(WaitForLevelOverCoroutine());
        }

        public override void EndStep()
        {
            OnEndStep?.Invoke();
            base.EndStep();
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        IEnumerator WaitForLevelOverCoroutine()
        {
            while (GameplayManager.Instance.GameplayStateData.gameplayStateType != GameplayStateType.LEVEL_OVER)
            {
                yield return null;
            }
            EndStep();
        }
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}