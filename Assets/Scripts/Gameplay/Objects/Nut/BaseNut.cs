using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public abstract class BaseNut : SerializedMonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public int NutType => _nutType;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField, NutTypeId] protected int _nutType;
        [ShowInInspector, ReadOnly, NutColorId] protected int _nutColorId;

        [SerializeField] protected MeshRenderer _nutRenderer;
        [SerializeField] private ParticleSystem _sparkPS;

        protected NutConfig nutConfig;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public virtual void InitNut(NutConfig nutConfig)
        {
            this.nutConfig = nutConfig;
            SetData(nutConfig);
            SetNutColorId(_nutColorId);
        }

        public virtual int GetNutColorType()
        {
            return _nutColorId;
        }

        public virtual int GetNutType()
        {
            return _nutType;
        }

        public virtual int GetRealNutColorType()
        {
            return _nutColorId;
        }

        public virtual void Recycle()
        {
            DOTween.Kill(transform);
            ObjectPool.Instance.Recycle(this);
        }

        public virtual void SetNutColorId(int nutColorId)
        {
            var nutColorTheme = LevelManager.Instance.GetNutTheme(nutColorId);
            MaterialPropertyBlock props = new MaterialPropertyBlock();
            props.SetColor("_Color", nutColorTheme._mainColor);
            props.SetFloat("_SpecularIntensity", nutColorTheme._specularMapIntensity);
            props.SetFloat("_LightIntensity", nutColorTheme._lightIntensity);
            _nutRenderer.SetPropertyBlock(props);
        }

        public void PlaySparkParticle()
        {
            _sparkPS.Play();
        }

        public virtual NutConfig GetSaveData()
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

        public virtual Dictionary<string, object> GetNutSaveData()
        {
            Dictionary<string,object> nutData = new Dictionary<string, object>();
            nutData.Add(NutPrefKeys.NUT_ID, _nutColorId);
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
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}