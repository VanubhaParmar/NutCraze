using DG.Tweening;
using Sirenix.OdinInspector;
using System;
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

        private Dictionary<int, ParticleSystem> _stackCompletePS;

        protected BaseScrewLevelDataInfo baseScrewLevelDataInfo;

        private ParticleSystem stackIdlePS;
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
            InitScrewDimensionAndMeshData(baseScrewLevelDataInfo.screwNutsCapacity);
            _screwInteractibilityState = ScrewInteractibilityState.Interactable;

            // Set screw capacity
            if (TryGetScrewBehaviour(out NutsHolderScrewBehaviour screwBehaviour))
                screwBehaviour.InitMaxScrewCapacity(baseScrewLevelDataInfo.screwNutsCapacity);
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
            if (stackIdlePS != null)
                ObjectPool.Instance.Recycle(stackIdlePS);
            ObjectPool.Instance.Recycle(this);
        }
        public void PlayStackFullIdlePS()
        {
            stackIdlePS = ObjectPool.Instance.Spawn(PrefabsHolder.Instance.StackFullIdlePsPrefab, this.transform);
            stackIdlePS.transform.localPosition = new Vector3(0, 1.2f, -1);
            stackIdlePS.Play();
        }

        public void PlayStackFullParticlesByID(int nutColorId)
        {
            var psSpawn = ObjectPool.Instance.Spawn(PrefabsHolder.Instance.GetStackFullParticlesByID(nutColorId), this.transform);
            psSpawn.gameObject.GetComponent<ParticleSystem>()?.Play();
            //psSpawn.Play();
        }

        public void StopStackFullIdlePS()
        {
            if (stackIdlePS != null)
                ObjectPool.Instance.Recycle(stackIdlePS);
            stackIdlePS = null;
        }

        public virtual void OnScrewSortCompleteImmediate()
        {
            SetScrewInteractableState(ScrewInteractibilityState.Locked);
            PlayStackFullIdlePS();
            _screwTopRenderer.transform.position = GetScrewCapPosition();
            _screwTopRenderer.gameObject.SetActive(true);
            _screwTopRenderer.transform.localScale = Vector3.one * ScrewDimensions.screwCapScale;
        }
        #endregion

        #region PRIVATE_METHODS
        protected void InitScrewDimensionAndMeshData(int screwCapacity)
        {
            ScrewObjectDimensionInfo screwObjectDimensionInfo = ScrewDimensions.GetScrewObjectDimensionInfo(screwCapacity);
            ResetScrewMeshes();

            _screwBaseRenderer.gameObject.SetActive(true);

            for (int i = 0; i < screwCapacity - 1; i++)
            {
                var baseNutMesh = FindInactiveBaseNutMesh() ?? InstantiateNewBaseNutMesh();
                baseNutMesh.transform.position = _screwBaseRenderer.transform.position + ScrewDimensions.baseHeight * Vector3.up + screwObjectDimensionInfo.repeatingTipsPositionOffsetFromBase[i];
                baseNutMesh.gameObject.SetActive(true);
            }

            _screwNutBaseEndRenderer.transform.position = _screwBaseRenderer.transform.position + ScrewDimensions.baseHeight * Vector3.up + screwObjectDimensionInfo.lastTipPositionOffsetFromBase;
            _screwNutBaseEndRenderer.gameObject.SetActive(true);

            _screwTopRenderer.transform.position = _screwBaseRenderer.transform.position + ScrewDimensions.baseHeight * Vector3.up + screwObjectDimensionInfo.screwCapPositionOffsetFromBase;
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

    [Serializable]
    public class ScrewParticalSystemConfig
    {
        [NutColorId] public int nutColorId;
        public GameObject particleSystem;
    }
}