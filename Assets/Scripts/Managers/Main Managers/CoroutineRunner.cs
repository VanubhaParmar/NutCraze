using System;
using System.Collections;
using UnityEngine;

namespace Tag.NutSort
{
    public class CoroutineRunner : MonoBehaviour
    {
        #region PROPERTIES

        private static CoroutineRunner _coroutineRunner;

        public static CoroutineRunner Instance
        {
            get
            {
                if (_coroutineRunner == null)
                {
                    GameObject obj = new GameObject("CoroutineRunner");
                    obj.AddComponent<CoroutineRunner>();
                }
                return _coroutineRunner;
            }
        }

        #endregion

        #region UNITY_CALLBACKS

        private void Awake()
        {
            _coroutineRunner = this;
        }

        private void OnDestroy()
        {
            AllCoroutiesStop();
            if (_coroutineRunner != null)
                _coroutineRunner = null;
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        public Coroutine WaitAndInvoke(DateTime endTime, Action action)
        {
            return StartCoroutine(WaitForEndAndInvoke(endTime, action));
        }

        public Coroutine Wait(float wait, Action action)
        {
            return StartCoroutine(WaitTime(wait, action));
        }

        public Coroutine WaitForFrame(Action action)
        {
            return StartCoroutine(WaitTimeForFrame(action));
        }

        public Coroutine InvokeRepeat(float wait, Action action)
        {
            return StartCoroutine(RepeatInvoke(action, wait));
        }

        public Coroutine CoroutineStart(IEnumerator coroutine)
        {
            return StartCoroutine(coroutine);
        }

        public void CoroutineStop(Coroutine coroutine)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }

        public void AllCoroutiesStop()
        {
            StopAllCoroutines();
        }

        #endregion

        #region CO-ROUTINES

        IEnumerator WaitTime(float wait, Action action)
        {
            yield return new WaitForSeconds(wait);
            action?.Invoke();
        }

        IEnumerator WaitTimeForFrame(Action action)
        {
            yield return new WaitForEndOfFrame();
            action?.Invoke();
        }

        IEnumerator RepeatInvoke(Action action, float wait)
        {
            WaitForSeconds w = new WaitForSeconds(wait);
            while (true)
            {
                yield return w;
                action?.Invoke();
            }
        }

        IEnumerator WaitForEndAndInvoke(DateTime endTime, Action action)
        {
            DateTime startTime = endTime;
            WaitForSeconds seconds = new WaitForSeconds(1);
            TimeSpan timeSpan = startTime - TimeManager.Now;
            while (timeSpan.TotalSeconds > 0)
            {
                timeSpan = startTime - TimeManager.Now;
                yield return seconds;
            }
            action?.Invoke();
        }

        #endregion
    }
}