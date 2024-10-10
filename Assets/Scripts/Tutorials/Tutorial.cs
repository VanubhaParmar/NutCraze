using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class Tutorial : SerializedMonoBehaviour
    {
        #region PUBLIC_VARS
        public static bool IsRunning = false;

        public TutorialType TutorialType => _tutorialType;
        public TutorialState State
        {
            get => _tutorialPlayerData.tutorialState;
            private set
            {
                _tutorialPlayerData.tutorialState = value;
                TutorialManager.Instance.SaveData();
            }
        }
        public TutorialPlayType TutorialPlayType => tutorialPlayType;
        #endregion

        #region PRIVATE_VARS

        [SerializeField] private TutorialPlayType tutorialPlayType;
        [SerializeField] private TutorialType _tutorialType;
        [SerializeField] private List<TutorialType> _preCompleteTutorialTypes;
        [SerializeField] private TutorialPlayRepeatConditionType _tutorialPlayRepeatConditionType;
        [SerializeField] private bool _basedOnCampaignLevel;
        [SerializeField, ShowIf(nameof(_basedOnCampaignLevel))] private Level _campaignLevel;
        [SerializeField, ShowIf(nameof(_basedOnCampaignLevel))] private CompareLevelResult _levelCompareResult;
        [SerializeField] private List<BaseTutorialStep> _tutorialSteps;
        [SerializeField] private List<BaseTutorialCondition> _extraTutorialConditions;

        private TutorialPlayerData _tutorialPlayerData;
        private int _lastRunningStep = 0;

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

        #region PUBLIC_FUNCTIONS
        public void MarkComplete()
        {
            State = TutorialState.Completed;
        }

        public void LoadData()
        {
            _tutorialPlayerData = TutorialManager.Instance.GetTutorialPlayerData(TutorialType);
            if (_tutorialPlayerData == null)
            {
                _tutorialPlayerData = new TutorialPlayerData() { tutorialType = _tutorialType, tutorialState = TutorialState.NotComplete };
                TutorialManager.Instance.SetTutorialPlayerData(_tutorialPlayerData);
            }
            //CheckForForceComplete();
        }

        public void StartTutorial()
        {
            if (!CheckForTutorialRepeatCondition())
                return;

            if (!CheckForTutorialConditions() && State == TutorialState.Running)
            {
                State = TutorialState.Completed;
                return;
            }

            //if (State != TutorialState.Running)
            //    AnalyticsManager.Instance.GetHandler<TutorialAnalyticsHandler>().OnTutorialStart(_tutorialType);

            State = TutorialState.Running;
            IsRunning = true;
            //MainUIManager.Instance.ShowView<Blocker>();
            TutorialElementHandler.Instance.SetActivateBackGround(true, 0);
            //AnimationUtilityHandler.Instance.WaitAndCall(0.1f, () => GlobalDataSavingManager.Instance.StopDataSaving = true);
            StartNextStep();
        }

        public bool CanStartTutorial()
        {
            if (!CheckForTutorialRepeatCondition())
                return false;

            for (int i = 0; i < _preCompleteTutorialTypes.Count; i++)
            {
                if (!TutorialManager.Instance.IsTutorialCompleted(_preCompleteTutorialTypes[i]))
                    return false;
            }

            return CanStartBasedOnLevel() && CheckForTutorialConditions();
        }

        public void ForceStopTutorial()
        {
            if (State == TutorialState.Running)
            {
                State = TutorialState.Completed;
                TutorialManager.RaiseOnTutorialComplete(_tutorialType);
                TutorialElementHandler.Instance.ResetTutorialView();
            }
        }
        #endregion

        #region PRIVATE_FUNCTIONS

        private bool CheckForTutorialRepeatCondition()
        {
            if (_tutorialPlayRepeatConditionType == TutorialPlayRepeatConditionType.SINGLE_TIME && State != TutorialState.Completed)
                return true;
            else if (_tutorialPlayRepeatConditionType == TutorialPlayRepeatConditionType.MULTIPLE_TIME)
                return true;

            return false;
        }

        private bool CheckForTutorialConditions()
        {
            if (_extraTutorialConditions.Find(x => !x.IsTutorialConditionPassed()) != null)
                return false;

            return true;
        }

        private void ResponseToCompleteStep()
        {
            _lastRunningStep++;
            if (_tutorialSteps.Count <= _lastRunningStep)
            {
                IsRunning = false;

                State = TutorialState.Completed;
                TutorialElementHandler.Instance.ResetTutorialView();
                //AnalyticsManager.Instance.GetHandler<TutorialAnalyticsHandler>().OnTutorialEnd(_tutorialType);
                TutorialManager.RaiseOnTutorialComplete(_tutorialType);
                //GlobalDataSavingManager.Instance.StopDataSaving = false;
            }
            else
            {
                StartNextStep();
            }
        }

        private void StartNextStep()
        {
            if (_lastRunningStep < _tutorialSteps.Count)
                _tutorialSteps[_lastRunningStep].StartStep(ResponseToCompleteStep);
            else
                ResponseToCompleteStep();
        }

        private bool CanStartBasedOnLevel()
        {
            return true;
        }
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
                var step = transform.GetChild(i).GetComponent<BaseTutorialStep>();
                if (step != null)
                    _tutorialSteps.Add(step);
            }
        }
#endif
        #endregion
    }

    public enum TutorialPlayRepeatConditionType
    {
        SINGLE_TIME,
        MULTIPLE_TIME
    }
}
