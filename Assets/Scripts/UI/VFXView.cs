using System.Collections;
using System.Collections.Generic;
using Tag.NutSort;
using Tag.TowerDefence;
using UnityEngine;

public class VFXView : BaseView
{
    public CurrencyAnimation CoinAnimation => coinAnimation;
    [SerializeField] private CurrencyAnimation coinAnimation;

    public void PlayCoinAnimation(Vector3 startPosition, int rewardAmount)
    {
        coinAnimation.StartAnimation(startPosition, rewardAmount);
    }
}