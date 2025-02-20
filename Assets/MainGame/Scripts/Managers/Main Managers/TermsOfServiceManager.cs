using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort {
    public class TermsOfServiceManager : Manager<TermsOfServiceManager>
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        private const string IS_AGREE_TERMS = "isAgreeTerms";

        #endregion

        #region UNITY_CALLBACKS

        public override void Awake()
        {
            base.Awake();
            StartCoroutine(WaitForUIManager());
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        public void AgreeTermsOfService()
        {
            PlayerPrefbsHelper.SetInt(IS_AGREE_TERMS, 1);
            OnLoadingDone();
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        private void Init()
        {
            if (PlayerPrefbsHelper.HasKey(IS_AGREE_TERMS) && PlayerPrefbsHelper.GetInt(IS_AGREE_TERMS, 0) == 1)
            {
                OnLoadingDone();
                return;
            }
            if (CheckForContry())
            {
                //GlobalUIManager.Instance.ShowView<TermsOfServiceView>();
            }
            else
                OnLoadingDone();
        }

        private bool CheckForContry()
        {
            return false;
        }

        #endregion

        #region CO-ROUTINES

        IEnumerator WaitForUIManager()
        {
            while (GlobalUIManager.Instance == null)
                yield return null;
            Init();
        }

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
