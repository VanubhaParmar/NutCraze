using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class BoosterActivatedScrew : BaseScrew
    {
        #region PUBLIC_PROPERTIES
        public bool IsExtended => CurrentCapacity > 0;
        public bool IsFullyExtended => CurrentCapacity == MaxCapacity;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Material screwOriginalMaterial;
        [SerializeField] private Material screwTransparentMaterial;
        [SerializeField] private ParticleSystem screwBaseParticle;
        [SerializeField] private GameObject extendScrewObject;

        #endregion

        #region PUBLIC_METHODS
        public override void InitScrew(GridCellId myGridCellId, ScrewConfig screwConfig)
        {
            _gridCellId = myGridCellId;
            base.screwConfig = screwConfig;
            SetData(screwConfig);
            basicScrewVFX.Init(this);

            _screwBaseRenderer.material = IsExtended ? screwOriginalMaterial : screwTransparentMaterial;
            InitScrewDimensionAndMeshData(CurrentCapacity);
            screwState = IsExtended ? ScrewState.Interactable : ScrewState.Locked;
            InitNuts();
        }

        public bool CanExtendScrew()
        {
            return screwState == ScrewState.Locked || CurrentCapacity < MaxCapacity;
        }

        public void ExtendScrew(bool canPlayFx = false)
        {
            screwState = ScrewState.Interactable;
            if (CurrentCapacity == 0)
            {
                _screwBaseRenderer.material = screwOriginalMaterial;
                screwBaseParticle.gameObject.SetActive(canPlayFx);
                if (canPlayFx)
                {
                    screwBaseParticle.Play();
                }
            }
            currentCapacity++;
            InitScrewDimensionAndMeshData(CurrentCapacity, canPlayFx);
            SetScrewInputSize();
            SaveData();
        }

        public override void InitScrewDimensionAndMeshData(int screwCapacity, bool canPlayFx = false)
        {
            ResetScrewMeshes();
            ScrewObjectDimensionInfo currentDimensions = ScrewDimensions.GetScrewObjectDimensionInfo(screwCapacity);
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
            else if (screwCapacity == MaxCapacity)
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
        private void SetData(ScrewConfig config)
        {
            if (config.screwData == null)
            {
                Debug.LogError("Screw data is null");
                maxCapacity = Constant.MAX_BOOSTER_CAPACITY;
                currentCapacity = 0;
                return;
            }
            Dictionary<string, object> screwData = config.screwData;
            //screwId = screwData.GetConverted<int>(ScrewPrefKeys.SCREW_ID, 0);
            maxCapacity = screwData.GetConverted<int>(ScrewPrefKeys.MAX_CAPACITY, 0);
            currentCapacity = screwData.GetConverted<int>(ScrewPrefKeys.CURRENT_CAPACITY, 0);
        }

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