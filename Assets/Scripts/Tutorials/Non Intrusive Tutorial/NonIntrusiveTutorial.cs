using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort
{
    public class NonIntrusiveTutorial : SerializedMonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public static bool IsRunning = false;
        public TutorialState State;
        public NonIntrusiveTutorialType TutorialType => _tutorialType;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private NonIntrusiveTutorialType _tutorialType;
        [SerializeField] private TutorialPlayRepeatConditionType _tutorialPlayRepeatConditionType;
        [SerializeField] private bool _basedOnCampaignLevel;
        [SerializeField, ShowIf(nameof(_basedOnCampaignLevel))] private Level _campaignLevel;
        [SerializeField, ShowIf(nameof(_basedOnCampaignLevel))] private CompareLevelResult _levelCompareResult;
        [SerializeField] private List<NonIntrusiveTapTutorialStep> _tutorialSteps;
        [SerializeField] private List<BaseTutorialCondition> _extraTutorialConditions;
        [SerializeField] private BaseNonIntrusiveTutorialTrigger _nonIntrusiveTutorialTrigger;

        private int _lastRunningStep = 0;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        public IEnumerator Start()
        {
            while (TutorialManager.Instance == null)
                yield return null;
            TutorialManager.Instance.AddTutorialToList(this);
        }

        public void OnDisable()
        {
            TutorialManager.Instance.RemoveTutorialFromList(this);
        }
        #endregion

        #region PUBLIC_METHODS
        public void LoadData()
        {
            State = TutorialState.NotComplete;
        }

        public void StartTutorial()
        {
            State = TutorialState.Running;
            IsRunning = true;
            StartNextStep();
        }

        public void StartNonIntrusiveTutorialTrigger()
        {
            if (_nonIntrusiveTutorialTrigger != null)
                _nonIntrusiveTutorialTrigger.OnStartNonIntrusiveTutorialTriggerCheck(this);
        }

        public bool CanStartTutorial()
        {
            return CheckForTutorialConditions();
        }

        public void ForceStopTutorial()
        {
            if (State == TutorialState.Running)
            {
                State = TutorialState.Completed;
                TutorialManager.RaiseOnNonIntrusiveTutorialComplete(_tutorialType);
                TutorialElementHandler.Instance.ResetTutorialView();
            }
        }
        #endregion

        #region PRIVATE_METHODS
        private void ResponseToCompleteStep()
        {
            _lastRunningStep++;
            if (_tutorialSteps.Count <= _lastRunningStep)
            {
                IsRunning = false;

                State = TutorialState.Completed;
                TutorialElementHandler.Instance.ResetTutorialView();
                TutorialManager.RaiseOnNonIntrusiveTutorialComplete(_tutorialType);
            }
            else
            {
                StartNextStep();
            }
        }

        private void StartNextStep()
        {
            if (_lastRunningStep < _tutorialSteps.Count)
                _tutorialSteps[_lastRunningStep].StartStep(ResponseToCompleteStep, AbortTutorial);
            else
                ResponseToCompleteStep();
        }

        private void AbortTutorial()
        {
            IsRunning = false;

            State = TutorialState.Completed;
            TutorialElementHandler.Instance.ResetTutorialView();
            TutorialManager.RaiseOnNonIntrusiveTutorialAbort(_tutorialType);
        }

        private bool CheckForTutorialConditions()
        {
            if (_extraTutorialConditions.Find(x => !x.IsTutorialConditionPassed()) != null)
                return false;

            return true;
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region UNITY_EDITOR_FUNCTIONS
#if UNITY_EDITOR
        [Button]
        public void Editor_ClearTutorialState()
        {
            _lastRunningStep = 0;
            State = TutorialState.NotComplete;
        }

        [Button]
        public void Editor_RefreshSteps()
        {
            _tutorialSteps.Clear();

            for (int i = 0; i < transform.childCount; i++)
            {
                var step = transform.GetChild(i).GetComponent<NonIntrusiveTapTutorialStep>();
                if (step != null)
                    _tutorialSteps.Add(step);
            }
        }
#endif
        #endregion
    }

    public enum NonIntrusiveTutorialType
    {
        Gameplay_Hero_Ult_Ready
    }

    public abstract class BaseNonIntrusiveTutorialTrigger : SerializedMonoBehaviour
    {
        protected NonIntrusiveTutorial _nonIntrusiveTutorial;

        public virtual void OnStartNonIntrusiveTutorialTriggerCheck(NonIntrusiveTutorial nonIntrusiveTutorial)
        {
            _nonIntrusiveTutorial = nonIntrusiveTutorial;
        }
        public virtual void StopTrigger() { }
        public bool CanTriggerTutorial()
        {
            return !NonIntrusiveTutorial.IsRunning && !Tutorial.IsRunning;
        }
    }
}