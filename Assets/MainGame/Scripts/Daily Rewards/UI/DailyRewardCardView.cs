using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityForge.PropertyDrawers;

namespace com.tag.nut_sort
{
    public class DailyRewardCardView : MonoBehaviour
    {
        #region PUBLIC_VARS
        public Image selectionImage;
        public Image headerBGImage;
        public List<Shadow> textThemeShades;
        public RectTransform GiftboxRectTranform => giftboxRectTransform;
        #endregion

        #region PRIVATE_VARS
        [SerializeField] private GameObject _rightTick;
        [SerializeField] private RectTransform giftboxRectTransform;
        [SerializeField] private CanvasGroup rewardsCanvasGroup;
        [SerializeField] private List<RewardImageTextView> rewardImageTextViews;
        [SerializeField] private Text _dayCountText;

        [Space]
        [SerializeField] private Animator giftboxScaleAnimator;
        [SerializeField, AnimatorStateName("giftboxScaleAnimator")] private string idleAnimationId;
        [SerializeField, AnimatorStateName("giftboxScaleAnimator")] private string scaleAnimationId;

        private RewardsDataSO _rewardData;
        private DailyRewardView DailyRewardView => MainSceneUIManager.Instance.GetView<DailyRewardView>();
        private Vector3 originalGiftboxTransformPos;
        #endregion

        #region UNITY_CALLBACKS
        private void Awake()
        {
            originalGiftboxTransformPos = giftboxRectTransform.anchoredPosition;
        }
        #endregion

        #region PUBLIC_FUNCTIONS
        public void SetView(RewardsDataSO rewardData, bool isClaimed, bool isCurrentDay)
        {
            ResetView();
            _rewardData = rewardData;

            var cardThemeType = isClaimed ? DailyRewardCardUIThemeType.REWARD_CLAIMED : (isCurrentDay ? DailyRewardCardUIThemeType.CURRENT_DAY_REWARD : DailyRewardCardUIThemeType.NOT_OPEN);

            SetDayRewardsView();
            SetDailyRewardCardThemeUI(cardThemeType);
            SetCard(isClaimed);

            if (giftboxRectTransform.gameObject.activeInHierarchy)
            {
                StopGiftboxIdleAnimation();
                if (isCurrentDay)
                    PlayGiftboxIdleAnimation();
            }
        }

        public void SetDayText(int dayCount)
        {
            _dayCountText.text = "Day " + dayCount.ToString();
        }

        public void PlayGiftboxIdleAnimation()
        {
            giftboxScaleAnimator.Play(scaleAnimationId);
        }

        public void StopGiftboxIdleAnimation()
        {
            giftboxScaleAnimator.Play(idleAnimationId);

        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private void SetDayRewardsView()
        {
            for (int i = 0; i < _rewardData.rewards.Count; i++)
            {
                var unusedView = rewardImageTextViews.Find(x => !x.gameObject.activeInHierarchy && x.targetRewardType == _rewardData.rewards[i].GetRewardType());
                if (unusedView != null)
                    unusedView.SetUI(_rewardData.rewards[i]);
            }

            rewardsCanvasGroup.GetComponent<RectTransform>().ForceUpdateRectTransforms();
        }

        private void SetDailyRewardCardThemeUI(DailyRewardCardUIThemeType dailyRewardCardUIThemeType)
        {
            DailyRewardCardUIThemeData themeData = DailyRewardView.DailyRewardCardUIThemeDatas.Find(x => x.dailyRewardCardUIThemeType == dailyRewardCardUIThemeType);

            if (themeData.cardBGSprite != null)
            {
                selectionImage.sprite = themeData.cardBGSprite;
                selectionImage.gameObject.SetActive(true);
            }
            headerBGImage.sprite = themeData.headerTopSprite;
            textThemeShades.ForEach(x => x.effectColor = themeData.textThemeColor);

            giftboxRectTransform.gameObject.SetActive(dailyRewardCardUIThemeType != DailyRewardCardUIThemeType.REWARD_CLAIMED);
            rewardsCanvasGroup.gameObject.SetActive(dailyRewardCardUIThemeType == DailyRewardCardUIThemeType.REWARD_CLAIMED);
        }

        private void ResetView()
        {
            rewardsCanvasGroup.gameObject.SetActive(false);
            rewardImageTextViews.ForEach(x => x.ResetView());
            selectionImage.gameObject.SetActive(false);
            _rightTick.SetActive(false);
        }

        private void SetCard(bool isClaimed)
        {
            _rightTick.SetActive(isClaimed);
        }
        #endregion

        #region CO-ROUTINES
        #endregion
    }
}
