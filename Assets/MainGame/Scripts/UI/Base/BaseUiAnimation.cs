using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tag.nut_sort {

    public class BaseUiAnimation : MonoBehaviour
    {
        #region protected veriables

        protected Action onHideComplete;
        protected Action onShowComplete;

        #endregion

        #region virtual methods

        public virtual void ShowAnimation(Action onShow)
        {
            this.onShowComplete = onShow;
        }

        public virtual void HideAnimation(Action action)
        {
            this.onHideComplete = action;
        }
        public virtual void AddToObjectAfterAnimate(Transform transform)
        {

        }

        public virtual void ClearObjectAfterAnimatList()
        {

        }

        public virtual void AddNewObjectToObjectAfterAnimate(Transform transform)
        {

        }
        #endregion
    }
}

