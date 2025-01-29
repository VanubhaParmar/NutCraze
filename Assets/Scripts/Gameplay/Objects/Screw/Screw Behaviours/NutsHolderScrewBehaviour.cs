using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class NutsHolderScrewBehaviour : BaseScrewBehaviour
    {
        #region PUBLIC_VARIABLES
        public int MaxNutCapacity => nutsHolderStack.stackCapacity;
        public int CurrentNutCount => nutsHolderStack.Count;
        public bool CanAddNut => CurrentNutCount < MaxNutCapacity;
        public bool IsEmpty => nutsHolderStack.Count == 0;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Transform nutsParent;
        [SerializeField] private NutStack nutsHolderStack = new NutStack();
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void InitScrewBehaviour(BaseScrew myScrew)
        {
            base.InitScrewBehaviour(myScrew);
            InitNutsHolderBehaviour();
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
            return myScrew.GetBasePosition() + myScrew.ScrewDimensions.GetNutPositionOffsetFromBase(positionIndexCount);
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
        #endregion

        #region PRIVATE_METHODS
        private void InitNutsHolderBehaviour()
        {
            nutsHolderStack.ResetStack();
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }

    public class NutStack
    {
        public int Count => nutsHolder.Count;
        public List<BaseNut> nutsHolder = new List<BaseNut>(); // First nut in the list will always be the bottom most nut of the screw
        public int stackCapacity;
        public NutStack() { }
        public NutStack(int stackCapacity)
        {
            this.stackCapacity = stackCapacity;
        }

        public void Push(BaseNut baseNut)
        {
            nutsHolder.Add(baseNut);
        }

        public BaseNut Pop() 
        {
            return nutsHolder.PopAt(nutsHolder.Count - 1);
        }

        public BaseNut Peek(int count)
        {
            int selectionIndex = nutsHolder.Count - 1 - count;
            if (selectionIndex >= 0)
                return nutsHolder[selectionIndex];
            return null;
        }

        public int GetNutIndex(BaseNut baseNut)
        {
            return nutsHolder.IndexOf(baseNut);
        }

        public void ResetStack()
        {
            nutsHolder.Clear();
        }
    }
}