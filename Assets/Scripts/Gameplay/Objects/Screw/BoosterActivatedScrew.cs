using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort
{
    public class BoosterActivatedScrew : BaseScrew
    {
        #region PUBLIC_VARIABLES
        public int CurrentScrewCapacity => currentScrewCapacity;
        #endregion

        #region PRIVATE_VARIABLES
        [ShowInInspector, ReadOnly] private int currentScrewCapacity;
        [SerializeField] private Material screwOriginalMaterial;
        [SerializeField] private Material screwTransparentMaterial;
        [SerializeField] private ParticleSystem screwBaseParticle;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void InitScrew(GridCellId myGridCellId, BaseScrewLevelDataInfo screwLevelDataInfo)
        {
            _gridCellId = myGridCellId;
            baseScrewLevelDataInfo = screwLevelDataInfo;
            basicScrewVFX.Init(this);

            currentScrewCapacity = 1;
            InitScrewDimensionAndMeshData(currentScrewCapacity);
            InitMaxScrewCapacity(currentScrewCapacity);
            screwState = ScrewState.Locked;
            ChangeScrewMaterials(screwTransparentMaterial);
        }

        public bool CanExtendScrew()
        {
            return screwState == ScrewState.Locked || currentScrewCapacity < baseScrewLevelDataInfo.screwNutsCapacity;
        }

        public void ExtendScrew(bool canPlayFx = false)
        {
            if (screwState == ScrewState.Locked)
            {
                screwState = ScrewState.Interactable;
                ChangeScrewMaterials(screwOriginalMaterial);
                if (canPlayFx)
                {
                    screwBaseParticle.gameObject.SetActive(true);
                    screwBaseParticle.Play();
                }
                else
                {
                    screwBaseParticle.gameObject.SetActive(false);
                }
            }
            else
            {
                currentScrewCapacity++;
                InitScrewDimensionAndMeshData(currentScrewCapacity, true);
                ChangeMaxScrewCapacity(currentScrewCapacity);
                SetScrewInputSize();
            }
        }
        public override float GetScrewApproxHeightFromBase()
        {
            return currentScrewCapacity * ScrewDimensions.repeatingTipHeight;
        }
        #endregion

        #region PRIVATE_METHODS
        private void ChangeScrewMaterials(Material material)
        {
            _screwBaseRenderer.material = material;
            _screwNutBaseEndRenderer.material = material;
            screwTopRenderer.material = material;
            _screwNutBaseRenderer.ForEach(x => x.material = material);
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