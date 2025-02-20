using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityForge.PropertyDrawers;

namespace Tag.NutSort
{
    public class GiftboxRewardAnimationView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private CanvasGroup mainCG;
        [SerializeField] private Image boxBottomImage;
        [SerializeField] private Image boxTopImage;

        [SerializeField] private RectTransform giftboxPosParent;

        [Space, Header("Giftbox Animation Settings")]
        [SerializeField] private float giftBoxTranslateAnimationTime;
        [SerializeField] private float giftBoxAnimationJumpPower;

        [Space, Header("Coin Top Bar Settings")]
        [SerializeField] private RectTransform coinTopBarMainParent;
        [SerializeField] private CurrencyTopbarComponent currencyTopbarComponent;
        [SerializeField] private float coinTopBarTranslateAnimationTime = 0.3f;

        [Space]
        [SerializeField] private Animator viewMainAnimator;
        [SerializeField, AnimatorStateName("viewMainAnimator")] private string idleAnimation;
        [SerializeField, AnimatorStateName("viewMainAnimator")] private string giftboxOpenAnimation;

        [Space]
        [SerializeField] private List<RewardImageTextView> rewardPrefabs;
        [SerializeField] private RectTransform rewardViewsParent;
        [ShowInInspector, ReadOnly] private List<RewardImageTextView> generatedRewardViews = new List<RewardImageTextView>();

        private Action actionToCallOnAnimationOver;
        private RewardsDataSO giftBoxReward;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void PlayRewardAnimation(Vector3 giftboxPosition, float giftboxScale, int giftboxId, RewardsDataSO giftboxReward, Action actionToCallOnAnimationOver = null, Action actionToCallOnGiftboxSpwan = null)
        {
            this.actionToCallOnAnimationOver = actionToCallOnAnimationOver;
            this.giftBoxReward = giftboxReward;

            base.Show();

            var giftboxSprites = ResourceManager.Instance.GetGiftBoxSprites(giftboxId);
            boxBottomImage.sprite = giftboxSprites.giftboxBotSprite;
            boxTopImage.sprite = giftboxSprites.giftboxTopSprite;

            SetRewardViews();

            coinTopBarMainParent.gameObject.SetActive(false);
            viewMainAnimator.Play(idleAnimation);

            ShowPanelAnimation();
            PlayGiftboxTranslateAnimation(giftboxPosition, giftboxScale, actionToCallOnGiftboxSpwan);
        }

