using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.tag.nut_sort {
    public class RewardImageTextView : SerializedMonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public RewardType targetRewardType;
        public CanvasGroup CanvasGroup => canvasGroup;
        public Image RewardImage => rewardImage;
        public RectTransform AnimationParent => animationParent;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform animationParent;
        [SerializeField] private Image rewardImage;
        [SerializeField] private Text rewardText;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void SetUI(BaseReward baseReward)
        {
            rewardImage.sprite = baseReward.GetRewardImageSprite();
            rewardText.text = "+" + baseReward.GetAmount();
            gameObject.SetActive(true);
        }

        public void ResetView()
        {
            gameObject.SetActive(false);
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}