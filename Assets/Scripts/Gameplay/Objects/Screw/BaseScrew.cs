using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.NutSort
{
    public abstract class BaseScrew : SerializedMonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public ScrewInteractibilityState ScrewInteractibilityState => _screwInteractibilityState;
        public GridCellId GridCellId => _gridCellId;
        public int ScrewType => _screwType;
        public ScrewDimensionsDataSO ScrewDimensions => _screwDimensionsData;
        public int ScrewNutsCapacity => baseScrewLevelDataInfo.screwNutsCapacity;
        public MeshRenderer ScrewTopRenderer => _screwTopRenderer;
        #endregion

        #region PRIVATE_VARIABLES
        [ShowInInspector, ReadOnly] protected GridCellId _gridCellId;
        [ShowInInspector, ReadOnly] protected ScrewInteractibilityState _screwInteractibilityState;

        [SerializeField, ScrewTypeId] protected int _screwType;
        [SerializeField] protected List<BaseScrewBehaviour> _screwBehaviours;
        [SerializeField] protected ScrewDimensionsDataSO _screwDimensionsData;

        [Space]
        [SerializeField] protected MeshRenderer _screwBaseRenderer;
        [SerializeField] protected List<MeshRenderer> _screwNutBaseRenderer;
        [SerializeField] protected MeshRenderer _screwNutBaseEndRenderer;
        [SerializeField] protected MeshRenderer _screwTopRenderer;

        protected BaseScrewLevelDataInfo baseScrewLevelDataInfo;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public virtual void InitScrew(GridCellId myGridCellId, BaseScrewLevelDataInfo screwLevelDataInfo)
        {
            _gridCellId = myGridCellId;
            baseScrewLevelDataInfo = screwLevelDataInfo;

            _screwBehaviours.ForEach(x => x.InitScrewBehaviour(this));
            InitScrewDimensionAndMeshData();
            _screwInteractibilityState = ScrewInteractibilityState.Interactable;
        }

        public virtual bool TryGetScrewBehaviour<T>(out T screwBehaviour) where T : BaseScrewBehaviour
        {
            screwBehaviour = _screwBehaviours.Find(x => x is T) as T;
            return screwBehaviour != null;
        }

        public virtual T GetScrewBehaviour<T>() where T : BaseScrewBehaviour
        {
            return _screwBehaviours.Find(x => x is T) as T;
        }

        public virtual void SetScrewInteractableState(ScrewInteractibilityState screwInteractibilityState)
        {
            _screwInteractibilityState = screwInteractibilityState;
        }

        public virtual Vector3 GetBasePosition() // Base position is at the Centre-Top point of the circle base of the screw
        {
            return transform.position;
        }
        public virtual Vector3 GetTopPosition()
        {
            return transform.position + Vector3.up * GetScrewApproxHeightFromBase();
        }
        public virtual Vector3 GetScrewCapPosition()
        {
            return transform.position + _screwDimensionsData.GetScrewObjectDimensionInfo(ScrewNutsCapacity).screwCapPositionOffsetFromBase;
        }
        public virtual float GetTotalScrewApproxHeight()
        {
            return ScrewDimensions.baseHeight + GetScrewApproxHeightFromBase();
        }
        public virtual float GetScrewApproxHeightFromBase()
        {
            return ScrewNutsCapacity * ScrewDimensions.repeatingTipHeight;
        }
        public virtual void Recycle()
        {
            DOTween.Kill(transform);
            ObjectPool.Instance.Recycle(this);
        }
        #endregion

        #region PRIVATE_METHODS
        protected void InitScrewDimensionAndMeshData()
        {
            ScrewObjectDimensionInfo screwObjectDimensionInfo = ScrewDimensions.GetScrewObjectDimensionInfo(baseScrewLevelDataInfo.screwNutsCapacity);
            ResetScrewMeshes();

            _screwBaseRenderer.gameObject.SetActive(true);

            for (int i = 0; i < baseScrewLevelDataInfo.screwNutsCapacity - 1; i++)
            {
                var baseNutMesh = FindInactiveBaseNutMesh() ?? InstantiateNewBaseNutMesh();
                baseNutMesh.transform.position = _screwBaseRenderer.transform.position + ScrewDimensions.baseHeight * Vector3.up + screwObjectDimensionInfo.repeatingTipsPositionOffsetFromBase[i];
                baseNutMesh.gameObject.SetActive(true);
            }

            _screwNutBaseEndRenderer.transform.position = _screwBaseRenderer.transform.position + ScrewDimensions.baseHeight * Vector3.up + screwObjectDimensionInfo.lastTipPositionOffsetFromBase;
            _screwNutBaseEndRenderer.gameObject.SetActive(true);

            _screwTopRenderer.transform.position = _screwBaseRenderer.transform.position + ScrewDimensions.baseHeight * Vector3.up + screwObjectDimensionInfo.screwCapPositionOffsetFromBase;

            // Set screw capacity
            if (TryGetScrewBehaviour(out NutsHolderScrewBehaviour screwBehaviour))
                screwBehaviour.InitMaxScrewCapacity(baseScrewLevelDataInfo.screwNutsCapacity);
        }

        protected MeshRenderer FindInactiveBaseNutMesh()
        {
            return _screwNutBaseRenderer.Find(x => !x.gameObject.activeInHierarchy);
        }

        protected MeshRenderer InstantiateNewBaseNutMesh()
        {
            var meshBaseNut = Instantiate(_screwNutBaseRenderer.First(), _screwNutBaseRenderer.First().transform.parent);
            _screwNutBaseRenderer.Add(meshBaseNut);
            return meshBaseNut;
        }

        protected void ResetScrewMeshes()
        {
            _screwBaseRenderer.gameObject.SetActive(false);
            _screwNutBaseRenderer.ForEach(x => x.gameObject.SetActive(false));
            _screwNutBaseEndRenderer.gameObject.SetActive(false);
            _screwTopRenderer.gameObject.SetActive(false);
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }

    public abstract class BaseScrewBehaviour : SerializedMonoBehaviour
    {
        protected BaseScrew myScrew;
        public virtual void InitScrewBehaviour(BaseScrew myScrew)
        {
            this.myScrew = myScrew;
        }
    }

    public enum ScrewInteractibilityState
    {
        Interactable,
        Locked
    }
}