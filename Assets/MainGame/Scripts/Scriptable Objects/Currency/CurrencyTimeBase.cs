using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "Time base Currency", menuName = Constant.GAME_NAME + "/Currency/Time base Currency")]
    [System.Serializable]
    public class CurrencyTimeBase : Currency
    {
        #region public veriables

        public int unitTimeUpdate;

        #endregion

        #region private veriables

        private Coroutine coroutine;
        private List<Action<bool, bool>> onTimerStartOrStop = new List<Action<bool, bool>>();
        private List<Action<TimeSpan>> onTimerTick = new List<Action<TimeSpan>>();
        private List<Action<int>> onCurrencyUpdateByTimer = new List<Action<int>>();
        protected int time;
        private bool canStartTimer;

        private DateTime infiniteCurrencyEndTime;
        private bool isInfiniteCurrencyActive = false;
        private const string INFINITE_CURRENCY_END_TIME_KEY = "_InfiniteCurrencyEndTime";
        private Coroutine infiniteCurrencyCO;
        private List<Action<bool>> onInfiniteTimerStartOrStop = new List<Action<bool>>();
        private List<Action<TimeSpan>> onInfiniteTimerTick = new List<Action<TimeSpan>>();
        #endregion

        #region propertices

        public virtual int UnitTimeUpdate
        {
            get { return unitTimeUpdate; }
        }

        private DateTime lastUpdateTime;

        public DateTime LastUpdateTime
        {
            get
            {
                string time = PlayerPrefbsHelper.GetString(key + "_Time", "");
                lastUpdateTime = (string.IsNullOrEmpty(time)) ? TimeManager.Now : SerializeUtility.DeserializeObject<DateTime>(time);
                return lastUpdateTime;
            }
            private set
            {
                lastUpdateTime = value;
                PlayerPrefbsHelper.SetString(key + "_Time", SerializeUtility.SerializeObject(lastUpdateTime));
            }
        }

        public DateTime LastUpdateTimeSaved
        {
            get
            {
                string time = PlayerPrefbsHelper.GetSavedString(key + "_Time", "");
                return (string.IsNullOrEmpty(time)) ? TimeManager.Now : SerializeUtility.DeserializeObject<DateTime>(time);
            }
        }

        public bool IsInfiniteCurrencyActive
        {
            get
            {
                UpdateInfiniteStatus();
                return isInfiniteCurrencyActive;
            }
            private set
            {
                isInfiniteCurrencyActive = value;
            }
        }

        public TimeSpan RemainingInfiniteTime
        {
            get
            {
                if (!IsInfiniteCurrencyActive)
                    return TimeSpan.Zero;
                return infiniteCurrencyEndTime - TimeManager.Now;
            }
        }
        #endregion

        #region virtual methods

        public override void Init()
        {
            base.Init();
            AddRemainEnergy();
            RegisterOnCurrencyChangeEvent(CheckForTimer);
            canStartTimer = true;
            TimeManager.Instance.RegisterOnTimerPause(OnGameResume);

            LoadInfiniteTimerStatus();
            if (IsInfiniteCurrencyActive)
                StartInfiniteTimerCoroutine();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            StopTimer();
            StopInfiniteLifeCoroutine();
            RemoveOnCurrencyChangeEvent(CheckForTimer);
            TimeManager.Instance.DeregisterOnTimerPause(OnGameResume);
        }

        public override void RemoveAllCallback()
        {
            base.RemoveAllCallback();
            onTimerStartOrStop.Clear();
            onTimerTick.Clear();
            onCurrencyUpdateByTimer.Clear();
            onInfiniteTimerStartOrStop.Clear();
            onInfiniteTimerTick.Clear();
        }
        #endregion

        #region private Methods

        private void OnGameResume(bool isPause, TimeSpan pauseTimeSpan)
        {
            if (Value >= defaultValue)
            {
                return;
            }
            if (!isPause)
            {
                if (pauseTimeSpan.TotalSeconds < time)
                {
                    StopTimer();
                    time = (time - (int)pauseTimeSpan.TotalSeconds);
                    StartTimer();
                    return;
                }

                int additionalValue = 1;
                long remainTime = (int)pauseTimeSpan.TotalSeconds;
                remainTime -= time;
                additionalValue += ((int)remainTime / UnitTimeUpdate);
                remainTime -= ((additionalValue - 1) * UnitTimeUpdate);
                StopTimer();
                if ((Value + additionalValue) >= defaultValue)
                {
                    int updateValues = defaultValue - Value;
                    Value += updateValues;
                    OnCurrencyUpdateByTimer(updateValues);
                    LastUpdateTime = TimeManager.Now;
                }
                else
                {
                    Value += additionalValue;
                    OnCurrencyUpdateByTimer(additionalValue);
                    time = UnitTimeUpdate - (int)remainTime;
                    LastUpdateTime = TimeManager.Now.AddSeconds(-time);
                    if (Value < defaultValue)
                        StartTimer();
                }
            }
        }

        private void AddRemainEnergy()
        {
            if (Value >= defaultValue)
                return;
            TimeSpan timeSpan = TimeManager.Now - LastUpdateTime;
            int energy = Mathf.FloorToInt((float)(timeSpan.TotalSeconds / UnitTimeUpdate));
            int remainTime = (int)(timeSpan.TotalSeconds - (energy * UnitTimeUpdate));
            if (energy > 0)
            {
                if ((Value + energy) < defaultValue)
                    Add(energy);
                else
                    SetValue(defaultValue);
                LastUpdateTime = TimeManager.Now.AddSeconds(-remainTime);
            }

            if (Value >= defaultValue)
                return;
            time = (remainTime > 0) ? (UnitTimeUpdate - remainTime) : UnitTimeUpdate;
            StartTimer();
        }

        private void CheckForTimer(int value)
        {
            if (coroutine != null || !canStartTimer)
            {
                if (defaultValue <= Value && coroutine != null)
                {
                    OnTimerStart(false, false);
                    StopTimer();
                    coroutine = null;
                }

                return;
            }

            LastUpdateTime = TimeManager.Now;
            if (defaultValue > Value)
            {
                time = UnitTimeUpdate;
                StartTimer();
                return;
            }
            OnTimerStart(false, false);
        }

        private void LoadInfiniteTimerStatus()
        {
            string savedTime = PlayerPrefbsHelper.GetString(key + INFINITE_CURRENCY_END_TIME_KEY, "");
            if (!string.IsNullOrEmpty(savedTime))
            {
                infiniteCurrencyEndTime = SerializeUtility.DeserializeObject<DateTime>(savedTime);
                UpdateInfiniteStatus();
            }
        }

        private void SaveInfiniteLifeStatus()
        {
            PlayerPrefbsHelper.SetString(key + INFINITE_CURRENCY_END_TIME_KEY, SerializeUtility.SerializeObject(infiniteCurrencyEndTime));
        }

        private void UpdateInfiniteStatus()
        {
            if (!isInfiniteCurrencyActive)
                return;

            if (TimeManager.Now >= infiniteCurrencyEndTime)
                EndInfiniteLife();
        }

        private void EndInfiniteLife()
        {
            IsInfiniteCurrencyActive = false;
            PlayerPrefbsHelper.DeleteKey(key + INFINITE_CURRENCY_END_TIME_KEY);
            ResumeTimer();
            StopInfiniteLifeCoroutine();
            InvokeInfiniteTimeStartOrStop(false);
        }

        private void StartInfiniteTimerCoroutine()
        {
            StopInfiniteLifeCoroutine();
            infiniteCurrencyCO = CoroutineRunner.Instance.CoroutineStart(InfiniteLifeTimer());
        }

        private void StopInfiniteLifeCoroutine()
        {
            if (infiniteCurrencyCO != null)
            {
                CoroutineRunner.Instance.CoroutineStop(infiniteCurrencyCO);
                infiniteCurrencyCO = null;
            }
        }

        private void InvokeInfiniteTimeStartOrStop(bool isStarted)
        {
            for (int i = 0; i < onInfiniteTimerStartOrStop.Count; i++)
                onInfiniteTimerStartOrStop[i]?.Invoke(isStarted);
        }

        private void InvokeInfiniteTimerTick(TimeSpan remainingTime)
        {
            for (int i = 0; i < onInfiniteTimerTick.Count; i++)
                onInfiniteTimerTick[i]?.Invoke(remainingTime);
        }

        private void OnTimerStart(bool value, bool isUpdated)
        {
            for (int i = 0; i < onTimerStartOrStop.Count; i++)
                onTimerStartOrStop[i]?.Invoke(value, isUpdated);
        }

        private void OnTimerTick(TimeSpan timeSpan)
        {
            for (int i = 0; i < onTimerTick.Count; i++)
                onTimerTick[i]?.Invoke(timeSpan);
        }

        private void OnCurrencyUpdateByTimer(int value)
        {
            for (int i = 0; i < onTimerTick.Count; i++)
                onCurrencyUpdateByTimer[i]?.Invoke(value);
        }

        public void StartTimer()
        {
            StopTimer();
            coroutine = CoroutineRunner.Instance.CoroutineStart(Timer());
        }
        private void StopTimer()
        {
            if (coroutine != null)
                CoroutineRunner.Instance.CoroutineStop(coroutine);
            coroutine = null;
        }

        #endregion

        #region public methods

        public override bool IsSufficentValue(int value)
        {
            return IsInfiniteCurrencyActive || Value >= value;

        }
        public void SetValue(int value, DateTime lastUpdateTime)
        {
            SetValue(value);
            LastUpdateTime = lastUpdateTime;
        }

        public void PauseTimer()
        {
            if (Value >= defaultValue)
                return;
            canStartTimer = false;
            StopTimer();
            coroutine = null;
            OnTimerStart(false, false);
        }

        public void ResumeTimer()
        {
            if (Value >= defaultValue)
                return;
            canStartTimer = true;
            StartTimer();
        }

        public bool IsFull()
        {
            return Value >= defaultValue;

        }
        public void RegisterOnInfiniteTimerStartOrStop(Action<bool> action)
        {
            if (!onInfiniteTimerStartOrStop.Contains(action))
                onInfiniteTimerStartOrStop.Add(action);
        }

        public void RemoveOnInfiniteTimerStartOrStop(Action<bool> action)
        {
            if (onInfiniteTimerStartOrStop.Contains(action))
                onInfiniteTimerStartOrStop.Remove(action);
        }

        public void RegisterOnInfiniteTimerTick(Action<TimeSpan> action)
        {
            if (!onInfiniteTimerTick.Contains(action))
                onInfiniteTimerTick.Add(action);
        }

        public void RemoveOnInfiniteTimerTick(Action<TimeSpan> action)
        {
            if (onInfiniteTimerTick.Contains(action))
                onInfiniteTimerTick.Remove(action);
        }

        public void AddInfiniteLife(TimeSpan duration)
        {
            UpdateInfiniteStatus();
            if (IsInfiniteCurrencyActive)
            {
                infiniteCurrencyEndTime = infiniteCurrencyEndTime.Add(duration);
            }
            else
            {
                infiniteCurrencyEndTime = infiniteCurrencyEndTime.Add(duration);
                IsInfiniteCurrencyActive = true;
                SetValue(defaultValue);
                PauseTimer();
            }
            StartInfiniteTimerCoroutine();
            SaveInfiniteLifeStatus();
        }

        public void AddInfiniteLifeMinutes(int minutes)
        {
            AddInfiniteLife(TimeSpan.FromMinutes(minutes));
        }

        public void RegisterTimerTick(Action<TimeSpan> action)
        {
            if (!onTimerTick.Contains(action))
                onTimerTick.Add(action);
        }

        public void RemoveTimerTick(Action<TimeSpan> action)
        {
            if (onTimerTick.Contains(action))
                onTimerTick.Remove(action);
        }

        public void RegisterTimerStartOrStop(Action<bool, bool> action)
        {
            if (!onTimerStartOrStop.Contains(action))
                onTimerStartOrStop.Add(action);
        }

        public void RemoveTimerStartOrStop(Action<bool, bool> action)
        {
            if (onTimerStartOrStop.Contains(action))
                onTimerStartOrStop.Remove(action);
        }

        public void RegisterOnCurrencyUpdateByTimer(Action<int> action)
        {
            if (!onCurrencyUpdateByTimer.Contains(action))
                onCurrencyUpdateByTimer.Add(action);
        }

        public void RemoveOnCurrencyUpdateByTimer(Action<int> action)
        {
            if (onCurrencyUpdateByTimer.Contains(action))
                onCurrencyUpdateByTimer.Remove(action);
        }

        #endregion

        #region Coroutine

        IEnumerator Timer()
        {
            OnTimerStart(true, false);
            TimeSpan i = new TimeSpan(0, 0, time);
            TimeSpan second = new TimeSpan(0, 0, 1);
            WaitForSeconds one = new WaitForSeconds(1);
            while (i.TotalSeconds > 0)
            {
                i = i.Subtract(second);
                OnTimerTick(i);
                time--;
                yield return one;
            }
            coroutine = null;
            Value++;
            OnCurrencyUpdateByTimer(1);
        }

        private IEnumerator InfiniteLifeTimer()
        {
            InvokeInfiniteTimeStartOrStop(true);
            TimeSpan remainingTime = infiniteCurrencyEndTime - TimeManager.Now;
            TimeSpan second = new TimeSpan(0, 0, 1);
            WaitForSeconds one = new WaitForSeconds(1);

            while (remainingTime.TotalSeconds > 0)
            {
                remainingTime = remainingTime.Subtract(second);
                InvokeInfiniteTimerTick(remainingTime);
                yield return one;
            }
            infiniteCurrencyCO = null;
            EndInfiniteLife();
        }
        #endregion
    }
}