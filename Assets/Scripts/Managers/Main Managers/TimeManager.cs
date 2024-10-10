using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort
{
    public class TimeManager : SerializedManager<TimeManager>
    {
        #region PUBLIC_VARS

        public static DateTime Now => DateTime.Now;
        public static DateTime UtcNow => DateTime.UtcNow;

        #endregion

        #region PRIVATE_VARS

        private Coroutine timeCoroutine;
        private List<Action<DateTime>> timerTickEvent = new List<Action<DateTime>>();
        private List<Action<DateTime>> realtimeTimerTickEvent = new List<Action<DateTime>>();
        [ShowInInspector] private List<Action<bool, TimeSpan>> onTimerPause = new List<Action<bool, TimeSpan>>();
        private DateTime pauseTime;

        #endregion

        #region PROPERTIES

        #endregion

        #region UNITY_CALLBACKS

        public override void Awake()
        {
            base.Awake();
            StartCoroutine(InfiniteTimer());
            StartCoroutine(InfiniteTimerRealTime());
            OnLoadingDone();
        }

        public override void OnDestroy()
        {
            if (timeCoroutine != null)
                StopCoroutine(timeCoroutine);
            base.OnDestroy();
        }

        public void RegisterOnTimerPause(Action<bool, TimeSpan> action)
        {
            if (!onTimerPause.Contains(action))
                onTimerPause.Add(action);
        }

        public void DeregisterOnTimerPause(Action<bool, TimeSpan> action)
        {
            if (onTimerPause.Contains(action))
                onTimerPause.Remove(action);
        }
        public void RegisterTimerTickEvent(Action<DateTime> action)
        {
            if (!timerTickEvent.Contains(action))
            {
                timerTickEvent.Add(action);
            }
        }

        public void DeRegisterTimerTickEvent(Action<DateTime> action)
        {
            if (timerTickEvent.Contains(action))
            {
                timerTickEvent.Remove(action);
            }
        }

        public void InvokeTimerTickEvent(DateTime time)
        {
            for (int i = 0; i < timerTickEvent.Count; i++)
            {
                timerTickEvent[i].Invoke(time);
            }
        }

        public void RegisterRealtimeTimerTickEvent(Action<DateTime> action)
        {
            if (!realtimeTimerTickEvent.Contains(action))
            {
                realtimeTimerTickEvent.Add(action);
            }
        }

        public void DeRegisterRealtimeTimerTickEvent(Action<DateTime> action)
        {
            if (realtimeTimerTickEvent.Contains(action))
            {
                realtimeTimerTickEvent.Remove(action);
            }
        }

        public void InvokeRealtimeTimerTickEvent(DateTime time)
        {
            realtimeTimerTickEvent.RemoveAll(x => x == null);
            for (int i = 0; i < realtimeTimerTickEvent.Count; i++)
            {
                realtimeTimerTickEvent[i]?.Invoke(time);
            }
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                pauseTime = Now;
            OnTimerPause(pauseStatus, Now - pauseTime);
        }

        private void OnTimerPause(bool pauseStatus, TimeSpan timeSpan)
        {
            for (int i = 0; i < onTimerPause.Count; i++)
            {
                onTimerPause[i]?.Invoke(pauseStatus, timeSpan);
            }
        }

        #endregion

        #region CO-ROUTINES

        IEnumerator InfiniteTimer()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(1);
            while (true)
            {
                yield return waitForSeconds;
                InvokeTimerTickEvent(Now);
            }
        }

        IEnumerator InfiniteTimerRealTime()
        {
            WaitForSecondsRealtime waitForSeconds = new WaitForSecondsRealtime(1f);
            while (true)
            {
                yield return waitForSeconds;
                InvokeRealtimeTimerTickEvent(Now);
            }
        }

        #endregion
    }
}