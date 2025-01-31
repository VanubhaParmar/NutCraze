using System;
using System.Collections;
using System.Collections.Generic;
using GameAnalyticsSDK;
using UnityEngine;

namespace com.tag.nut_sort {
    public class GameAnalyticsManager : Manager<GameAnalyticsManager>
    {
        #region PRIVATE_VARS
        public ConfigType configType;
        public List<BaseConfig> baseConfigList;
        public bool IsRCValuesFetched => isRCValuesFetched;

        private Dictionary<GAEventType, List<string>> _pendingEvents = new();
        private bool isRCValuesFetched = false;

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

        //public T GetRemoteData<T>(string key, T defaultValue)
        //{
        //    key = GetRemoteConfigKey(key);
        //    try
        //    {
        //        if (GameAnalytics.IsRemoteConfigsReady())
        //        {
        //            string json = GameAnalytics.GetRemoteConfigsValueAsString(key);
        //            if (string.IsNullOrEmpty(json))
        //            {
        //                Debug.Log("<color=red> Try: GameAnalytics Key Remote Configs Null: " + key + " </color> _JSON:  : " + JsonConvert.SerializeObject(defaultValue));
        //                return defaultValue;
        //            }
        //            Debug.Log("<color=yellow>GameAnalytics Key: " + key + "</color> _JSON: " + json);
        //            return JsonConvert.DeserializeObject<T>(json);
        //        }
        //        Debug.Log("<color=red> Try: GameAnalytics Key Remote Configs Not Ready: " + key + " </color> _JSON:  : " + JsonConvert.SerializeObject(defaultValue));
        //        return defaultValue;
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError(e.Message);
        //        Debug.Log("<color=red> Catch: GameAnalytics Key: " + key + " </color> _JSON:  : " + JsonConvert.SerializeObject(defaultValue));
        //        return defaultValue;
        //    }
        //}

        public void FetchAndUpdateData()
        {
            for (int i = 0; i < baseConfigList.Count; i++)
            {
                if (GameAnalytics.IsRemoteConfigsReady())
                {
                    string dataString = GameAnalytics.GetRemoteConfigsValueAsString(baseConfigList[i].GetRemoteId(configType), baseConfigList[i].GetDefaultString());
                    baseConfigList[i].Init(dataString);
                    Debug.Log("<color=red> Catch: GameAnalytics Key: " + baseConfigList[i].GetRemoteId(configType) + " </color> _JSON:  : " + dataString);
                }
            }

            isRCValuesFetched = true;
            RaiseOnRCValuesFetched();
        }

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

#if !UNITY_EDITOR
            StartCoroutine(WaitForRemoteConfigToLoad());
#else
            isRCValuesFetched = true;
#endif
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

        #region EVENT_HANDLERS
        public delegate void RemoteConfigVoidEvents();
        public static event RemoteConfigVoidEvents onRCValuesFetched;

        public static void RaiseOnRCValuesFetched()
        {
            if (onRCValuesFetched != null)
                onRCValuesFetched();
        }
        #endregion

        #region COROUTINES
        IEnumerator WaitForRemoteConfigToLoad()
        {
            yield return null;
            while (!GameAnalytics.IsRemoteConfigsReady())
            {
                yield return new WaitForSecondsRealtime(2f);
            }

            if (GameAnalytics.IsRemoteConfigsReady())
                FetchAndUpdateData();
        }
        #endregion
    }
}