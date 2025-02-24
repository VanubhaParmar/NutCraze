using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class DailyRewardView : BaseView
    {
        #region PUBLIC_VARS
        public List<DailyRewardCardUIThemeData> DailyRewardCardUIThemeDatas => dailyRewardCardUIThemeDatas;
        #endregion

        #region PRIVATE_VARS
        [SerializeField] private Button _claimButton;
        [SerializeField] private List<DailyRewardCardView> _dailyRewardCardViews;

        [Space]
        [SerializeField] private List<DailyRewardCardUIThemeData> dailyRewardCardUIThemeDatas;

        private int _currentDay;

        private Action actionToCallOnClaimOver;
        #endregion

        #region PUBLIC_FUNCTIONS
        public override void Show(Action action = null, bool isForceShow = false)
        {
            GameStatsCollector.Instance.OnPopUpTriggered(GameStatPopUpTriggerType.SYSTEM_TRIGGERED);
            base.Show(action, isForceShow);

            bool canClaimReward = DailyRewardManager.Instance.CanClaimTodayReward();
            int currentDay = DailyRewardManager.Instance.GetCurrentDay();

            SetView(DailyRewardManager.Instance.DailyRewardDataSO, currentDay, canClaimReward);
        }

        public void ShowAndCollectRewards(Action actionToCallOnClaimOver)
        {
            this.actionToCallOnClaimOver = actionToCallOnClaimOver;
            Show();

            MainSceneUIManager.Instance.GetView<BannerAdsView>().Hide();
        }

        public override void Hide()
        {
            base.Hide();
            MainSceneUIManager.Instance.GetView<BannerAdsView>().Show(true);

            actionToCallOnClaimOver?.Invoke();
            actionToCallOnClaimOver = null;
        }

        public void SetView(DailyRewardDataSO dailyRewardDataSet, int currentDay, bool canClaimReward)
        {
            _currentDay = currentDay;
            for (int i = 0; i < dailyRewardDataSet.rewardDataSets.Count; i++)
            {
                _dailyRewardCardViews[i].SetView(dailyRewardDataSet.rewardDataSets[i], i < _currentDay, i == _currentDay);
                _dailyRewardCardViews[i].SetDayText(i + 1);
            }
            SetClaimButton(canClaimReward);
        }

        public void SetClaimButton(bool state)
        {
            _claimButton.interactable = state;
        }

        public void ClaimRewards()
        {
            EventSystemHelper.Instance.BlockInputs(true);
            DailyRewardManager.Instance.OnClaimTodayReward();

            Vector3 giftBoxPosition = _dailyRewardCardViews[_currentDay].GiftboxRectTranform.transform.position;
            GiftboxId giftBoxId = _currentDay == DailyRewardManager.Instance.DailyRewardDataSO.rewardDataSets.Count - 1 ? GiftboxId.RED : GiftboxId.SKY_BLUE;
            
            MainSceneUIManager.Instance.GetView<GiftboxRewardAnimationView>().PlayRewardAnimation(giftBoxPosition, 1f, (int)giftBoxId, DailyRewardManager.Instance.GetDayReward(_currentDay), OnClaimAnimationComplete,
                () => {
                    _dailyRewardCardViews[_currentDay].GiftboxRectTranform.gameObject.SetActive(false);
                });
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private void OnClaimAnimationComplete()
        {
            EventSystemHelper.Instance.BlockInputs(false);
            Hide();
        }
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_Claim()
        {
            ClaimRewards();
        }
        #endregion
    }

    [Serializable]
    public class DailyRewardCardUIThemeData
    {
        public DailyRewardCardUIThemeType dailyRewardCardUIThemeType;
        public Sprite cardBGSprite;
        public Sprite headerTopSprite;
        public Color textThemeColor;
    }

    public enum DailyRewardCardUIThemeType
    {
        NOT_OPEN,
        CURRENT_DAY_REWARD,
        REWARD_CLAIMED
    }
}
