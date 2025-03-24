using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class BoosterManager : SerializedManager<BoosterManager>
    {
        #region PRIVATE_VARS
        [OdinSerialize]
        [SerializeField, DictionaryDrawerSettings(KeyLabel = "Booster ID", ValueLabel = "Booster")]
        private Dictionary<int, BaseBooster> boosterMapping = new Dictionary<int, BaseBooster>();
        private static List<Action<int>> onBoosterUse = new List<Action<int>>();
        #endregion

        #region PUBLIC_VARS
        #endregion

        #region PROPERTIES
        private ShopView ShopView => MainSceneUIManager.Instance.GetView<ShopView>();
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        public void OnExtraScrewButtonClick()
        {
            OnBoosterButtonClick(BoosterIdConstant.EXTRA_SCREW);
        }

        public void OnUndoButtonClick()
        {
            OnBoosterButtonClick(BoosterIdConstant.UNDO);
        }

        public static void RegisterOnBoosterUse(Action<int> action)
        {
            if (!onBoosterUse.Contains(action))
                onBoosterUse.Add(action);
        }

        public static void DeRegisterOnBoosterUse(Action<int> action)
        {
            if (onBoosterUse.Contains(action))
                onBoosterUse.Remove(action);
        }

        public void OnBoosterButtonClick(int boosterId)
        {
            BaseBooster booster = GetBooster(boosterId);
            if (booster.CanUse())
            {
                booster.Use();
            }
            else if (!booster.HasBooster())
            {
                if (AdManager.Instance.CanShowRewardedAd())
                    AdManager.Instance.ShowRewardedAd(booster.OnAdWatchSuccess, booster.RewardAdType, booster.RewardAdPlace);
                else
                    ShopView.Show();
            }
            else
            {
                ToastMessageView.Instance.ShowMessage(booster.CannotUseMessage);
            }
        }

        public BaseBooster GetBooster(int boosterId)
        {
            if (boosterMapping.TryGetValue(boosterId, out BaseBooster booster))
            {
                return booster;
            }
            Debug.LogError($"Booster with ID {boosterId} not found!");
            return null;
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        public void InvokeOnBoosterUse(int boosterType)
        {
            for (int i = 0; i < onBoosterUse.Count; i++)
                onBoosterUse[i]?.Invoke(boosterType);
        }
        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region EDITOR
#if UNITY_EDITOR
        [Button]
        public void AddBooster([BoosterId] int boosterId, BaseBooster booster)
        {
            if (!boosterMapping.ContainsKey(boosterId))
                boosterMapping.Add(boosterId, booster);
            else
                boosterMapping[boosterId] = booster;
        }
#endif
        #endregion
    }
}
