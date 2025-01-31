using System;
using System.Collections;
using UnityEngine;
using UnityForge.PropertyDrawers;

namespace com.tag.nut_sort {

    public class PopUpAnimatorAnimation : BaseUiAnimation
    {
        #region PUBLIC_VAR
        #endregion
        [SerializeField] private Animator animator;
        [SerializeField, AnimatorStateName(animatorField: "animator")]
        private string inAnimation;
        [SerializeField, AnimatorStateName(animatorField: "animator")]
        private string outAnimation;
        private Coroutine coroutine;

        #region overrided methods

        public override void ShowAnimation(Action action)
        {
            base.ShowAnimation(action);
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            coroutine = StartCoroutine(DoShowFx());
        }

        public override void HideAnimation(Action action)
        {
            base.HideAnimation(action);
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            coroutine = StartCoroutine(DoHideFx());
        }

        #region PUBLIC_METHOD
        public void PlayInAnimation()
        {
            animator.Play(inAnimation);
        }

        public void PlayOutAnimation()
        {
            animator.Play(outAnimation);
        }

        #endregion

        #endregion

        public IEnumerator DoShowFx()
        {
            animator.Play(inAnimation);
            yield return new WaitForSeconds(animator.GetAnimatorClipLength(inAnimation));
            onShowComplete?.Invoke();
        }

        public IEnumerator DoHideFx()
        {
            animator.Play(outAnimation);
            yield return new WaitForSeconds(animator.GetAnimatorClipLength(outAnimation));
            onHideComplete?.Invoke();
            coroutine = null;
        }
    }
}