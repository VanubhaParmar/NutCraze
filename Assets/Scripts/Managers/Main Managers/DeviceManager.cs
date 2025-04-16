using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class DeviceManager : Manager<DeviceManager>
    {
        #region private methods
        // [SerializeField] private DeveloperDeviceRemoteConfig developerDeviceRemoteConfig;
        [SerializeField] private int tergetFPS = 60;
        [SerializeField] private List<string> deviceIds = new List<string>();
        private bool isInit;

        public bool IsInit { get => isInit; }
        public List<string> DeviceIds { get => deviceIds; }

        #endregion

        #region unity callback

        public override void Awake()
        {
            isInit = false;
            base.Awake();
            Application.targetFrameRate = tergetFPS;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            AssignDeveloperDevices();
#if UNITY_EDITOR
            Application.runInBackground = true;
#endif
        }

        //private void OnEnable()
        //{
        //    FirebaseRemoteConfigManager.onRCValuesFetched += FirebaseRemoteConfigManager_onRCValuesFetched;
        //}

        //private void OnDisable()
        //{
        //    FirebaseRemoteConfigManager.onRCValuesFetched -= FirebaseRemoteConfigManager_onRCValuesFetched;
        //}

        #endregion

        #region public methods
        [Button]
        public string GetDeviceID()
        {
#if UNITY_EDITOR || UNITY_ANDROID
            return SystemInfo.deviceUniqueIdentifier;
#else
            return "";
#endif
        }

        public bool IsDeveloper()
        {
            return DeviceIds.Contains(GetDeviceID());
        }
        #endregion

        #region PRIVATE_METHODS
        private void AssignDeveloperDevices()
        {
            if (deviceIds == null)
                return;
            if (!deviceIds.Contains(DeveloperDeviceIds.Vanrajsinh))
                deviceIds.Add(DeveloperDeviceIds.Vanrajsinh);
        }

        //private void FirebaseRemoteConfigManager_onRCValuesFetched()
        //{
        //    if (developerDeviceRemoteConfig == null)
        //        return;
        //    List<string> remoteDeviceIds = developerDeviceRemoteConfig.GetValue<List<string>>();
        //    if (deviceIds == null)
        //        deviceIds = new List<string>();
        //    for (int i = 0; i < remoteDeviceIds.Count; i++)
        //    {
        //        if (!deviceIds.Contains(remoteDeviceIds[i]))
        //            deviceIds.Add(remoteDeviceIds[i]);
        //    }
        //}
        #endregion
    }
}
