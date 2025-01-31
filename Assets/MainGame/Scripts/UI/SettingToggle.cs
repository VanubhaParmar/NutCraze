using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.tag.nut_sort {
    public class SettingToggle : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Toggle mainToggle;
        [SerializeField] private Image toggleSetImage;
        [SerializeField] private RectTransform onPosition;
        [SerializeField] private RectTransform offPosition;

        private Func<bool> getToogleValueFunc;
        private Action<bool> onToggleValueChangedAction;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void InitView(Func<bool> getToogleValueFunc, Action<bool> onToggleValueChangedAction)
        {
            this.getToogleValueFunc = getToogleValueFunc;
            this.onToggleValueChangedAction = onToggleValueChangedAction;

            mainToggle.SetIsOnWithoutNotify(getToogleValueFunc());
            SetToggleView();
        }

        public void SetToggleView(bool isAnimate = false)
        {
            bool isOn = mainToggle.isOn;

            if (!isAnimate)
                toggleSetImage.transform.position = isOn ? onPosition.position : offPosition.position;
            else
            {
                Vector3 endPos = isOn ? onPosition.position : offPosition.position;

                Sequence tweenSeq = DOTween.Sequence();
                toggleSetImage.transform.DOMove(endPos, 0.2f).SetEase(Ease.OutQuad);
            }
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnChangeValue_Toggle()
        {
            SetToggleView(true);
            onToggleValueChangedAction?.Invoke(mainToggle.isOn);

            SoundHandler.Instance.PlaySound(SoundType.ButtonClick);
        }
        #endregion
    }
}