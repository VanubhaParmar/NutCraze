using System;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Tag.NutSort
{
    public abstract class BaseTutorialStep : SerializedMonoBehaviour
    {

        #region PRIVATE_VARS

        [SerializeField] private bool saveData;
        [SerializeField] private bool blockUI = true;
        [SerializeField] private bool backgroundInvisible;
        [SerializeField, HideIf("backgroundInvisible")] private float _backgroundAlpha = 0.5f;
        [SerializeField] private float _waitForStartNextStep;
        
        private Action _responseToCompleteStep;
        private Coroutine _startStepCO;

        protected TutorialElementHandler TutorialElementHandler { get { return TutorialElementHandler.Instance; } }

        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        public void StartStep(Action responseToCompleteStep)
        {
            _responseToCompleteStep = responseToCompleteStep;
            _startStepCO = StartCoroutine(StartStepWaitCO());
        }

        public virtual void EndStep()
        {
            if (_startStepCO != null)
            {
                StopCoroutine(_startStepCO);
                _startStepCO = null;
            }
            HideBackground();
            TutorialElementHandler.DeregisterOnHighlighterActions();
            _responseToCompleteStep?.Invoke();
            CheckForDataSave();
        }

        [Button]
        public virtual void OnStartStep1()
        {
            SetBackground();
            BlockUI();
        }

        public void SetBackground()
        {
            float alpha = backgroundInvisible ? 0 : _backgroundAlpha;
            TutorialElementHandler.SetActivateBackGround(true, alpha);
        }

        public void BlockUI()
        {
            TutorialElementHandler.SetUIBlocker(blockUI);
        }

        public void BlockUI(bool state)
        {
            TutorialElementHandler.SetUIBlocker(state);
        }
        #endregion

        #region PRIVATE_FUNCTIONS

        private void HideBackground()
        {
            if (!backgroundInvisible)
                TutorialElementHandler.SetActivateBackGround(false, 0f);
        }

        private void CheckForDataSave()
        {
        }

        #endregion

        #region CO-ROUTINES

        private IEnumerator StartStepWaitCO()
        {
            BlockUI(true);
            yield return new WaitForSeconds(_waitForStartNextStep);
            BlockUI(false);
            OnStartStep1();
            _startStepCO = null;
        }

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
