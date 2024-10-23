using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private GameObject _lockedObject;
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

            currentScrewCapacity = 1;
            _screwBehaviours.ForEach(x => x.InitScrewBehaviour(this));
            InitScrewDimensionAndMeshData(currentScrewCapacity);

            // Set screw capacity
            if (TryGetScrewBehaviour(out NutsHolderScrewBehaviour screwBehaviour))
                screwBehaviour.InitMaxScrewCapacity(currentScrewCapacity);

            _screwInteractibilityState = ScrewInteractibilityState.Locked;
            _lockedObject.gameObject.SetActive(true);
        }

        public bool CanExtendScrew()
        {
            return _screwInteractibilityState == ScrewInteractibilityState.Locked || currentScrewCapacity < baseScrewLevelDataInfo.screwNutsCapacity;
        }

        public void ExtendScrew()
        {
            if (_screwInteractibilityState == ScrewInteractibilityState.Locked)
            {
                _screwInteractibilityState = ScrewInteractibilityState.Interactable;
                _lockedObject.gameObject.SetActive(false);
            }
            else
            {
                currentScrewCapacity++;
                InitScrewDimensionAndMeshData(currentScrewCapacity);
                if (TryGetScrewBehaviour(out NutsHolderScrewBehaviour screwBehaviour))
                    screwBehaviour.ChangeMaxScrewCapacity(currentScrewCapacity);
                if (TryGetScrewBehaviour(out ScrewInputBehaviour screwInputBehaviour))
                    screwInputBehaviour.UpdateScrewInputSize();
            }
        }
        public override float GetScrewApproxHeightFromBase()
        {
            return currentScrewCapacity * ScrewDimensions.repeatingTipHeight;
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

    public class BoosterActivatedScrewLevelDataInfo : BaseScrewLevelDataInfo
    {
    }
}