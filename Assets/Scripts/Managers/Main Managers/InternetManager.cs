using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Tag.NutSort
{
    public class InternetManager : Manager<InternetManager>
    {
        #region private veriabels

        [SerializeField] private string netCheckURL;
        [SerializeField] private float internetCheckingInterval;

        private WaitForSeconds waitForSeconds;

        #endregion

        #region unitycallback
        private void Start()
        {
            waitForSeconds = new WaitForSeconds(internetCheckingInterval);
#if UNITY_EDITOR
            OnLoadingDone();
#else
            CheckNetConnection(OnNetConnectionResponse);
#endif
        }
        #endregion

        #region private methods

        private void OnNetConnectionResponse(bool isNetAvailable)
        {
            //if (isNetAvailable)
                OnLoadingDone();
            //else
                //GlobalUIManager.Instance.GetView<NoInternetView>().Show(() => { CheckNetConnection(OnNetConnectionResponse); });
        }

        public bool IsReachableToNetwork()
        {
            return Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork || Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        }

        #endregion

        #region public methods

        public void CheckNetConnection(Action<bool> onCheckingDone)
        {
            StartCoroutine(CheckInternetConnection(onCheckingDone));
        }

        public void CheckNetConnection(Action onInternetRestore)
        {
            if (IsReachableToNetwork())
            {
                onInternetRestore?.Invoke();
                return;
            }

            CheckNetConnection(b =>
            {
                //if (b)
                    onInternetRestore?.Invoke();
                //else
                    //GlobalUIManager.Instance.GetView<NoInternetView>().Show(() => { CheckNetConnection(onInternetRestore); });
            });
        }

        public void StartInternetCheckingInRepeat()
        {
            StartCoroutine(CheckInternetInRepeat());
        }

        #endregion

        #region Coroutine

        IEnumerator CheckInternetConnection(Action<bool> action)
        {
            bool result;
            using (var request = UnityWebRequest.Head(netCheckURL))
            {
                request.timeout = 5;
                yield return request.SendWebRequest();
                result = request.result != UnityWebRequest.Result.ConnectionError && request.result != UnityWebRequest.Result.ProtocolError && request.responseCode == 200;
            }
            action(result);
        }

        IEnumerator CheckInternetInRepeat()
        {
            yield return waitForSeconds;
            if (IsReachableToNetwork())
            {
                StartInternetCheckingInRepeat();
            }
            else
            {
                CheckNetConnection(StartInternetCheckingInRepeat);
            }
        }

        #endregion
    }
}
