using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class ToastMessageView : BaseView
    {
        #region PUBLIC_VARS
        public static ToastMessageView Instance { get; private set; }
        [SerializeField] private ToastMessage toastMessage;
        #endregion

        #region PRIVATE_VARS
        private List<ToastMessage> toastPools = new List<ToastMessage>();
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            Instance = this;
        }
        #endregion

        #region PUBLIC_FUNCTIONS

        [Button]
        public void ShowMessage(string message, bool canLocalize = true)
        {
            var toast = toastPools.Find(x => !x.gameObject.activeInHierarchy);
            if (toast == null)
                toast = CreateToast();

            toast.ShowToastMessage(message, Vector3.zero, canLocalize: canLocalize);
        }

        public void ShowMessage(string message, Vector3 startPosition)
        {
            var toast = toastPools.Find(x => !x.gameObject.activeInHierarchy);
            if (toast == null)
                toast = CreateToast();
            toast.ShowToastMessage(message, startPosition);
        }

        #endregion

        #region PRIVATE_FUNCTIONS
        private ToastMessage CreateToast()
        {
            ToastMessage tm = Instantiate(toastMessage, transform);
            toastPools.Add(tm);

            return tm;
        }
        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}
