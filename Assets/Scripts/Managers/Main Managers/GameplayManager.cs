using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class GameplayManager : SerializedManager<GameplayManager>
    {
        #region PRIVATE_VARIABLES
        [SerializeField] private Dictionary<GamePlayType, BaseGameplayHelper> gameplayMapping = new Dictionary<GamePlayType, BaseGameplayHelper>();
        private BaseGameplayHelper currentGameplay;

        private List<Action<BaseScrew>> onScrewSortComplete = new List<Action<BaseScrew>>();
        #endregion

        #region PUBLIC_VARIABLES
        #endregion

        #region PROPERTIES
        public GameplayStateData GameplayStateData => currentGameplay.GameplayStateData;
        public bool IsPlayingLevel => currentGameplay.IsPlayingLevel;
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            foreach (var item in gameplayMapping)
                item.Value.Init(this);
            OnLoadingDone();
        }
        #endregion

        #region PUBLIC_METHODS
        public void StartMainGamePlay()
        {
            currentGameplay = gameplayMapping[GamePlayType.Main];
            currentGameplay.StartGameplay();
        }


#if UNITY_EDITOR
        public void StartEditorGamePlay()
        {
            currentGameplay = gameplayMapping[GamePlayType.Editor];
            currentGameplay.StartGameplay();
        }
#endif

        public void RestartGamePlay()
        {
            currentGameplay.RestartGameplay();
        }
        public void EndGamePlay()
        {
            currentGameplay.EndGameplay();
        }

        public void LoadLevel(LevelDataSO levelDataSO)
        {
            currentGameplay.LoadLevel(levelDataSO);
        }


        public void OnScrewSortComplete(BaseScrew baseScrew)
        {
            InvokeOnScrewSortComplete(baseScrew);
            currentGameplay.OnScrewSortComplete(baseScrew);
        }

        public void RegisterOnScrewSortComplete(Action<BaseScrew> action)
        {
            if (!onScrewSortComplete.Contains(action))
                onScrewSortComplete.Add(action);
        }

        public void DeRegisterOnScrewSortComplete(Action<BaseScrew> action)
        {
            if (onScrewSortComplete.Contains(action))
                onScrewSortComplete.Remove(action);
        }
        #endregion

        #region PRIVATE_METHODS
        private void InvokeOnScrewSortComplete(BaseScrew screw)
        {
            for (int i = 0; i < onScrewSortComplete.Count; i++)
                onScrewSortComplete[i]?.Invoke(screw);
        }

      
        #endregion

        #region ANALYTICS_EVENTS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region UNITY_EDITOR_FUNCTIONS
        #endregion
    }

    public enum GamePlayType
    {
        Main,
        Editor,
    }
}