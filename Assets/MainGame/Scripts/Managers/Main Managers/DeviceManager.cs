using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tag.nut_sort {
    public class DeviceManager : Manager<DeviceManager>
    {
        #region private methods

        //[SerializeField] private bool isLogEnable;
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
            StartCoroutine(Wait());
        }

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

        IEnumerator Wait()
        {
            yield return null;
            //if (IsDeveloper())
            //{
            //    Debug.unityLogger.filterLogType = LogType.Log;
            //}
            //else
            //{
            //    if (isLogEnable)
            //        Debug.unityLogger.filterLogType = LogType.Log;
            //    else
            //        Debug.unityLogger.filterLogType = LogType.Error;
            //}
            isInit = true;
        }
    }
}
