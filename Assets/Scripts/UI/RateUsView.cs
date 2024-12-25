using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class RateUsView : BaseView
    {
        #region PUBLIC_VARIABLES
        public static bool IsRated
        {
            get => RatedState;
            set => RatedState = value;
        }

        private static bool RatedState { get { return PlayerPrefs.GetInt(RatedStateKey, 0) == 1; } set { PlayerPrefs.SetInt(RatedStateKey, value ? 1 : 0); } }
        private const string RatedStateKey = "GameRateUsPlayerPref";

        private Action actionToCallOnHide;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);
            MainSceneUIManager.Instance.GetView<BannerAdsView>().Hide();
        }

        public void ShowWithHideAction(Action actionToCallOnHide)
        {
            this.actionToCallOnHide = actionToCallOnHide;
            Show();
        }

        public override void Hide()
        {
            MainSceneUIManager.Instance.GetView<BannerAdsView>().Show(true);
            base.Hide();
        }

        public override void OnHideComplete()
        {
            base.OnHideComplete();

            actionToCallOnHide?.Invoke();
            actionToCallOnHide = null;
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_RateUs()
        {
            Application.OpenURL(GameManager.Instance.GameMainDataSO.playStoreLink);
            RatedState = true;

            GlobalUIManager.Instance.GetView<UserPromptView>().Show(UserPromptMessageConstants.RateUsDoneMessage);
            Hide();
        }
        #endregion
    }
}