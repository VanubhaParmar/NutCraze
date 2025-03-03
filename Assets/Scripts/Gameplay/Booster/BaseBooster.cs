using System;
using UnityEngine;

namespace Tag.NutSort
{
    [Serializable]
    public abstract class BaseBooster
    {
        #region PRIVATE_VARIABLES

        [SerializeField, BoosterId] protected int boosterId;
        [SerializeField] protected string boosterName;
        [SerializeField] protected string cannotUseMessage;
        [SerializeField] protected string rewardAdPlace;
        [SerializeField] protected RewardAdShowCallType rewardAdType;
        [SerializeField] protected int boostersToAddOnAdWatch = 1;

        #endregion

        #region PROPERTIES
        public int BoosterId => boosterId;
        public string BoosterName => boosterName;
        public string CannotUseMessage => cannotUseMessage;
        public string RewardAdPlace => rewardAdPlace;
        public RewardAdShowCallType RewardAdType => rewardAdType;
        public int BoostersToAddOnAdWatch => boostersToAddOnAdWatch;
        #endregion

        #region ABSTRACT_METHODS
        
        public abstract bool CanUse();
        public abstract bool HasBooster();
        public abstract int GetBoosterCount();
        public abstract void Use();
        public abstract void OnAdWatchSuccess();
        public abstract void FireBoosterAdWatchEvent();

        #endregion
    }
}
