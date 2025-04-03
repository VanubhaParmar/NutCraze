using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort
{
    public class BoosterActivatedScrew : BaseScrew
    {
        #region PUBLIC_PROPERTIES
        public int CurrentScrewCapacity => currentScrewCapacity;
        #endregion

        #region PRIVATE_VARIABLES
        [ShowInInspector, ReadOnly] private int currentScrewCapacity;
        [SerializeField] private Material screwOriginalMaterial;
        [SerializeField] private Material screwTransparentMaterial;
        [SerializeField] private ParticleSystem screwBaseParticle;
        [SerializeField] private GameObject extendScrewObject;

        #endregion

        #region PUBLIC_METHODS
        public override void InitScrew(GridCellId myGridCellId, BaseScrewLevelDataInfo screwLevelDataInfo)
        {
            _gridCellId = myGridCellId;
            baseScrewLevelDataInfo = screwLevelDataInfo;
            basicScrewVFX.Init(this);
            currentScrewCapacity = 0;

            _screwBaseRenderer.material = screwTransparentMaterial;
            InitScrewDimensionAndMeshData(currentScrewCapacity);
            InitMaxScrewCapacity(currentScrewCapacity);

            screwState = ScrewState.Locked;
        }

        public bool CanExtendScrew()
        {
            return screwState == ScrewState.Locked || currentScrewCapacity < baseScrewLevelDataInfo.screwNutsCapacity;
        }

        public void ExtendScrew(bool canPlayFx = false)
        {
            screwState = ScrewState.Interactable;
            if (currentScrewCapacity == 0)
            {
                _screwBaseRenderer.material = screwOriginalMaterial;
                screwBaseParticle.gameObject.SetActive(canPlayFx);
                if (canPlayFx)
                {
                    screwBaseParticle.Play();
                }
            }

            currentScrewCapacity++;
            InitScrewDimensionAndMeshData(currentScrewCapacity, canPlayFx);
            ChangeMaxScrewCapacity(currentScrewCapacity);
            SetScrewInputSize();
        }

        public override float GetScrewApproxHeightFromBase()
        {
            return currentScrewCapacity * ScrewDimensions.repeatingTipHeight;
        }

        public override void InitScrewDimensionAndMeshData(int screwCapacity, bool canPlayFx = false)
        {
            ResetScrewMeshes();
            ScrewObjectDimensionInfo currentDimensions = ScrewDimensions.GetScrewObjectDimensionInfo(screwCapacity);
            int maxCapacity = baseScrewLevelDataInfo.screwNutsCapacity;
            Vector3 basePosition = _screwBaseRenderer.transform.position + ScrewDimensions.baseHeight * Vector3.up;
            for (int i = 0; i < screwCapacity - 1; i++)
            {
                var baseNutMesh = FindInactiveBaseNutMesh() ?? InstantiateNewBaseNutMesh();
                baseNutMesh.transform.position = basePosition + currentDimensions.repeatingTipsPositionOffsetFromBase[i];
                baseNutMesh.material = screwOriginalMaterial;
                baseNutMesh.gameObject.SetActive(true);
            }

            if (screwCapacity == 0)
            {
                SetupEmptyScrew(currentDimensions, basePosition);
            }
            else if (screwCapacity == maxCapacity)
            {
                SetupFullCapacityScrew(currentDimensions, basePosition, canPlayFx);
            }
            else
            {
                SetupPartialCapacityScrew(screwCapacity, currentDimensions, basePosition, canPlayFx);
            }


            capAnimation.transform.position = basePosition + currentDimensions.screwCapPositionOffsetFromBase;
        }

        public override void ResetScrewMeshes()
        {
            _screwNutBaseRenderer.ForEach(x => x.gameObject.SetActive(false));
            _screwNutBaseEndRenderer.gameObject.SetActive(false);
            capAnimation.gameObject.SetActive(false);
        }
        #endregion

        #region PRIVATE_METHODS
        private void SpawnParticleAtPosition(Transform targetTransform)
        {
            ParticleSystem ps = Instantiate(ResourceManager.ScrewEndParticle, targetTransform);
            ps.transform.position = new Vector3(targetTransform.position.x - 0.25f, targetTransform.position.y, -0.7f);
            ps.gameObject.SetActive(true);
            ps.Play();
        }

        private void SetupEmptyScrew(ScrewObjectDimensionInfo dimensions, Vector3 basePosition)
        {
            _screwNutBaseEndRenderer.gameObject.SetActive(false);
            extendScrewObject.SetActive(true);
            extendScrewObject.transform.position = basePosition + dimensions.lastTipPositionOffsetFromBase;
        }

        private void SetupFullCapacityScrew(ScrewObjectDimensionInfo dimensions, Vector3 basePosition, bool canPlayFx)
        {
            _screwNutBaseEndRenderer.gameObject.SetActive(true);
            _screwNutBaseEndRenderer.material = screwOriginalMaterial;
            extendScrewObject.SetActive(false);
            _screwNutBaseEndRenderer.transform.position = basePosition + dimensions.lastTipPositionOffsetFromBase;
            extendScrewObject.transform.position = basePosition + dimensions.lastTipPositionOffsetFromBase;
            if (canPlayFx)
            {
                SpawnParticleAtPosition(_screwNutBaseEndRenderer.transform);
            }
        }

        private void SetupPartialCapacityScrew(int screwCapacity, ScrewObjectDimensionInfo currentDimensions, Vector3 basePosition, bool canPlayFx)
        {
            _screwNutBaseEndRenderer.gameObject.SetActive(true);
            _screwNutBaseEndRenderer.material = screwOriginalMaterial;
            _screwNutBaseEndRenderer.transform.position = basePosition + currentDimensions.lastTipPositionOffsetFromBase;

            extendScrewObject.SetActive(true);
            if (canPlayFx)
            {
                SpawnParticleAtPosition(extendScrewObject.transform);
            }
            ScrewObjectDimensionInfo nextDimensions = ScrewDimensions.GetScrewObjectDimensionInfo(screwCapacity + 1);
            extendScrewObject.transform.position = basePosition + nextDimensions.lastTipPositionOffsetFromBase;
        }
        #endregion
    }
}