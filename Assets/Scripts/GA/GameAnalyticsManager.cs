using System.Collections.Generic;
using GameAnalyticsSDK;

namespace Tag.NutSort
{
    public class GameAnalyticsManager : Manager<GameAnalyticsManager>
    {
        #region PRIVATE_VARS

        private Dictionary<GAEventType, List<string>> _pendingEvents = new();

        #endregion

        #region UNITY_CALLBACKS

        public override void Awake()
        {
            base.Awake();
            Init();
            OnLoadingDone();
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        public void Log_Event(GAEventType gAEventType, string eventName)
        {
            if (GameAnalytics.Initialized)
                Log_EventInSDK(gAEventType, eventName);
            else
                AddPendingEvent(gAEventType, eventName);
        }

        public void Log_Event(GAEventType gAEventType, string eventName, float parameterValue)
        {
            if (GameAnalytics.Initialized)
                Log_EventInSDK(gAEventType, eventName, parameterValue);
        }

        private void Log_EventInSDK(GAEventType gAEventType, string eventName)
        {
            switch (gAEventType)
            {
                case GAEventType.Design:
                    GameAnalytics.NewDesignEvent(eventName);
                    break;
            }
        }

        private void Log_EventInSDK(GAEventType gAEventType, string eventName, float parameterValue)
        {
            switch (gAEventType)
            {
                case GAEventType.Design:
                    GameAnalytics.NewDesignEvent(eventName, parameterValue);
                    break;
            }
        }

        private void AddPendingEvent(GAEventType gAEventType, string eventName)
        {
            if(_pendingEvents.ContainsKey(gAEventType))
                _pendingEvents[gAEventType].Add(eventName);
            else
                _pendingEvents.Add(gAEventType, new List<string> { eventName });
        }

        private void Init()
        {
            GameAnalytics.Initialize();
            FirePendingEvents();
        }

        private void FirePendingEvents()
        {
            foreach (var eventName in _pendingEvents)
            {
                foreach(var eventVal in eventName.Value)
                {
                    Log_EventInSDK(eventName.Key, eventVal);
                }
            }
            _pendingEvents.Clear();
        }

        #endregion
    }
}