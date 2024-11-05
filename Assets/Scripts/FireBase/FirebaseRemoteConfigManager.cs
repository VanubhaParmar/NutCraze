using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Tag.NutSort
{
    public class FirebaseRemoteConfigManager : MonoBehaviour
    {
        #region PUBLIC_VARIABLES

        public ConfigType configType;
        public bool isDubugRemoteString;
        public bool isRemoteDataFetched;
        public BaseConfig applicationVersionUpdateRCConfig;

        public RemoteConfigFetchTaskStatus remoteConfigFetchTaskStatus;
        public List<BaseConfig> baseConfigList;

        #endregion

        #region PRIVATE_VARIABLES

        private Action _onCompleteInit;

        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS

        public void Init(Action onCompleteInit)
        {
            _onCompleteInit = onCompleteInit;
            StartCoroutine(WaitAndCallAction(2f));
            Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(GetDefaultValues())
              .ContinueWithOnMainThread(task =>
              {
                  // [END set_defaults]
                  Debug.Log("RemoteConfig configured and ready!");
                  FetchDataAsync();
              });
        }

        // [START fetch_async]
        // Start a fetch request.
        // FetchAsync only fetches new data if the current data is older than the provided
        // timespan.  Otherwise it assumes the data is "recent enough", and does nothing.
        // By default the timespan is 12 hours, and for production apps, this is a good
        // number. For this example though, it's set to a timespan of zero, so that
        // changes in the console will always show up immediately.
        public System.Threading.Tasks.Task FetchDataAsync()
        {
            Debug.Log("Fetching data...");
            System.Threading.Tasks.Task fetchTask =
            Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);//Set TimeSpan.Zero for testing
            return fetchTask.ContinueWithOnMainThread(FetchComplete);
        }
        //[END fetch_async]

        // Display the currently loaded data.  If fetch has been called, this will be
        // the data fetched from the server.  Otherwise, it will be the defaults.
        // Note:  Firebase will cache this between sessions, so even if you haven't
        // called fetch yet, if it was called on a previous run of the program, you
        //  will still have data from the last time it was run.
        //public void DisplayData()
        //{
        //    Debug.Log("Current Data:");
        //    Debug.Log("config_test_string: " +
        //             Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
        //             .GetValue("config_test_string").StringValue);
        //    Debug.Log("config_test_int: " +
        //             Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
        //             .GetValue("config_test_int").LongValue);
        //    Debug.Log("config_test_float: " +
        //             Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
        //             .GetValue("config_test_float").DoubleValue);
        //    Debug.Log("config_test_bool: " +
        //             Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
        //             .GetValue("config_test_bool").BooleanValue);
        //}

        public void FetchAndUpdateData()
        {
            for (int i = 0; i < baseConfigList.Count; i++)
            {
                var dataString = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(baseConfigList[i].GetRemoteId(configType)).StringValue;
                baseConfigList[i].Init(dataString);
                LogString(dataString);
            }
        }

        #endregion

        #region PRIVATE_FUNCTIONS
        private void LogString(string data)
        {
            if (isDubugRemoteString)
            {
                Debug.Log(data);
            }
        }

        private Dictionary<string, object> GetDefaultValues()
        {
            Dictionary<string, object> defaults = new Dictionary<string, object>();

            defaults.Add(applicationVersionUpdateRCConfig.GetRemoteId(configType), applicationVersionUpdateRCConfig.GetDefaultString());
            for (int i = 0; i < baseConfigList.Count; i++)
            {
                defaults.Add(baseConfigList[i].GetRemoteId(configType), baseConfigList[i].GetDefaultString());
            }
            return defaults;
        }

        private void FetchComplete(System.Threading.Tasks.Task fetchTask)
        {
            isRemoteDataFetched = true;
            _onCompleteInit?.Invoke();
            _onCompleteInit = null;
            if (fetchTask.IsCanceled)
            {
                Debug.Log("Fetch canceled.");
            }
            else if (fetchTask.IsFaulted)
            {
                Debug.Log("Fetch encountered an error.");
            }
            else if (fetchTask.IsCompleted)
            {
                Debug.Log("Fetch completed successfully!");
            }

            var info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;
            switch (info.LastFetchStatus)
            {
                case Firebase.RemoteConfig.LastFetchStatus.Success:
                    StartCoroutine(SetData());
                    break;
                case Firebase.RemoteConfig.LastFetchStatus.Failure:
                    remoteConfigFetchTaskStatus = RemoteConfigFetchTaskStatus.ERROR;
                    StartCoroutine(SetData());
                    switch (info.LastFetchFailureReason)
                    {
                        case Firebase.RemoteConfig.FetchFailureReason.Error:
                            Debug.Log("Fetch failed for unknown reason");
                            break;
                        case Firebase.RemoteConfig.FetchFailureReason.Throttled:
                            Debug.Log("Fetch throttled until " + info.ThrottledEndTime);
                            break;
                    }
                    break;
                case Firebase.RemoteConfig.LastFetchStatus.Pending:
                    remoteConfigFetchTaskStatus = RemoteConfigFetchTaskStatus.PENDING;
                    StartCoroutine(SetData());
                    Debug.Log("Latest Fetch call still pending.");
                    break;
            }
        }

        private void UpdateApplicationVersionAndData()
        {
            applicationVersionUpdateRCConfig.Init(Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(applicationVersionUpdateRCConfig.GetRemoteId(configType)).StringValue);
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

        private IEnumerator WaitAndCallAction(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            _onCompleteInit?.Invoke();
            _onCompleteInit = null;
        }

        private IEnumerator SetData()
        {
            Task task = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync()
                    .ContinueWithOnMainThread(task =>
                    {
                        /* Debug.Log(String.Format("Remote data loaded and ready (last fetch time {0}).",
                                    info.FetchTime));*/
                        remoteConfigFetchTaskStatus = RemoteConfigFetchTaskStatus.COMPLETED;
                        UpdateApplicationVersionAndData();
                        FetchAndUpdateData();

                        RaiseOnRCValuesFetched();
                    });
            while (!task.IsCompleted)
                yield return null;

            if (task.Exception != null)
            {
                Debug.LogException(task.Exception);
            }
        }

        #endregion

        #region UI_CALLBACKS
        #endregion

        public enum RemoteConfigFetchTaskStatus
        {
            NONE,
            COMPLETED,
            ERROR,
            PENDING
        }
    }
}
