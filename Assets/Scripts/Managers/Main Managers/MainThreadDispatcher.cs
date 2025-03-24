using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

namespace Tag.NutSort
{
    public class MainThreadDispatcher : SerializedMonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        private static readonly Queue<Action> Actions = new Queue<Action>();
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
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
        public static void ExecuteOnMainThread(Action action)
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