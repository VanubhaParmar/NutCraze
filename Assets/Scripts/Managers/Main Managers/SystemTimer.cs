using System;
using System.Collections;
using UnityEngine;

namespace Tag.NutSort
{
    public class SystemTimer
    {
        #region PUBLIC_FUNCTIONS
        //public bool IsRunning => _systemTimerCoroutine != null;
        public bool IsRunning => isRunning;
        #endregion

        #region PRIVATE_VARIABLES

        //private MonoBehaviour _targetSystemMonobehaviour;
        private DateTime _timerEndDateTime = DateTime.MinValue;
        private DateTime _timerStartDateTime = DateTime.MaxValue;
        private Action _actionToCallOnTimerOver;

        //private Coroutine _systemTimerCoroutine;
        private bool isRunning = false;

        private Action<string> _timerTickStringEvents = null;
        private Action<double> _timerTickDoubleEvents = null;
        private Action _timerTickEvent = null;

        #endregion

        #region CONSTRUCTORS

        public SystemTimer() { }

        //public SystemTimer(MonoBehaviour targetSystemMonobehaviour)
        //{
        //    _targetSystemMonobehaviour = targetSystemMonobehaviour;
        //}

        #endregion

        #region PUBLIC_METHODS

        //public void StartSystemTimer()
        //{
        //    StopSystemTimer();
        //    _systemTimerCoroutine = _targetSystemMonobehaviour.StartCoroutine(EndLessTimerCoroutine());
        //}

        public void StartSystemTimer(DateTime timerEndDateTime, Action actionToCallOnTimerOver)
        {
            StopSystemTimer();

            _timerEndDateTime = timerEndDateTime;
            _actionToCallOnTimerOver = actionToCallOnTimerOver;

            //_systemTimerCoroutine = _targetSystemMonobehaviour.StartCoroutine(TimerCoroutine());
            StartTimerTick();
        }

        public void StartSystemTimer(DateTime timerEndDateTime, DateTime timerStartDateTime, Action actionToCallOnTimerOver)
        {
            StopSystemTimer();

            _timerStartDateTime = timerStartDateTime;
            _timerEndDateTime = timerEndDateTime;
            _actionToCallOnTimerOver = actionToCallOnTimerOver;

            //_systemTimerCoroutine = _targetSystemMonobehaviour.StartCoroutine(TimerCoroutine());
            StartTimerTick();
        }

        public void StopSystemTimer()
        {
            //if (_systemTimerCoroutine != null)
            //    _targetSystemMonobehaviour.StopCoroutine(_systemTimerCoroutine);
            //_systemTimerCoroutine = null;

            isRunning = false;
            TimeManager.Instance.DeRegisterRealtimeTimerTickEvent(TimerTick);
        }

        private void StartTimerTick()
        {
            isRunning = true;
            TimeManager.Instance.RegisterRealtimeTimerTickEvent(TimerTick);
        }

        public void RegisterTimerTickEvent(Action<string> timerTickEvent)
        {
            _timerTickStringEvents += timerTickEvent;
        }

        public void UnregisterTimerTickEvent(Action<string> timerTickEvent)
        {
            _timerTickStringEvents -= timerTickEvent;
        }

        public void RegisterTimerTickEvent(Action<double> timerTickEvent)
        {
            _timerTickDoubleEvents += timerTickEvent;
        }

        public void UnregisterTimerTickEvent(Action<double> timerTickEvent)
        {
            _timerTickDoubleEvents -= timerTickEvent;
        }

        public void RegisterTimerTickEvent(Action timerTickEvent)
        {
            _timerTickEvent += timerTickEvent;
        }

        public void UnregisterTimerTickEvent(Action timerTickEvent)
        {
            _timerTickEvent -= timerTickEvent;
        }

        public void RegisterTimerOverEvent(Action timerOverEvent)
        {
            _actionToCallOnTimerOver += timerOverEvent;
        }

        public void UnregisterTimerOverEvent(Action timerOverEvent)
        {
            _actionToCallOnTimerOver -= timerOverEvent;
        }

        public void ResetTimerObject()
        {
            StopSystemTimer();
            _actionToCallOnTimerOver = null;
            _timerEndDateTime = DateTime.MinValue;
            _timerStartDateTime = DateTime.MaxValue;
        }

        public void InvokeTimerOverActions()
        {
            _actionToCallOnTimerOver.Invoke();
        }

        public double GetRemainingTimeInSeconds()
        {
            if (_timerEndDateTime == null)
                return 0;

            TimeSpan remainingTime = _timerEndDateTime - TimeManager.Now;
            return remainingTime.TotalSeconds;
        }

        public TimeSpan GetRemainingTimeSpan()
        {
            if (_timerEndDateTime == null)
                return TimeSpan.Zero;

            TimeSpan remainingTime = _timerEndDateTime - TimeManager.Now;
            return remainingTime;
        }

        public string GetRemainingTimeInString(int formatValues = 2)
        {
            if (_timerEndDateTime == null)
                return "0m 0s";
            TimeSpan remainingTime = _timerEndDateTime - TimeManager.Now;
            return remainingTime.ParseTimeSpan(formatValues);
        }

        public float GetTimerProgress()
        {
            if (_timerStartDateTime < TimeManager.Now)
            {
                int totalTimerSeconds = Mathf.FloorToInt((float)(_timerEndDateTime - _timerStartDateTime).TotalSeconds);
                int currentSeconds = Mathf.CeilToInt((float)(TimeManager.Now - _timerStartDateTime).TotalSeconds);

                return Mathf.Clamp(Mathf.InverseLerp(1f, totalTimerSeconds, currentSeconds), 0f, 1f);
            }

            return -1f;
        }
        #endregion

        #region COROUTINES

        private void TimerTick(DateTime dateTime)
        {
            if (_timerEndDateTime > TimeManager.Now)
            {
                isRunning = true;
                TimeSpan remainingTime = (_timerEndDateTime - TimeManager.Now);
                _timerTickStringEvents?.Invoke(remainingTime.ParseTimeSpan(2));
                _timerTickDoubleEvents?.Invoke(remainingTime.TotalSeconds);
                _timerTickEvent?.Invoke();

                return;
            }

            StopSystemTimer();
            _actionToCallOnTimerOver?.Invoke();
        }

        //IEnumerator TimerCoroutine()
        //{
        //    WaitForSecondsRealtime oneRealSecondWait = new WaitForSecondsRealtime(1);
        //    TimeSpan remainingTime;

        //    while (_timerEndDateTime > CustomTime.GetCurrentTime())
        //    {
        //        remainingTime = (_timerEndDateTime - CustomTime.GetCurrentTime());
        //        _timerTickStringEvents?.Invoke(remainingTime.ParseTimeSpan(2));
        //        _timerTickDoubleEvents?.Invoke(remainingTime.TotalSeconds);
        //        _timerTickEvent?.Invoke();
        //        yield return oneRealSecondWait;
        //    }

        //    _systemTimerCoroutine = null;
        //    _actionToCallOnTimerOver?.Invoke();
        //}

        //IEnumerator EndLessTimerCoroutine()
        //{
        //    WaitForSecondsRealtime oneRealSecondWait = new(1f);

        //    while (true)
        //    {
        //        _timerTickEvent?.Invoke();
        //        yield return oneRealSecondWait;
        //    }
        //}

        #endregion
    }
}