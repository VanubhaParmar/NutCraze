using GameAnalyticsSDK;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class GameplayManager : SerializedManager<GameplayManager>
    {
        #region PRIVATE_VARIABLES
        [SerializeField] Dictionary<GamePlayType, BaseGamePlayHandler> gameplayHandlerMapping = new Dictionary<GamePlayType, BaseGamePlayHandler>();
        [SerializeField] private BaseGamePlayHandler currentHandler;
        private List<Action<BaseScrew>> onScrewSortComplete = new List<Action<BaseScrew>>();
        #endregion

        #region PUBLIC_VARIABLES
        #endregion

        #region PROPERTIES
        public GameplayStateData GameplayStateData => currentHandler.GameplayStateData;
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            RegisterEvents();
            OnLoadingDone();
        }

        public override void OnDestroy()
        {
            DeRegisterEvents();
            base.OnDestroy();
        }

        private void RegisterEvents()
        {
            NutTransferHelper.Instance.RegisterOnNutTransferComplete(OnNutTransferComplete);
        }

        private void DeRegisterEvents()
        {
            NutTransferHelper.Instance.DeRegisterOnNutTransferComplete(OnNutTransferComplete);
        }
        #endregion

        #region PUBLIC_METHODS
        [Button]
        public void StartMainGameLevel()
        {
            currentHandler = gameplayHandlerMapping[GamePlayType.Main];
            LevelDataSO levelDataSO = LevelManager.Instance.GetLevelData(DataManager.PlayerLevel.Value);
            currentHandler.StartLevel(levelDataSO);
        }

        public virtual void StartNextOrCurrentLevel()
        {
            currentHandler.StartNextOrCurrentLevel();
        }

        public void OnLevelComplete()
        {
            currentHandler.OnLevelComplete();
        }

        public void OnExit()
        {
            currentHandler.OnExit();    
        }

        public void OnLevelFail()
        {
            currentHandler.OnLevelFail();
        }

        public void OnLevelFailRetry()
        {
            currentHandler.OnLevelFailRetry();
        }


        public void OnRetry()
        {
            currentHandler.OnRetry();
        }

        public void OnNutTransferComplete(BaseScrew fromScrew, BaseScrew toScrew, int nutsTransferred)
        {
            currentHandler.OnNutTransferComplete(fromScrew, toScrew, nutsTransferred);
        }

        public void OnScrewSortComplete(BaseScrew baseScrew)
        {
            InvokeOnScrewSortComplete(baseScrew);
            currentHandler.OnScrewSortComplete(baseScrew);
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

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region UNITY_EDITOR_FUNCTIONS
        #endregion
    }


}