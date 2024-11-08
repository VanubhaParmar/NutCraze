using System.Collections;
using System.Collections.Generic;
using Tag.NutSort;
using UnityEngine;

namespace Tag.NutSort
{
    public class VFXView : BaseView
    {
        public CurrencyAnimation CoinAnimation => coinAnimation;
        [SerializeField] private CurrencyAnimation coinAnimation;

        public void PlayCoinAnimation(Vector3 startPosition, int rewardAmount)
        {
            coinAnimation.StartAnimation(startPosition, rewardAmount);
        }

        public void PlayCoinAnimation(Vector3 startPosition, int rewardAmount, Transform endPosition)
        {
            coinAnimation.StartAnimation(startPosition, rewardAmount, endPosition);
        }
    }
}