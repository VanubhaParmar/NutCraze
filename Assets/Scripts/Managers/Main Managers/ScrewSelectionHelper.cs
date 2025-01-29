using Sirenix.OdinInspector;
using System;

namespace Tag.NutSort
{
    public class ScrewSelectionHelper : SerializedMonoBehaviour
    {
        #region Events
        public event Action<BaseScrew> OnScrewSelected;
        public event Action<BaseScrew> OnScrewDeselected;
        #endregion

        #region Private Variables
        [ShowInInspector, ReadOnly] private BaseScrew currentSelectedScrew;
        #endregion

        #region Properties
        public BaseScrew CurrentSelectedScrew => currentSelectedScrew;
        public bool HasSelectedScrew => currentSelectedScrew != null;
        #endregion

        #region Unity Callbacks
        private void OnDisable()
        {
            ClearSelection();
        }
        #endregion

        #region Public Methods
        public void HandleScrewSelection(BaseScrew screw)
        {
            if (currentSelectedScrew == null)
            {
                TrySelectScrew(screw);
            }
            else if (currentSelectedScrew == screw)
            {
                DeselectCurrentScrew();
            }
            else
            {
                // If selecting a different screw while one is already selected
                DeselectCurrentScrew();
                TrySelectScrew(screw);
            }
        }

        public void ClearSelection()
        {
            if (currentSelectedScrew != null)
            {
                DeselectCurrentScrew();
            }
        }

        public bool CanSelectScrew(BaseScrew screw)
        {
            if (screw == null) return false;
            if (screw.ScrewInteractibilityState == ScrewInteractibilityState.Locked) return false;

            var nutsHolder = screw.GetScrewBehaviour<NutsHolderScrewBehaviour>();
            return nutsHolder != null && !nutsHolder.IsEmpty;
        }
        #endregion

        #region Private Methods
        private void TrySelectScrew(BaseScrew screw)
        {
            if (CanSelectScrew(screw))
            {
                SelectScrew(screw);
            }
        }

        private void SelectScrew(BaseScrew screw)
        {
            currentSelectedScrew = screw;
            OnScrewSelected?.Invoke(screw);
        }

        private void DeselectCurrentScrew()
        {
            if (currentSelectedScrew != null)
            {
                var screwToDeselect = currentSelectedScrew;
                currentSelectedScrew = null;
                OnScrewDeselected?.Invoke(screwToDeselect);
            }
        }
        #endregion

        #region Unity Editor Methods
        [Button]
        private void ClearSelectionDebug()
        {
            ClearSelection();
        }
        #endregion
    }
}