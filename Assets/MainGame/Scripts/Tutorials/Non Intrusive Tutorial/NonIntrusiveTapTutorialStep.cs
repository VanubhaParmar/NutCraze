using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace com.tag.nut_sort {
    public class NonIntrusiveTapTutorialStep : SerializedMonoBehaviour
    {
        #region PRIVATE_VARS

        [SerializeField] private float _waitForStartNextStep;

        private Action _responseToCompleteStep;
        private Action _responseToAbortTutorial;

        private Coroutine _startStepCO;

        protected TutorialElementHandler TutorialElementHandler { get { return TutorialElementHandler.Instance; } }

        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        public void StartStep(Action responseToCompleteStep, Action responseToAbortTutorial)
        {
            _responseToCompleteStep = responseToCompleteStep;
            _responseToAbortTutorial = responseToAbortTutorial;

            _startStepCO = StartCoroutine(StartStepWaitCO());
        }

        public virtual void EndStep()
        {
            if (_startStepCO != null)
            {
                StopCoroutine(_startStepCO);
                _startStepCO = null;
            }
            TutorialElementHandler.DeregisterOnHighlighterActions();
            _responseToCompleteStep?.Invoke();
            CheckForDataSave();
        }

        public virtual void OnStartStep1()
        {
        }

        public virtual void AbortTutorial()
        {
            if (_startStepCO != null)
            {
                StopCoroutine(_startStepCO);
                _startStepCO = null;
            }
            TutorialElementHandler.DeregisterOnHighlighterActions();
            CheckForDataSave();

            _responseToAbortTutorial?.Invoke();
        }

        public void SetBackground(bool state, float alpha)
        {
            alpha = !state ? 0f : alpha;
            TutorialElementHandler.SetActivateBackGround(true, alpha);
        }

        public void BlockUI(bool state)
        {
            TutorialElementHandler.SetUIBlocker(state);
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        private void CheckForDataSave()
        {
        }

        #endregion

        #region CO-ROUTINES

        private IEnumerator StartStepWaitCO()
        {
            yield return new WaitForSeconds(_waitForStartNextStep);
            //MainUIManager.Instance.HideView<Blocker>();
            OnStartStep1();
            _startStepCO = null;
        }

        protected IEnumerator WaitForTapDownAndCall(Action actionToCall)
        {
            yield return null;

            while (true)
            {
                if (Input.GetMouseButtonDown(0))
                    break;

                yield return null;
            }

            actionToCall?.Invoke();
        }

        protected IEnumerator WaitForTapUpAndCall(Action actionToCall)
        {
            yield return null;

            while (true)
            {
                if (Input.GetMouseButtonUp(0))
                    break;

                yield return null;
            }

            actionToCall?.Invoke();
        }

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
