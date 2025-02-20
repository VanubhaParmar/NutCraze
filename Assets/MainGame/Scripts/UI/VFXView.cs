using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class VFXView : BaseView
    {
        [SerializeField] private CurrencyAnimation coinAnimation;
        [SerializeField] private BoosterAnimationView boosterAnimationView;

        public CurrencyAnimation CoinAnimation => coinAnimation;

        public void PlayBoosterClaimAnimation(int boosterType, int boosterCount, Vector3 startPosition, Action actionToCallOnOver = null)
        {
            boosterAnimationView.PlayBoosterClaimAnimation(boosterType, boosterCount, startPosition, actionToCallOnOver);
        }
    }
}