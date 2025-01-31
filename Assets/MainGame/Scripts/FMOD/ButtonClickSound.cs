using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace com.tag.nut_sort {
    [RequireComponent(typeof(Button))]
    public class ButtonClickSound : MonoBehaviour
    {
        #region PUBLIC_VARS
        public bool overrideClip;
        [ShowIf("overrideClip")] public SoundType soundType;
        #endregion

        #region PRIVATE_VARS

        #endregion

        #region UNITY_CALLBACKS

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(PlayButtonClickSound);
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        #endregion

        #region PRIVATE_FUNCTIONS

        private void PlayButtonClickSound()
        {
            SoundHandler.Instance.PlaySound(overrideClip ? soundType : SoundType.ButtonClick);
        }
        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS

        #endregion
    }
}