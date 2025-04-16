using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        private static MainThreadDispatcher instance;
        private static readonly Queue<Action> Actions = new Queue<Action>();
        #endregion

        #region PROPERTIES
        public static MainThreadDispatcher Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject("MainThreadDispatcher");
                    instance = obj.AddComponent<MainThreadDispatcher>();
                    if (Application.isPlaying)
                        DontDestroyOnLoad(obj);
                }
                return instance;
            }
        }
        #endregion

        #region UNITY_CALLBACKS
        private void Awake()
        {
            if (instance == null)
                instance = this;
        }

        private void OnDestroy()
        {
            if (instance != null)
                instance = null;
        }

        private void Update()
        {
            lock (Actions)
            {
                while (Actions.Count > 0)
                {
                    Actions.Dequeue()?.Invoke();
                }
            }
        }
        #endregion

        #region PUBLIC_METHODS
        public void ExecuteOnMainThread(Action action)
        {
            if (action == null) return;

            lock (Actions)
            {
                Actions.Enqueue(action);
            }
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}