using System;
using UnityEngine;

namespace Tag.NutSort
{
    public class Timer 
    {
        private float _elapsedTime = 0f;
        private bool _isRunning = false;
        private bool _isPaused = false;

        private Action<DateTime> _tickCallback;
        
        public float ElapsedTime => _elapsedTime;
        public int ElapsedTimeSeconds => Mathf.FloorToInt(_elapsedTime);
        public bool IsRunning => _isRunning;
        public bool IsPaused => _isPaused;

        public Timer()
        {
            _tickCallback = HandleTimeManagerTick;
        }

        public void StartTimer(int initialTimeSeconds = 0)
        {
            CleanupAndDeregister();
            _elapsedTime = initialTimeSeconds;
            _isRunning = true;
            _isPaused = false;
            TimeManager.Instance.RegisterTimerTickEvent(_tickCallback);
        }

        public void StopTimer()
        {
            CleanupAndDeregister();
            _isRunning = false;
            _isPaused = false;
            _elapsedTime = 0f;
        }

        private void CleanupAndDeregister()
        {
            TimeManager.Instance.DeRegisterTimerTickEvent(_tickCallback);
        }

        public void PauseTimer()
        {
            if (_isRunning && !_isPaused)
                _isPaused = true;
        }

        public void ResumeTimer()
        {
            if (_isRunning && _isPaused)
                _isPaused = false;
        }

        public void ResetTimer()
        {
            StopTimer();
        }

        private void HandleTimeManagerTick(DateTime currentTime)
        {
            if (_isRunning && !_isPaused)
                _elapsedTime += 1.0f;
        }

    }
}