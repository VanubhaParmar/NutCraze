using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;

namespace com.tag.nut_sort {
    [RequireComponent(typeof(Button))]
    public class ButtonDependentTheme : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public bool spriteSwap;
        [BoxGroup("spriteSwap"), ShowIf("spriteSwap")] public Image targetImage;
        [BoxGroup("spriteSwap"), ShowIf("spriteSwap")] public List<Sprite> spritesToChange;

        public bool textColorChange;
        [BoxGroup("textColorChange"), ShowIf("textColorChange")] public TextMeshProUGUI targetText;
        [BoxGroup("textColorChange"), ShowIf("textColorChange")] public List<Color> colorsToChange;
        #endregion

        #region PRIVATE_VARIABLES
        private Button myButton;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void Awake()
        {
            AssignButton();
        }

        private void OnEnable()
        {
            UIUtilityEvents.onRefreshUIRects += UIUtilityEvents_onRefreshUIRects;

            UIUtilityEvents_onRefreshUIRects();
        }

        private void OnDisable()
        {
            UIUtilityEvents.onRefreshUIRects -= UIUtilityEvents_onRefreshUIRects;
        }
        #endregion

        #region PUBLIC_METHODS
        [Button]
        public void AssignButton()
        {
            if (myButton == null)
                myButton = gameObject.GetComponent<Button>();
        }

        [Button]
        public void ChangeTheme()
        {
            if (spriteSwap)
                targetImage.sprite = myButton.interactable ? spritesToChange[0] : spritesToChange[1];

            if (textColorChange)
                targetText.color = myButton.interactable ? colorsToChange[0] : colorsToChange[1];
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        private void UIUtilityEvents_onRefreshUIRects()
        {
            ChangeTheme();
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}