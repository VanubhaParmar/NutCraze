using DG.Tweening;
using Sirenix.OdinInspector;
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
        [ShowInInspector, ReadOnly, ColorId] protected int _nutColorId;

        [SerializeField] protected MeshRenderer _nutRenderer;
        [SerializeField] private ParticleSystem _sparkPS;

        protected BaseNutLevelDataInfo baseNutLevelDataInfo;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public virtual void InitNut(BaseNutLevelDataInfo baseNutLevelDataInfo)
        {
            this.baseNutLevelDataInfo = baseNutLevelDataInfo;
            _nutColorId = baseNutLevelDataInfo.nutColorTypeId;

            SetNutColorId(baseNutLevelDataInfo.nutColorTypeId);
        }

        public virtual int GetNutColorType()
        {
            return _nutColorId;
        }

        public virtual int GetOriginalNutColorType()
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
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}