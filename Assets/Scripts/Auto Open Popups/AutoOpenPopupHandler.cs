using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class AutoOpenPopupHandler : SerializedManager<AutoOpenPopupHandler>
    {
        #region PUBLIC_VARIABLES
        public List<BaseAutoOpenPopupChecker> autoOpenPopupCheckers;
        #endregion

        #region PRIVATE_VARIABLES
        private int currentCheckingIndex;
        private Action actionToCallOnCheckOver;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            OnLoadingDone();
        }
        #endregion

        #region PUBLIC_METHODS
        public void OnCheckForAutoOpenPopUps(Action actionToCallOnCheckOver = null)
        {
            currentCheckingIndex = -1;
            this.actionToCallOnCheckOver = actionToCallOnCheckOver;
            CheckForNextAutoOpenPopup();
        }

        public T GetAutoOpenChecker<T>() where T : BaseAutoOpenPopupChecker
        {
            return autoOpenPopupCheckers.Find(x => x is T) as T;
        }
        #endregion

        #region PRIVATE_METHODS
        private void CheckForNextAutoOpenPopup()
        {
            currentCheckingIndex++;

            if (currentCheckingIndex < autoOpenPopupCheckers.Count && !Tutorial.IsRunning)
            {
                autoOpenPopupCheckers[currentCheckingIndex].InitializeChecker();
                autoOpenPopupCheckers[currentCheckingIndex].CheckForAutoOpenPopup(CheckForNextAutoOpenPopup);
            }
            else
                actionToCallOnCheckOver?.Invoke();
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }

    public abstract class BaseAutoOpenPopupChecker : MonoBehaviour
    {
        protected Action actionToCallOnAutoOpenComplete;

        public abstract void InitializeChecker();
        public virtual void CheckForAutoOpenPopup(Action actionToCallOnAutoOpenComplete)
        {
            this.actionToCallOnAutoOpenComplete = actionToCallOnAutoOpenComplete;
        }

        protected void OnAutoOpenCheckComplete()
        {
            actionToCallOnAutoOpenComplete?.Invoke();
        }

        protected virtual void OnAutoOpenCheckAbort()
        {
        }
    }
}