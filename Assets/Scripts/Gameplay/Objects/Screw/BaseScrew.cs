using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.NutSort
{
    public abstract class BaseScrew : SerializedMonoBehaviour
    {

        #region PRIVATE_VARIABLES
        [SerializeField, ScrewTypeId] protected int _screwType;
        [SerializeField] protected List<BaseScrewBehaviour> _screwBehaviours;
        [SerializeField] protected ScrewDimensionsDataSO _screwDimensionsData;
        [SerializeField] protected MeshRenderer _screwBaseRenderer;
        [SerializeField] protected List<MeshRenderer> _screwNutBaseRenderer;
        [SerializeField] protected MeshRenderer _screwNutBaseEndRenderer;
        [SerializeField] protected MeshRenderer screwTopRenderer;
        [SerializeField] protected Animator capAnimation;
        [SerializeField] protected BasicScrewVFX basicScrewVFX;

        [ShowInInspector, ReadOnly] protected GridCellId _gridCellId;
        [ShowInInspector, ReadOnly] protected ScrewInteractibilityState _screwInteractibilityState;

        protected BaseScrewLevelDataInfo baseScrewLevelDataInfo;
        #endregion

        #region PUBLIC_VARIABLES
        #endregion

        #region PROPERTIES

        public ScrewInteractibilityState ScrewInteractibilityState => _screwInteractibilityState;
        public GridCellId GridCellId => _gridCellId;
        public int ScrewType => _screwType;
        public ScrewDimensionsDataSO ScrewDimensions => _screwDimensionsData;
        public int ScrewNutsCapacity => baseScrewLevelDataInfo.screwNutsCapacity;
        public Animator CapAnimation => capAnimation;
        public MeshRenderer ScrewTopRenderer => screwTopRenderer;
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public virtual void InitScrew(GridCellId myGridCellId, BaseScrewLevelDataInfo screwLevelDataInfo)
        {
            _gridCellId = myGridCellId;
            baseScrewLevelDataInfo = screwLevelDataInfo;
            basicScrewVFX.Init(this);

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
            basicScrewVFX.Recycle();
            ObjectPool.Instance.Recycle(this);
        }

        public void PlayStackFullIdlePS()
        {
            basicScrewVFX.PlayStackFullIdlePS();
        }

        public void StopStackFullIdlePS()
        {
            basicScrewVFX.StopStackFullIdlePS();
        }

        public virtual void OnScrewSortCompleteImmediate()
        {
            SetScrewInteractableState(ScrewInteractibilityState.Locked);
            PlayStackFullIdlePS();
            capAnimation.transform.position = GetScrewCapPosition();
            capAnimation.transform.localScale = Vector3.one * ScrewDimensions.screwCapScale;
            capAnimation.gameObject.SetActive(true);
            capAnimation.Play("Default_State");
        }

        public void PlayScrewSortCompletionAnimation()
        {
            basicScrewVFX.PlayScrewSortCompletion();
        }

        public void LiftTheFirstNutAnimation()
        {
            basicScrewVFX.LiftTheFirstNutAnimation();
        }

        public void PutBackFirstSelectedNutAnimation()
        {
            basicScrewVFX.PutBackFirstSelectedNutAnimation();
        }

        public void CheckForSurpriseNutColorReveal()
        {
            NutsHolderScrewBehaviour currentSelectedScrewNutsHolder = GetScrewBehaviour<NutsHolderScrewBehaviour>();

            int myNutCheckColorId = -1;
            for (int i = 0; i < currentSelectedScrewNutsHolder.CurrentNutCount; i++)
            {
                BaseNut nextNut = currentSelectedScrewNutsHolder.PeekNut(i);

                if (nextNut is SurpriseColorNut surpriseNextNut && surpriseNextNut.SurpriseColorNutState == SurpriseColorNutState.COLOR_NOT_REVEALED)
                {
                    if (myNutCheckColorId == -1 || myNutCheckColorId == surpriseNextNut.GetRealNutColorType())
                    {
                        myNutCheckColorId = surpriseNextNut.GetRealNutColorType();
                        surpriseNextNut.transform.localScale = Vector3.one;
                        basicScrewVFX.PlayRevealAnimationOnNut(surpriseNextNut);
                    }
                    else
                        break;
                }
                else
                    break;
            }
        }

        public void CheckForScrewSortCompletion()
        {
            if (IsSorted())
            {
                PlayScrewSortCompletionAnimation();
                GameplayManager.Instance.OnScrewSortComplete(this);
                SetScrewInteractableState(ScrewInteractibilityState.Locked);
            }
        }

        public bool IsSorted()
        {
            NutsHolderScrewBehaviour currentSelectedScrewNutsHolder = GetScrewBehaviour<NutsHolderScrewBehaviour>();
            if (!currentSelectedScrewNutsHolder.CanAddNut)
            {
                int firstNutColorId = currentSelectedScrewNutsHolder.PeekNut().GetNutColorType();
                int colorCountOfNuts = GameplayManager.Instance.GameplayStateData.levelNutsUniqueColorsCount[firstNutColorId];

                int currentColorCount = 0;
                for (int i = 0; i < currentSelectedScrewNutsHolder.CurrentNutCount; i++)
                {
                    int colorOfNut = currentSelectedScrewNutsHolder.PeekNut(i).GetNutColorType();
                    if (colorOfNut == firstNutColorId)
                        currentColorCount++;
                    else
                        break;
                }

                if (currentColorCount == colorCountOfNuts) // Screw Sort is Completed
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region PRIVATE_METHODS
        protected void InitScrewDimensionAndMeshData(int screwCapacity, bool canPlayFx = false)
        {
            ScrewObjectDimensionInfo screwObjectDimensionInfo = ScrewDimensions.GetScrewObjectDimensionInfo(screwCapacity);
            ResetScrewMeshes();

            _screwBaseRenderer.gameObject.SetActive(true);

            for (int i = 0; i < screwCapacity - 1; i++)
            {
                var baseNutMesh = FindInactiveBaseNutMesh() ?? InstantiateNewBaseNutMesh();
                baseNutMesh.transform.position = _screwBaseRenderer.transform.position + ScrewDimensions.baseHeight * Vector3.up + screwObjectDimensionInfo.repeatingTipsPositionOffsetFromBase[i];
                baseNutMesh.gameObject.SetActive(true);
                if (canPlayFx)
                {
                    ParticleSystem ps = Instantiate(ResourceManager.ScrewEndParticle, baseNutMesh.transform.parent);
                    ps.transform.position = new Vector3(baseNutMesh.transform.position.x, baseNutMesh.transform.position.y, -0.7f);
                    ps.gameObject.SetActive(true);
                    ps.Play();
                }
            }

            _screwNutBaseEndRenderer.transform.position = _screwBaseRenderer.transform.position + ScrewDimensions.baseHeight * Vector3.up + screwObjectDimensionInfo.lastTipPositionOffsetFromBase;
            _screwNutBaseEndRenderer.gameObject.SetActive(true);

            capAnimation.transform.position = _screwBaseRenderer.transform.position + ScrewDimensions.baseHeight * Vector3.up + screwObjectDimensionInfo.screwCapPositionOffsetFromBase;
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
            capAnimation.gameObject.SetActive(false);
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