        public void HidePanel()
        {
            HidePanelAnimation(() =>
                {
                    actionToCallOnAnimationOver?.Invoke();
                    Hide();
                });
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetRewardViews()
        {
            generatedRewardViews.ForEach(x => x.ResetView());

            for (int i = 0; i < giftBoxReward.rewards.Count; i++)
            {
                var rewardView = GetInActiveRewardView(giftBoxReward.rewards[i].GetRewardType()) ?? GenerateNewRewardImageTextView(giftBoxReward.rewards[i].GetRewardType());
                rewardView.SetUI(giftBoxReward.rewards[i]);
                rewardView.gameObject.SetActive(true);
                rewardView.CanvasGroup.alpha = 0f;
            }

            rewardViewsParent.ForceUpdateRectTransforms();
        }

        private RewardImageTextView GetInActiveRewardView(RewardType rewardTypeToTarget)
        {
            return generatedRewardViews.Find(x => x.targetRewardType == rewardTypeToTarget && !x.gameObject.activeInHierarchy);
        }

        private RewardImageTextView GenerateNewRewardImageTextView(RewardType rewardTypeToTarget)
        {
            var view = Instantiate(rewardPrefabs.Find(x => x.targetRewardType == rewardTypeToTarget), rewardViewsParent);
            view.gameObject.SetActive(false);
            generatedRewardViews.Add(view);

            return view;
        }

        private void ShowPanelAnimation()
        {
            mainCG.alpha = 0f;
            mainCG.DOFade(1f, 0.4f);
        }

        private void HidePanelAnimation(Action actionToCall)
        {
            mainCG.alpha = 1f;
            mainCG.DOFade(0f, 0.4f).onComplete += () => { actionToCall?.Invoke(); };
        }

        private void PlayGiftboxTranslateAnimation(Vector3 giftboxPosition, float giftboxScale, Action actionToCallOnGiftboxSpwan = null)
        {
            giftboxPosParent.transform.position = giftboxPosition;
            giftboxPosParent.transform.localScale = Vector3.one * giftboxScale;

            Sequence translateAnimation = DOTween.Sequence();
            translateAnimation.AppendInterval(0.5f);
            translateAnimation.AppendCallback(() => { actionToCallOnGiftboxSpwan?.Invoke(); });
            translateAnimation.Append(giftboxPosParent.DOJumpAnchorPos(Vector2.zero, giftBoxAnimationJumpPower, 1, giftBoxTranslateAnimationTime));
            translateAnimation.Join(giftboxPosParent.DOScale(1.5f, giftBoxTranslateAnimationTime - 0.1f));
            translateAnimation.InsertCallback(0.5f + giftBoxTranslateAnimationTime - 0.1f, () =>
            {
                viewMainAnimator.Play(giftboxOpenAnimation);
            });
            translateAnimation.InsertCallback(0.5f + giftBoxTranslateAnimationTime + 0.3f, () =>
            {
                SoundHandler.Instance.PlaySound(SoundType.GiftboxOpen);
            });
            translateAnimation.InsertCallback(0.5f + giftBoxTranslateAnimationTime + 0.7f, () =>
            {
                PlayRewardViewsUpAnimation();
            });
            translateAnimation.AppendInterval(viewMainAnimator.GetAnimatorClipLength(giftboxOpenAnimation) - 0.5f);
            translateAnimation.onComplete += PlayRewardCollectAnimation;
        }

        private void PlayRewardViewsUpAnimation()
        {
            Sequence rewardsUpAnimation = DOTween.Sequence();
            var openViews = generatedRewardViews.FindAll(x => x.gameObject.activeInHierarchy);

            float totalTime = 0f;
            float animTranslateTime = 0.5f;

            for (int i = 0; i < openViews.Count; i++)
            {
                var currentAnimView = openViews[i];
                currentAnimView.AnimationParent.anchoredPosition = Vector2.down * 50f;

                Sequence viewSequence = DOTween.Sequence();
                viewSequence.Append(currentAnimView.CanvasGroup.DOFade(1f, animTranslateTime));
                viewSequence.Join(currentAnimView.AnimationParent.DOAnchorPos(Vector2.zero, animTranslateTime));

                rewardsUpAnimation.Insert(totalTime, viewSequence);
                totalTime += 0.1f;
            }
        }

        private bool HasCoinReward()
        {
            var currencyRewards = giftBoxReward.rewards.FindAll(x => x.GetRewardType() == RewardType.Currency);
            return currencyRewards.Find(x => x.GetRewardId() == CurrencyConstant.COIN) != null;
        }

        private void PlayRewardCollectAnimation()
        {
            if (!HasCoinReward())
            {
                Sequence waitAndHide = DOTween.Sequence();
                waitAndHide.AppendInterval(0.25f);
                waitAndHide.onComplete += OnGiftboxOpenAniamtionComplete;
                return;
            }

            BaseReward baseReward = giftBoxReward.rewards.Find(x => x.GetRewardId() == CurrencyConstant.COIN);
            Vector3 coinPos = generatedRewardViews.Find(x => x.targetRewardType == RewardType.Currency).RewardImage.transform.position;

            coinTopBarMainParent.gameObject.SetActive(true);

            currencyTopbarComponent.SetCurrencyValue(DataManager.Instance.GetCurrency(CurrencyConstant.COIN).Value - baseReward.GetAmount());

            Vector3 originalPos = new Vector2(coinTopBarMainParent.anchoredPosition.x, 100f);
            Vector3 targetPos = new Vector2(coinTopBarMainParent.anchoredPosition.x, -65f);

            coinTopBarMainParent.anchoredPosition = originalPos;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(coinTopBarMainParent.DOAnchorPos(targetPos, coinTopBarTranslateAnimationTime).SetEase(Ease.OutQuad));
            sequence.AppendInterval(1.5f);
            sequence.Append(coinTopBarMainParent.DOAnchorPos(originalPos, coinTopBarTranslateAnimationTime).SetEase(Ease.OutQuad));
            sequence.onComplete += OnGiftboxOpenAniamtionComplete;

            baseReward.ShowRewardAnimation(currencyTopbarComponent.CurrencyAnimation, coinPos);
        }

        private void OnGiftboxOpenAniamtionComplete()
        {
            coinTopBarMainParent.gameObject.SetActive(false);
            HidePanel();
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}