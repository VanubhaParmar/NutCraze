//using Spine.Unity;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Tag.NutSort {
    public class TutorialHandAnimation : MonoBehaviour
    {
        #region PUBLIC_VARS

        public Transform startPosition;
        public Transform endPosition;
        public SortingGroup SortingGroup
        {
            get => _handSortingGroup;
            set
            {
                _handSortingGroup.sortingLayerID = value.sortingLayerID;
                _handSortingGroup.sortingOrder = value.sortingOrder;
            }
        }

        public Transform HandTransform { get { return handTransform; } }
        public RectTransform HandRectTransform { get { return handTransform.GetComponent<RectTransform>(); } }
        #endregion

        #region PRIVATE_SERIALIZE_FIELD_VARS

        //[SerializeField] private SkeletonAnimation handSkeletonAnimation;
        //[SerializeField] private Transform clickParticleParent;
        [SerializeField] private Transform handTransform;
        //[SerializeField] private TrailRenderer handTrailTransform;
        [SerializeField] private float speed;
        [SerializeField] private float handInOutSpeed;
        [SerializeField] private AnimationCurve animationCurve;
        [SerializeField] private SortingGroup _handSortingGroup;

        #endregion

        #region PRIVATE_VARS

        private Action nextAnimation;
        private Action currentAnimation;
        private Coroutine moveAnimation;

        #endregion

        #region UNITY_CALLBACKS

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        public void OnTapAnimation(bool isAnimation = false)
        {
            ResetHand();
            HandIn();
            nextAnimation = isAnimation ? HandTranslateAnimation : HandTapAnimation;
            if (!isAnimation)
                handTransform.position = endPosition.position;
        }

        public void PlayHandMove(bool isLoop = false)
        {
            ResetHand();
            HandIn();
            nextAnimation = HandMoveAnimation;
            currentAnimation = isLoop ? () => { PlayHandMove(true); } : null;
        }

        public void PlayHandMoveWithCurve(bool isLoop = false)
        {
            ResetHand();
            HandIn();
            nextAnimation = HandMoveWithCurve;
            currentAnimation = isLoop ? () => { PlayHandMoveWithCurve(true); } : null;
        }

        public void PlayHandIdleMoveAnimation()
        {
            ResetHand();
            Sequence moveSeq = DOTween.Sequence().SetTarget(handTransform.transform).SetLoops(-1);

            Vector3 targetPosition = handTransform.transform.position + Vector3.up * 0.4f;
            Vector3 currentPosition = handTransform.transform.position;

            moveSeq.Append(handTransform.DOMove(targetPosition, 1f).SetEase(Ease.OutQuad));
            moveSeq.Append(handTransform.DOMove(currentPosition, 1f).SetEase(Ease.OutQuad));
        }

        #endregion

        #region HandInAnimation

        private void HandTranslateAnimation()
        {
            StartCoroutine(StartHandTranslateToEndPosAnimation());
            nextAnimation = HandTapAnimation;
        }

        private IEnumerator StartHandTranslateToEndPosAnimation()
        {
            float i = 0;
            while (i < 1)
            {
                i += Time.deltaTime * speed;
                Vector3 newPosition = Vector3.Lerp(startPosition.position, endPosition.position, animationCurve.Evaluate(i));
                Quaternion newRotation = Quaternion.Lerp(Quaternion.identity, endPosition.rotation, animationCurve.Evaluate(i));

                handTransform.position = newPosition;
                handTransform.localRotation = newRotation;

                yield return null;
            }
            handTransform.position = endPosition.position;
            nextAnimation?.Invoke();
        }

        public void HandIn()
        {
            StartCoroutine(HandInAnimation());
        }

        private IEnumerator HandInAnimation()
        {
            float i = 0;
            handTransform.position = startPosition.position;
            while (i < 1)
            {
                i += Time.deltaTime * handInOutSpeed;
                Vector3 newScale = Vector3.Lerp(Vector3.one * 1.4f, Vector3.one, i);
                handTransform.localScale = newScale;
                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
            nextAnimation.Invoke();
        }

        #endregion

        #region HandOutAnimation

        public void HandOut()
        {
            StartCoroutine(HandOutAnimation());
        }

        private IEnumerator HandOutAnimation()
        {
            float i = 0;
            while (i < 1)
            {
                i += Time.deltaTime * handInOutSpeed;
                Vector3 newScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.4f, i);
                Vector3 newRotation = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, 25), i);
                handTransform.localRotation = Quaternion.Euler(newRotation);
                handTransform.localScale = newScale;
                yield return null;
            }
            StopCoroutine(moveAnimation);
            currentAnimation?.Invoke();
        }

        #endregion

        #region TapAnimation

        public void HandTapAnimation()
        {
            //handSkeletonAnimation.AnimationState.SetAnimation(0, "Tap", true);
            //clickParticleParent.gameObject.SetActive(true);
        }

        private IEnumerator OnHandUp()
        {
            yield return new WaitForSeconds(0.5f);
            HandOut();
        }

        public void OnStopTapping()
        {
            HandOut();
        }

        public void ResetHand()
        {
            //handTrailTransform.Clear();
            //handTrailTransform.gameObject.SetActive(false);
            //handSkeletonAnimation.AnimationState.ClearTrack(0);
            StopAllCoroutines();
            handTransform.localRotation = Quaternion.Euler(Vector3.zero);
            DOTween.Kill(handTransform.transform);
            //clickParticleParent.gameObject.SetActive(false);
        }

        #endregion

        #region HandMoveAnimation

        public void HandMoveAnimation()
        {
            moveAnimation = StartCoroutine(StartHandMoveAnimation());
        }

        private IEnumerator StartHandMoveAnimation()
        {
            float i = 0;
            //handTrailTransform.Clear();
            //handTrailTransform.gameObject.SetActive(true);
            while (i < 1)
            {
                i += Time.deltaTime * speed;
                Vector3 newPosition = Vector3.Lerp(startPosition.position, endPosition.position, animationCurve.Evaluate(i));
                handTransform.position = newPosition;
                yield return null;
            }
            //handTrailTransform.gameObject.SetActive(false);
            StartCoroutine(OnHandUp());
        }

        #endregion

        #region HandMoveWithCurve

        public void HandMoveWithCurve()
        {
            StartCoroutine(StartHandMoveWithCurve());
        }

        private IEnumerator StartHandMoveWithCurve()
        {
            float i = 0;
            while (i < 1)
            {
                i += Time.deltaTime * speed;
                Vector3 newPosition = Vector3.Lerp(startPosition.position, endPosition.position, i);
                newPosition.y = (animationCurve.Evaluate(i) * 100) + newPosition.y;
                handTransform.position = newPosition;
                yield return null;
            }
            StartCoroutine(OnHandUp());
        }

        #endregion
    }
}