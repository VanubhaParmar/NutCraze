using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class SurpriseColorNut : BaseNut
    {
        #region PRIVATE_VARIABLES
        [SerializeField, NutColorId] private int surpriseColorId;
        [SerializeField] private GameObject undefinedObjectTransform;
        [SerializeField] private ParticleSystem nutRevealFx;

        private bool isRevealed;
        #endregion

        #region PUBLIC_VARIABLES

        #endregion


        #region PROPERTIES
        public bool IsRevealed => isRevealed;
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void InitNut(NutConfig nutConfig)
        {
            this.nutConfig = nutConfig;
            SetData(nutConfig);
            SetRevealState();
        }

        public void RevealNut()
        {
            if (isRevealed)
                return;
            isRevealed = true;
            PlayRevealAnimation();
        }

        public override int GetNutColorType()
        {
            return isRevealed ? base.GetNutColorType() : surpriseColorId;
        }

        public override int GetRealNutColorType()
        {
            return base.GetNutColorType();
        }
        public override NutConfig GetSaveData()
        {
            if (nutConfig == null)
            {
                nutConfig = new NutConfig()
                {
                    nutType = _nutType,
                };
            }
            nutConfig.nutData = GetNutSaveData();
            return nutConfig;
        }

        public override Dictionary<string, object> GetNutSaveData()
        {
            Dictionary<string, object> nutData = base.GetNutSaveData();
            nutData.Add(NutPrefKeys.IS_REVEALED, isRevealed);
            return nutData;
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetData(NutConfig nutConfig)
        {
            if (nutConfig == null)
                return;
            _nutType = nutConfig.nutType;
            Dictionary<string, object> nutData = nutConfig.nutData;
            _nutColorId = nutData.GetConverted<int>(NutPrefKeys.NUT_ID, -1);
            isRevealed = nutData.GetConverted<bool>(NutPrefKeys.IS_REVEALED, false);
        }

        private void SetRevealState()
        {
            SetNutColorId(isRevealed ? _nutColorId : surpriseColorId);
            undefinedObjectTransform.gameObject.SetActive(!isRevealed);
        }

        private void PlayRevealAnimation()
        {
            transform.localScale = Vector3.one;
            Sequence tweenAnimSeq = DOTween.Sequence().SetId(transform);
            tweenAnimSeq.Append(transform.DoScaleWithReverseOvershoot(Vector3.one * 0.7f, 0.3f, 0.2f, 0.1f).SetEase(Ease.Linear));
            tweenAnimSeq.AppendCallback(() =>
            {
                SetRevealState();
                PlayNutRevealFX();
            });
            tweenAnimSeq.Append(transform.DoScaleWithOvershoot(Vector3.zero, Vector3.one, 0.25f, 0.2f, 0.1f).SetEase(Ease.Linear));
            tweenAnimSeq.onComplete += () =>
            {
                transform.localScale = Vector3.one;
            };
            tweenAnimSeq.onKill += () =>
            {
                SetRevealState();
                transform.localScale = Vector3.one;
            };
        }

        private void PlayNutRevealFX()
        {
            NutColorThemeInfo nutColorTheme = LevelManager.Instance.GetNutTheme(_nutColorId);
            var main = nutRevealFx.main;
            main.startColor = nutColorTheme._mainColor;
            nutRevealFx.gameObject.SetActive(true);
            nutRevealFx.Play();
            Vibrator.LightFeedback();
            CoroutineRunner.Instance.Wait(2f, () =>
            {
                nutRevealFx.Stop();
                nutRevealFx.gameObject.SetActive(false);
            });
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }

    public enum SurpriseColorNutState
    {
        COLOR_NOT_REVEALED,
        COLOR_REVEALED
    }
}