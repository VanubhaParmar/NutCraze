using System;
using Firebase.Crashlytics;
using UnityEngine;

namespace com.tag.nut_sort {
    public class FirebaseCrashlyticsManager : MonoBehaviour
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public void Init()
        {
            string userID = SystemInfo.deviceUniqueIdentifier;
            Crashlytics.SetUserId(userID);
        }

        public void ThrowUncaughtException()
        {
            DebugLog("Causing a platform crash.");
            throw new InvalidOperationException("Uncaught exception created from UI.");
        }

        // Log a caught exception.
        public void LogCaughtException()
        {
            DebugLog("Catching an logging an exception.");
            try
            {
                throw new InvalidOperationException("This exception should be caught");
            }
            catch (Exception ex)
            {
                Crashlytics.LogException(ex);
            }
        }

        public void LogCaughtException(Exception ex)
        {
            Crashlytics.LogException(ex);
        }

        // Write to the Crashlytics session log
        public void WriteCustomLog(String s)
        {
            DebugLog("Logging message to Crashlytics session: " + s);
            Crashlytics.Log(s);
        }

        // Output text to the debug log text field, as well as the console.
        public void DebugLog(string s)
        {
            // print(s);
        }

        public void ThrowTestException()
        {
            throw new System.Exception("test exception please ignore");
        }
        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
