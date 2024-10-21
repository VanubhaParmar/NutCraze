using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class LevelHintsManager : SerializedManager<LevelHintsManager>
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private GameObject correctSignPrefab;
        [SerializeField] private GameObject incorrectSignPrefab;

        [Space]
        [SerializeField] private float heightOffsetFromScrew;
        [SerializeField] private float inTime = 0.5f;
        [SerializeField] private float animationHeightOffset = 0.5f;

        [ShowInInspector, ReadOnly] private List<GameObject> generatedCorrectSigns = new List<GameObject>();
        [ShowInInspector, ReadOnly] private List<GameObject> generatedIncorrectSigns = new List<GameObject>();
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void StartShowingHints()
        {
            StartCoroutine(InputHintsCheckerCoroutine());
        }

        public void StopShowingHints()
        {
            StopAllCoroutines();
            HideScrewHints();
        }
        #endregion

        #region PRIVATE_METHODS
        private void ShowScrewHints()
        {
            var allScrews = LevelManager.Instance.LevelScrews;
            var selectedScrew = GameplayManager.Instance.CurrentSelectedScrew;

            List<BaseScrew> allowedScrews = new List<BaseScrew>();
            List<BaseScrew> notAllowedScrews = new List<BaseScrew>();

            if (selectedScrew != null)
            {
                var selectedScrewNutHolder = selectedScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();

                foreach (var screw in allScrews)
                {
                    if (screw != selectedScrew)
                    {
                        var targetScrewNutHolder = screw.GetScrewBehaviour<NutsHolderScrewBehaviour>();

                        if (!targetScrewNutHolder.CanAddNut || (!targetScrewNutHolder.IsEmpty && selectedScrewNutHolder.PeekNut().GetNutColorType() != targetScrewNutHolder.PeekNut().GetNutColorType()))
                        {
                            notAllowedScrews.Add(screw);
                            continue;
                        }

                        allowedScrews.Add(screw);
                    }
                }
            }

            foreach (var screw in allowedScrews)
            {
                var correctSign = GetCorrectSignHint();
                correctSign.gameObject.SetActive(true);

                correctSign.transform.position = screw.GetBasePosition() + (screw.GetTotalScrewApproxHeight() + heightOffsetFromScrew) * Vector3.up;
                PlayHintInAnimation(correctSign);
            }

            foreach (var screw in notAllowedScrews)
            {
                var incorrectSign = GetInCorrectSignHint();
                incorrectSign.gameObject.SetActive(true);

                incorrectSign.transform.position = screw.GetBasePosition() + (screw.GetTotalScrewApproxHeight() + heightOffsetFromScrew) * Vector3.up;
                PlayHintInAnimation(incorrectSign);
            }
        }

        private void PlayHintOutAnimation(GameObject signObject)
        {
            DOTween.Kill(signObject.transform);

            Sequence objectSeq = DOTween.Sequence().SetTarget(signObject.transform);
            var signSprite = signObject.GetComponentInChildren<SpriteRenderer>();
            var color = signSprite.color;
            color.a = 1f;
            signSprite.color = color;

            signSprite.transform.localPosition = Vector3.zero;

            objectSeq.Append(signSprite.DOFade(0f, inTime));
            objectSeq.Join(signSprite.transform.DOLocalMove(Vector3.up * animationHeightOffset, inTime).SetEase(Ease.OutSine));
            objectSeq.onComplete += () => {
                signObject.gameObject.SetActive(false);
            };
        }

        private void PlayHintInAnimation(GameObject signObject)
        {
            DOTween.Kill(signObject.transform);

            Sequence objectSeq = DOTween.Sequence().SetTarget(signObject.transform);
            var signSprite = signObject.GetComponentInChildren<SpriteRenderer>();
            var color = signSprite.color;
            color.a = 0f;
            signSprite.color = color;

            signSprite.transform.localPosition = Vector3.up * animationHeightOffset;

            objectSeq.Append(signSprite.DOFade(1f, inTime));
            objectSeq.Join(signSprite.transform.DOLocalMove(Vector3.zero, inTime).SetEase(Ease.InSine));
        }

        private void HideScrewHints()
        {
            foreach (var sign in generatedCorrectSigns)
            {
                if (sign.gameObject.activeInHierarchy)
                {
                    PlayHintOutAnimation(sign);
                }
            }

            foreach (var sign in generatedIncorrectSigns)
            {
                if (sign.gameObject.activeInHierarchy)
                {
                    PlayHintOutAnimation(sign);
                }
            }
        }

        private GameObject GetCorrectSignHint()
        {
            var signHint = generatedCorrectSigns.Find(x => !x.gameObject.activeInHierarchy);
            if (signHint == null)
            {
                signHint = Instantiate(correctSignPrefab, transform);
                signHint.gameObject.SetActive(false);
                generatedCorrectSigns.Add(signHint);
            }

            return signHint;
        }

        private GameObject GetInCorrectSignHint()
        {
            var signHint = generatedIncorrectSigns.Find(x => !x.gameObject.activeInHierarchy);
            if (signHint == null)
            {
                signHint = Instantiate(incorrectSignPrefab, transform);
                signHint.gameObject.SetActive(false);
                generatedIncorrectSigns.Add(signHint);
            }

            return signHint;
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        IEnumerator InputHintsCheckerCoroutine()
        {
            while (true)
            {
                yield return new WaitUntil(() => GameplayManager.Instance.CurrentSelectedScrew != null);
                ShowScrewHints();

                yield return new WaitUntil(() => GameplayManager.Instance.CurrentSelectedScrew == null);
                HideScrewHints();
            }
        }
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}