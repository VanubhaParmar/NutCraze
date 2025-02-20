using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class RemoveAdsHandler : SerializedManager<RemoveAdsHandler>
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        [SerializeField] private CameraViewPortHandler[] cameraViewPortHandlers;
        private List<Action> onRemoveAdRefresh = new List<Action>();

        #endregion

        #region UNITY_CALLBACKS

        public override void Awake()
        {
            base.Awake();
            cameraViewPortHandlers = FindObjectsOfType<CameraViewPortHandler>();
            OnRemoveAdsPurchase();
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        [Button]
        public void OnRemoveAdsPurchase()
        {
            AdManager.Instance.HideBannerAd();
            for (int i = 0; i < cameraViewPortHandlers.Length; i++)
            {
                cameraViewPortHandlers[i].ApplySafeArea();
            }
            SafeAreaBGHandler.Instance.SetView();
            InvakeOnRemoveAdRefresh();
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        public void InvakeOnRemoveAdRefresh()
        {
            foreach (Action ev in onRemoveAdRefresh)
            {
                var action = ev;
                if (action != null)
                    action.Invoke();
            }
        }

        public void AddListenerOnRemoveAdRefresh(Action action)
        {
            if (!onRemoveAdRefresh.Contains(action))
                onRemoveAdRefresh.Add(action);
        }

        public void RemoveListenerOnRemoveAdRefresh(Action action)
        {
            if (onRemoveAdRefresh.Contains(action))
                onRemoveAdRefresh.Remove(action);
        }

        #endregion

        #region UI_CALLBACKS

        #endregion
    }
}
