using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tag.nut_sort {
    public class TutorialManager : SerializedManager<TutorialManager>
    {
        #region PROPERTIES
        public bool CanPlayTutorial { get { return canPlayTutorial; } set { canPlayTutorial = value; } }
        #endregion

        #region PRIVATE_VARS
        [SerializeField] private bool canPlayTutorial = true;
        private List<Tutorial> _tutorials = new();
        private List<NonIntrusiveTutorial> _nonIntrusiveTutorials = new();

        private TutorialsPlayerData _tutorialsPlayerData;
        private List<TutorialPlayerData> TutorialPlayerDataList => _tutorialsPlayerData.tutorialPlayerDatas;

        #endregion

        #region UNITY_CALLBACKS

        public override void Awake()
        {
            base.Awake();
            LoadData();
            OnLoadingDone();
        }

        // TODO : Set conditions for already running tutorials
        //public void Start()
        //{
        //    CheckForRunningTutorial();
        //}

        #endregion

        #region PUBLIC_FUNCTIONS

        public void CheckForTutorialsToStart()
        {
            if (!canPlayTutorial)
                return;
            for (int i = 0; i < _tutorials.Count; i++)
            {
                if (_tutorials[i].TutorialPlayType == TutorialPlayType.Auto && _tutorials[i].CanStartTutorial())
                {
                    _tutorials[i].StartTutorial();
                    break;
                }
            }
        }

        public void ActiveTutorial(TutorialType tutorialType)
        {
            if (Tutorial.IsRunning || NonIntrusiveTutorial.IsRunning)
                return;

            var tutorial = GetTutorial(tutorialType);
            if (tutorial != null && tutorial.CanStartTutorial())
                tutorial.StartTutorial();
        }

        public void ActiveNITutorial(NonIntrusiveTutorialType tutorialType)
        {
            if (Tutorial.IsRunning || NonIntrusiveTutorial.IsRunning)
                return;

            var tutorialNI = GetNonIntrusiveTutorial(tutorialType);
            if (tutorialNI != null && tutorialNI.CanStartTutorial())
                tutorialNI.StartTutorial();
        }

        public NonIntrusiveTutorial GetNonIntrusiveTutorial(NonIntrusiveTutorialType tutorialType)
        {
            return _nonIntrusiveTutorials.Find(x => x.TutorialType == tutorialType);
        }

        public void AddTutorialToList(Tutorial tutorial)
        {
            _tutorials ??= new List<Tutorial>();
            if (!_tutorials.Contains(tutorial))
                _tutorials.Add(tutorial);
            tutorial.LoadData();
        }

        public void AddTutorialToList(NonIntrusiveTutorial tutorial)
        {
            _nonIntrusiveTutorials ??= new List<NonIntrusiveTutorial>();
            if (!_nonIntrusiveTutorials.Contains(tutorial))
                _nonIntrusiveTutorials.Add(tutorial);
            tutorial.LoadData();
        }

        public TutorialPlayerData GetTutorialPlayerData(TutorialType type)
        {
            return TutorialPlayerDataList.Find(x => x.tutorialType == type);
        }

        public void SetTutorialPlayerData(TutorialPlayerData tutorialPlayerData)
        {
            TutorialPlayerDataList.Add(tutorialPlayerData);
            SaveData();
        }

        public void StartNonIntrusiveTutorials()
        {
            _nonIntrusiveTutorials.ForEach(x =>
            {
                if (x.CanStartTutorial())
                    x.StartNonIntrusiveTutorialTrigger();
            });
        }

        public void RemoveTutorialFromList(Tutorial tutorial)
        {
            _tutorials.Remove(tutorial);
        }

        public void RemoveTutorialFromList(NonIntrusiveTutorial tutorial)
        {
            _nonIntrusiveTutorials.Remove(tutorial);
        }

        public bool IsAnyTutorialActive()
        {
            for (int i = 0; i < TutorialPlayerDataList.Count; i++)
            {
                if (TutorialPlayerDataList[i].tutorialState == TutorialState.Running)
                    return true;
            }
            return false;
        }

        public bool IsTutorialActive(TutorialType tutorialType)
        {
            for (int i = 0; i < TutorialPlayerDataList.Count; i++)
            {
                if (TutorialPlayerDataList[i].tutorialType == tutorialType && TutorialPlayerDataList[i].tutorialState == TutorialState.Running)
                    return true;
            }
            return false;
        }

        public Tutorial GetTutorial(TutorialType tutorialType)
        {
            return _tutorials.Find(x => x.TutorialType == tutorialType);
        }

        public bool CanStartTutorial(TutorialType tutorialType)
        {
            Tutorial tutorial = _tutorials.Find(x => x.TutorialType == tutorialType);
            if (tutorial == null)
                return false;

            return tutorial.CanStartTutorial();
        }

        public bool IsTutorialCompleted(TutorialType tutorialType)
        {
            return GetTutorialState(tutorialType) == TutorialState.Completed;
        }

        public void SaveData()
        {
            PlayerPersistantData.SetTutorialsPlayerPersistantData(_tutorialsPlayerData);
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        private void LoadData()
        {
            _tutorialsPlayerData = PlayerPersistantData.GetTutorialsPlayerPersistantData();
            _tutorialsPlayerData ??= new TutorialsPlayerData();
        }

        //private void CheckForRunningTutorial()
        //{
        //    TutorialType tutorialType = GetTutorialType(TutorialState.Running);
        //    if (tutorialType != TutorialType.None)
        //    {
        //        if (GetSceneBy(tutorialType) == SceneHandler.MetaScene)
        //            StartTutorialAfterDelay();
        //        else
        //            GameManager.Instance.LoadMainGameplay(StartTutorialAfterDelay, false);
        //    }

        //    void StartTutorialAfterDelay()
        //    {
        //        StartTutorial(tutorialType);
        //        AnimationUtilityHandler.Instance.WaitAndCall(1, () => );
        //    }
        //}

        public TutorialType GetTutorialType(TutorialState tutorialState)
        {
            for (int i = 0; i < TutorialPlayerDataList.Count; i++)
            {
                if (TutorialPlayerDataList[i].tutorialState == tutorialState)
                    return TutorialPlayerDataList[i].tutorialType;
            }
            return TutorialType.None;
        }

        private void StartTutorial(TutorialType tutorialType)
        {
            for (int i = 0; i < _tutorials.Count; i++)
            {
                if (tutorialType == _tutorials[i].TutorialType)
                {
                    _tutorials[i].StartTutorial();
                    break;
                }
            }
        }

        private TutorialState GetTutorialState(TutorialType tutorialType)
        {
            for (int i = 0; i < TutorialPlayerDataList.Count; i++)
            {
                if (tutorialType == TutorialPlayerDataList[i].tutorialType)
                    return TutorialPlayerDataList[i].tutorialState;
            }
            return TutorialState.NotComplete;
        }

        private void StartNITutorial(NonIntrusiveTutorialType tutorialType)
        {
            for (int i = 0; i < _nonIntrusiveTutorials.Count; i++)
            {
                if (tutorialType == _nonIntrusiveTutorials[i].TutorialType)
                {
                    _nonIntrusiveTutorials[i].StartTutorial();
                    break;
                }
            }
        }
        #endregion

        #region EVENT_HANDLERS

        public delegate void OnTutorialComplete(TutorialType tutorialType);

        public static event OnTutorialComplete onTutorialComplete;

        public static void RaiseOnTutorialComplete(TutorialType tutorialType)
        {
            onTutorialComplete?.Invoke(tutorialType);
        }

        public delegate void OnNonIntrusiveTutorialComplete(NonIntrusiveTutorialType tutorialType);

        public static event OnNonIntrusiveTutorialComplete onNonIntrusiveTutorialComplete;

        public static void RaiseOnNonIntrusiveTutorialComplete(NonIntrusiveTutorialType tutorialType)
        {
            onNonIntrusiveTutorialComplete?.Invoke(tutorialType);
        }

        public static event OnNonIntrusiveTutorialComplete onNonIntrusiveTutorialAbort;

        public static void RaiseOnNonIntrusiveTutorialAbort(NonIntrusiveTutorialType tutorialType)
        {
            onNonIntrusiveTutorialAbort?.Invoke(tutorialType);
        }

        #endregion

        #region UI_CALLBACKS

        //public void OnPlayButtonClick()
        //{
        //    MainUIManager.Instance.GetView<HomeView>().OnPlayButtonClick();
        //}

        #endregion
    }

    public enum TutorialType
    {
        None = 0,
        FirstLevel_Gameplay = 1,
        SecondLevel_Gameplay = 2,
    }

    public enum TutorialState
    {
        NotComplete = 0,
        Running = 1,
        Completed = 2,
    }

    public enum TutorialPlayType
    {
        Auto = 0,
        Manual = 1,
    }
}