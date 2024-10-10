using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort.LevelEditor
{
    public class LevelEditorLoadingView : BaseView
    {
        #region PUBLIC_VARIABLES
        public Text loadingText;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);
            StartCoroutine(LoadingTextAnim());
        }
        #endregion

        #region PUBLIC_METHODS
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        IEnumerator LoadingTextAnim()
        {
            while (true)
            {
                loadingText.text = "Loading";
                int dotsAnim = 3;

                while (dotsAnim > 0)
                {
                    yield return new WaitForSeconds(1f);
                    loadingText.text += ".";

                    dotsAnim--;
                }

                yield return new WaitForSeconds(1f);
            }
        }
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}