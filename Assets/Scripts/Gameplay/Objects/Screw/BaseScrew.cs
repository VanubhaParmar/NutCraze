using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.NutSort
{
    public abstract class BaseScrew : SerializedMonoBehaviour
    {
        #region PRIVATE_VARIABLES
        [SerializeField, ScrewTypeId] protected int _screwType;
        [SerializeField] protected ScrewDimensionsDataSO _screwDimensionsData;
        [SerializeField] protected MeshRenderer _screwBaseRenderer;
        [SerializeField] protected List<MeshRenderer> _screwNutBaseRenderer;
        [SerializeField] protected MeshRenderer _screwNutBaseEndRenderer;
        [SerializeField] protected MeshRenderer screwTopRenderer;
        [SerializeField] protected Animator capAnimation;
        [SerializeField] protected BasicScrewVFX basicScrewVFX;
        [SerializeField] protected Transform inputTransform;
        [SerializeField] protected Transform nutsParent;
        [SerializeField] protected NutStack nutsHolderStack;

        protected ScrewState screwState;
        protected ScrewConfig saveData;
        private ScrewStageConfig currentStage;
        #endregion

        #region PUBLIC_VARIABLES
        #endregion

        #region PROPERTIES
        public ScrewState ScrewState => screwState;
        public GridCellId CellId => saveData.cellId;
        public int Id => saveData.id;
        public int ScrewType => _screwType;
        public ScrewDimensionsDataSO ScrewDimensions => _screwDimensionsData;
        public int Capacity => saveData.capacity;
        public Animator CapAnimation => capAnimation;
        public int MaxNutCapacity => nutsHolderStack.stackCapacity;
        public int CurrentNutCount => nutsHolderStack.Count;
        public bool CanAddNut => CurrentNutCount < MaxNutCapacity;
        public bool IsEmpty => nutsHolderStack.Count == 0;
        public ScrewStageConfig CurrentStage => currentStage;
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region VIRTUAL_METHODS
        public virtual void Init(ScrewConfig saveConfig)
        {
            this.saveData = saveConfig;
            basicScrewVFX.Init(this);
            InitScrewDimensionAndMeshData(Capacity);
            SetScrewInputSize();
            screwState = ScrewState.Interactable;
            InitMaxScrewCapacity(Capacity);
            InitScrewStage(saveConfig.currentStage);
        }

        public virtual void InitScrewStage(int screwstage)
        {
            if (saveData.TryGetScrewStage(screwstage, out ScrewStageConfig screwStageSaveConfig))
            {
                currentStage = screwStageSaveConfig;
                InitNuts(screwStageSaveConfig.nutDatas);
            }
        }

        public virtual void InitNuts(NutConfig[] nutDatas)
        {
            for (int i = nutDatas.Length - 1; i >= 0; i--)
            {
                BaseNut myNut = ObjectPool.Instance.Spawn(ResourceManager.Instance.GetNut(nutDatas[i].nutType), nutsParent);
                myNut.gameObject.SetActive(true);
                myNut.Init(nutDatas[i]);
                AddNut(myNut);
            }
        }

        public virtual void OnScrewClick()
        {
            if (GameplayManager.Instance.IsPlayingLevel && ScrewState == ScrewState.Interactable)
            {
                ScrewSelectionHelper.Instance.OnScrewClicked(this);
            }
        }

        public virtual void SetScrewInteractableState(ScrewState screwInteractibilityState)
        {
            screwState = screwInteractibilityState;
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
            return transform.position + _screwDimensionsData.GetScrewObjectDimensionInfo(Capacity).screwCapPositionOffsetFromBase;
        }
        public virtual float GetTotalScrewApproxHeight()
        {
            return ScrewDimensions.baseHeight + GetScrewApproxHeightFromBase();
        }
        public virtual float GetScrewApproxHeightFromBase()
        {
            return Capacity * ScrewDimensions.repeatingTipHeight;
        }

        public virtual void Recycle()
        {
            DOTween.Kill(transform);
            RecycleAllNuts();
            basicScrewVFX.Recycle();
            ObjectPool.Instance.Recycle(this);
        }

        public virtual void RecycleAllNuts()
        {
            while (!IsEmpty)
            {
                BaseNut nut = PopNut();
                nut?.Recycle();
            }
        }

        public virtual void OnScrewSortCompleteImmediate()
        {
            SetScrewInteractableState(ScrewState.Locked);
            PlayStackFullIdlePS();
            capAnimation.transform.position = GetScrewCapPosition();
            capAnimation.transform.localScale = Vector3.one * ScrewDimensions.screwCapScale;
            capAnimation.gameObject.SetActive(true);
            capAnimation.Play("Default_State");
        }

        public virtual void CheckForCurrentStageCompletion()
        {
            if (IsCurrentStageSorted())
            {
                PlayScrewSortCompletionAnimation();
                GameplayManager.Instance.OnScrewSortComplete(this);
                SetScrewInteractableState(ScrewState.Locked);
            }
        }


        public virtual void CheckForScrewSortCompletion()
        {
            if (IsSorted())
            {
                PlayScrewSortCompletionAnimation();
                GameplayManager.Instance.OnScrewSortComplete(this);
                SetScrewInteractableState(ScrewState.Locked);
            }
        }

        public virtual bool IsCurrentStageSorted()
        {
            if (!CanAddNut)
            {
                int firstNutColorId = PeekNut().GetNutColorType();
                int colorCountOfNuts = GameplayManager.Instance.GameplayStateData.levelNutsUniqueColorsCount[firstNutColorId];
                int currentColorCount = 0;
                for (int i = 0; i < CurrentNutCount; i++)
                {
                    int colorOfNut = PeekNut(i).GetNutColorType();
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


        public virtual bool IsSorted()
        {
            if (!CanAddNut)
            {
                int firstNutColorId = PeekNut().GetNutColorType();
                int colorCountOfNuts = GameplayManager.Instance.GameplayStateData.levelNutsUniqueColorsCount[firstNutColorId];

                int currentColorCount = 0;
                for (int i = 0; i < CurrentNutCount; i++)
                {
                    int colorOfNut = PeekNut(i).GetNutColorType();
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

        #region PUBLIC_METHODS
        public void PlayStackFullIdlePS()
        {
            basicScrewVFX.PlayStackFullIdlePS();
        }

        public void StopStackFullIdlePS()
        {
            basicScrewVFX.StopStackFullIdlePS();
        }
        public void InitMaxScrewCapacity(int capacity)
        {
            nutsHolderStack = new NutStack(capacity);
        }

        public void ChangeMaxScrewCapacity(int capacity)
        {
            nutsHolderStack.stackCapacity = capacity;
        }

        public void AddNut(BaseNut baseNut, bool setPosition = true)
        {
            baseNut.transform.SetParent(nutsParent);
            if (setPosition)
                baseNut.transform.position = GetNextScrewPosition();

            nutsHolderStack.Push(baseNut);
        }

        public int GetNutIndex(BaseNut baseNut)
        {
            return nutsHolderStack.GetNutIndex(baseNut);
        }

        public Vector3 GetMyNutPosition(BaseNut baseNut)
        {
            return GetScrewPosition(GetNutIndex(baseNut));
        }

        public Vector3 GetScrewPosition(int positionIndexCount) // from 0 to Capacity - 1
        {
            return GetBasePosition() + ScrewDimensions.GetNutPositionOffsetFromBase(positionIndexCount);
        }

        public Vector3 GetTopScrewPosition()
        {
            return GetScrewPosition(CurrentNutCount - 1);
        }

        public Vector3 GetNextScrewPosition()
        {
            return GetScrewPosition(nutsHolderStack.Count);
        }

        public BaseNut PopNut()
        {
            if (!IsEmpty)
                return nutsHolderStack.Pop();
            return null;
        }

        public BaseNut PeekNut(int index = 0)
        {
            if (!IsEmpty)
                return nutsHolderStack.Peek(index);

            return null;
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
            int myNutCheckColorId = -1;
            for (int i = 0; i < CurrentNutCount; i++)
            {
                BaseNut nextNut = PeekNut(i);

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
        #endregion

        #region PROTECTED_METHODS
        protected void SetScrewInputSize()
        {
            inputTransform.position = GetBasePosition() + ScrewDimensions.baseHeight * Vector3.down;
            inputTransform.localScale = new Vector3(1f, GetTotalScrewApproxHeight(), 1f);
        }

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

    public enum ScrewState
    {
        Interactable,
        Locked
    }
}
