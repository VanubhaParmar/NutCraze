using System;
using UnityEngine;

namespace com.tag.nut_sort {
    public class VFXView : BaseView
    {
        public CurrencyAnimation CoinAnimation => coinAnimation;

        [SerializeField] private CurrencyAnimation coinAnimation;
        [SerializeField] private BoosterAnimationView boosterAnimationView;

        public void PlayCoinAnimation(Vector3 startPosition, int rewardAmount)
        {
            coinAnimation.StartAnimation(startPosition, rewardAmount);
        }

        public void PlayCoinAnimation(Vector3 startPosition, int rewardAmount, Transform endPosition)
        {
            coinAnimation.StartAnimation(startPosition, rewardAmount, endPosition);
        }

        public void PlayBoosterClaimAnimation(BoosterType boosterType, int boosterCount, Vector3 startPosition, Action actionToCallOnOver = null)
        {
            boosterAnimationView.PlayBoosterClaimAnimation(boosterType, boosterCount, startPosition, actionToCallOnOver);
        }
    }
}