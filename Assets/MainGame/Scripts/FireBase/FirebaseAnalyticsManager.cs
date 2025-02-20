using System;
using System.Collections.Generic;
using Firebase.Analytics;
using UnityEngine;
using FireAnalytics = Firebase.Analytics.FirebaseAnalytics;

namespace Tag.NutSort {
    public class FirebaseAnalyticsManager : MonoBehaviour
    {
        #region PRIVATE_VARS

        private bool initialized;
        private List<FireBaseEvent> _pendingEvents = new();

        #endregion

        #region PUBLIC_FUNCTIONS

        public void Init()
        {
            FireAnalytics.SetAnalyticsCollectionEnabled(true);
            // Set the user's sign up method.
            FireAnalytics.SetUserProperty(FireAnalytics.UserPropertySignUpMethod, "Guest");
            // Set the user ID.
            string userID = SystemInfo.deviceUniqueIdentifier;
            FireAnalytics.SetUserId(userID);
            // Set default session duration values.
            FireAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 5, 0));

            //LogEvent("Game Start");
            initialized = true;
            FirePendingEvents();
        }

        public void Log_Event(string eventName, Dictionary<string, string> keyValuePair = null)
        {
            if (!initialized)
            {
                _pendingEvents.Add(new FireBaseEvent { eventName = eventName, keyValuePair = keyValuePair });
            }
            else if (keyValuePair == null)
            {
                LogEvent(eventName);
            }
            else
            {
                LogEvent(eventName, keyValuePair);
            }
        }

        public void LogEvent(string eventName, Parameter[] eventParameters)
        {
            FireAnalytics.LogEvent(eventName, eventParameters);
        }
        #endregion

        #region PRIVATE_FUNCTIONS

        private void LogEvent(string eventName)
        {
            FireAnalytics.LogEvent(eventName);
        }

        private void LogEvent(string eventName, IDictionary<string, string> keyValuePair)
        {
            FireAnalytics.LogEvent(eventName, ConvertToArrayOfParameters(keyValuePair));
        }

        private Parameter[] ConvertToArrayOfParameters(IDictionary<string, string> data)
        {
            Parameter[] parameters = new Parameter[data.Count];
            int count = 0;
            foreach (var item in data)
            {
                parameters[count] = new Parameter(item.Key, item.Value);
                count++;
            }
            return parameters;
        }

        private void FirePendingEvents()
        {
            foreach (var item in _pendingEvents)
            {
                LogEvent(item.eventName, item.keyValuePair);
            }
            _pendingEvents.Clear();
        }

        #endregion
    }
    [Serializable]
    public class FireBaseEvent
    {
        public string eventName;
        public Dictionary<string, string> keyValuePair;
    }
}
