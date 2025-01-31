using System;
using UnityEngine;

namespace com.tag.nut_sort {
    public class SoundClipPlayer : MonoBehaviour
    {
        #region PUBLIC_VARS

        public SoundType soundType;
        public bool playOnEnable;

        //[Range(0, 100f)]
        //public float volume = 100f;

        #endregion

        #region PRIVATE_VARS
        #endregion

        #region UNITY_CALLBACKS
        private void OnEnable()
        {
            if (playOnEnable)
                PlaySound();
        }
        #endregion

        #region PUBLIC_FUNCTIONS

        public void PlaySound()
        {
            SoundHandler.Instance.PlaySound(soundType, Response);

            void Response(bool status)
            {
                if (!status)
                {
                    throw new Exception("No Sound Type Found Please Create SO and Event For " + soundType + ", GameObject Name : " + gameObject.name);
                }
            }
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
