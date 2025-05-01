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
        [SerializeField] protected ScrewDimensionsDataSO _screwDimensionsData;
        [SerializeField] protected MeshRenderer _screwBaseRenderer;
        [SerializeField] protected List<MeshRenderer> _screwNutBaseRenderer;
        [SerializeField] protected MeshRenderer _screwNutBaseEndRenderer;
        [SerializeField] protected MeshRenderer screwTopRenderer;
        [SerializeField] protected Animator capAnimation;
        [SerializeField] protected BasicScrewVFX basicScrewVFX;
        [SerializeField] protected Transform inputTransform;
        [SerializeField] protected Transform nutsParent;
        [SerializeField] private NutStack nutsHolderStack = new NutStack();

        protected GridCellId _gridCellId;
        protected ScrewState screwState;
        protected ScrewConfig screwConfig;
        protected int currentCapacity;
        protected int maxCapacity;
        #endregion

        #region PUBLIC_VARIABLES
        #endregion

        #region PROPERTIES
        public ScrewState ScrewState => screwState;
        public GridCellId GridCellId => _gridCellId;
        public int ScrewType => _screwType;
        public ScrewDimensionsDataSO ScrewDimensions => _screwDimensionsData;
        public int CurrentCapacity => currentCapacity;
        public int MaxCapacity => maxCapacity;
        public Animator CapAnimation => capAnimation;
        public int CurrentNutCount => Nuts.Count;
        public virtual bool CanAddNut => CurrentNutCount < CurrentCapacity;
        public bool IsEmpty => Nuts.Count == 0;
        public List<BaseNut> Nuts => nutsHolderStack.nutsHolder;
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region VIRTUAL_METHODS
        public virtual void InitScrew(GridCellId myGridCellId, ScrewConfig screwConfig)
        {
            _gridCellId = myGridCellId;
            this.screwConfig = screwConfig;
            basicScrewVFX.Init(this);
            SetData(screwConfig);
            InitScrewDimensionAndMeshData(CurrentCapacity);
            SetScrewInputSize();
            screwState = ScrewState.Interactable;
            InitNuts();
        }

        public virtual void InitScrew(ScrewConfig screwConfig)
        {
            this.screwConfig = screwConfig;
            basicScrewVFX.Init(this);
            SetData(screwConfig);
            InitScrewDimensionAndMeshData(CurrentCapacity);
            SetScrewInputSize();
            screwState = ScrewState.Interactable;
            InitNuts();
        }

        public virtual void InitNuts()
        {
            Dictionary<string, object> screwData = screwConfig.screwData;
            if (screwData == null)
                return;
            List<NutConfig> nutData = screwData.GetJObjectCast<List<NutConfig>>(ScrewPrefKeys.NUT_DATA, new List<NutConfig>());
            for (int j = nutData.Count - 1; j >= 0; j--)
            {
                BaseNut myNut = ObjectPool.Instance.Spawn(ResourceManager.Instance.GetNut(nutData[j].nutType), nutsParent);
                myNut.gameObject.SetActive(true);
                myNut.InitNut(nutData[j]);
                AddNut(myNut, canSave: false);
            }
        }

        public virtual void SetNewGridCellId(GridCellId cellId)
        {
            _gridCellId = cellId;
        }

        public virtual void ResetPosition()
        {
            transform.position = LevelProgressManager.Instance.ArrangementConfig.GetCellPosition(GridCellId);
        }

        public virtual ScrewConfig GetScrewConfig()
        {
            if (screwConfig == null)
            {
                screwConfig = new ScrewConfig();
                screwConfig.screwType = ScrewType;
            }
            screwConfig.screwData = GetSaveData();
            return screwConfig;
        }

        public virtual Dictionary<string, object> GetSaveData()
        {
            Dictionary<string, object> saveData = new Dictionary<string, object>();
            saveData.Add(ScrewPrefKeys.MAX_CAPACITY, maxCapacity);
            saveData.Add(ScrewPrefKeys.CURRENT_CAPACITY, currentCapacity);
            saveData.Add(ScrewPrefKeys.NUT_DATA, GetNutSaveData());
            return saveData;
        }


        public virtual List<NutConfig> GetNutSaveData()
        {
            List<NutConfig> nutConfigs = new List<NutConfig>();
            for (int i = Nuts.Count - 1; i >= 0; i--)
                nutConfigs.Add(Nuts[i].GetSaveData());
            return nutConfigs;
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
            return transform.position + _screwDimensionsData.GetScrewObjectDimensionInfo(CurrentCapacity).screwCapPositionOffsetFromBase;
        }
        public virtual float GetTotalScrewApproxHeight()
        {
            return ScrewDimensions.baseHeight + GetScrewApproxHeightFromBase();
        }
        public virtual float GetScrewApproxHeightFromBase()
        {
            return CurrentCapacity * ScrewDimensions.repeatingTipHeight;
        }

        public virtual void Recycle()
        {
            DOTween.Kill(transform);
            Nuts.ForEach(x => x?.Recycle());
            Nuts.Clear();
            basicScrewVFX.Recycle();
            ObjectPool.Instance.Recycle(this);
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

        public virtual void CheckForScrewSortCompletion()
        {
            if (IsSorted())
            {
                PlayScrewSortCompletionAnimation();
                GameplayManager.Instance?.OnScrewSortComplete(this);
                SetScrewInteractableState(ScrewState.Locked);
            }
        }

        public virtual bool IsSorted()
        {
            if (IsEmpty)
                return false;

            if (!CanAddNut)
            {
                int firstNutColorId = PeekNut().GetNutColorType();
                if (firstNutColorId == NutColorIdConstant.surprise) //surprise colorNut
                    return false;
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

        public virtual void InitScrewDimensionAndMeshData(int screwCapacity, bool canPlayFx = false)
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

        public virtual void ResetScrewMeshes()
        {
            _screwBaseRenderer.gameObject.SetActive(false);
            _screwNutBaseRenderer.ForEach(x => x.gameObject.SetActive(false));
            _screwNutBaseEndRenderer.gameObject.SetActive(false);
            capAnimation.gameObject.SetActive(false);
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

        public void AddNut(BaseNut baseNut, bool setPosition = true, bool canSave = true)
        {
            baseNut.transform.SetParent(nutsParent);
            if (setPosition)
                baseNut.transform.position = GetNextScrewPosition();
            nutsHolderStack.Push(baseNut);
            if (canSave)
                SaveData();
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
            return GetScrewPosition(Nuts.Count);
        }

        public BaseNut PopNut()
        {
            if (!IsEmpty)
            {
                BaseNut baseNut = nutsHolderStack.Pop();
                SaveData();
                return baseNut;
            }
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

                if (nextNut is SurpriseColorNut surpriseNextNut && !surpriseNextNut.IsRevealed)
                {
                    if (myNutCheckColorId == -1 || myNutCheckColorId == surpriseNextNut.GetRealNutColorType())
                    {
                        myNutCheckColorId = surpriseNextNut.GetRealNutColorType();
                        surpriseNextNut.RevealNut();
                    }
                    else
                        break;
                }
                else
                    break;
            }
            SaveData();
        }

        public void SaveData()
        {
            LevelProgressManager.Instance.SaveScrewData(GridCellId, GetScrewConfig());
        }
        #endregion

        #region PROTECTED_METHODS
        protected void SetScrewInputSize()
        {
            inputTransform.position = GetBasePosition() + ScrewDimensions.baseHeight * Vector3.down;
            inputTransform.localScale = new Vector3(1f, GetTotalScrewApproxHeight(), 1f);
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

        #endregion

        #region PRIVATE_METHODS
        private void SetData(ScrewConfig screwConfig)
        {
            if (screwConfig.screwData == null)
            {
                maxCapacity = Constant.MAX_BOOSTER_CAPACITY;
                currentCapacity = maxCapacity;
                return;
            }
            maxCapacity = screwConfig.screwData.GetConverted<int>(ScrewPrefKeys.MAX_CAPACITY, 0);
            currentCapacity = screwConfig.screwData.GetConverted<int>(ScrewPrefKeys.CURRENT_CAPACITY, MaxCapacity);
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

    [Serializable]
    public class ScrewParticalSystemConfig
    {
        [NutColorId] public int nutColorId;
        public GameObject particleSystem;
    }
}