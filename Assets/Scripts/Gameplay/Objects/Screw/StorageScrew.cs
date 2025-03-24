using UnityEngine;

namespace Tag.NutSort
{
    public class StorageScrew : BaseScrew
    {
        #region PRIVATE_VARS
        [SerializeField] private Material storageBaseMaterial;
        [SerializeField] private Color storageHighlightColor = Color.cyan;
        #endregion

        #region PUBLIC_VARS
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        #endregion

        #region OVERRIDE_FUNCTIONS
        public override void InitScrew(GridCellId myGridCellId, BaseScrewLevelDataInfo screwLevelDataInfo)
        {
            base.InitScrew(myGridCellId, screwLevelDataInfo);

            if (storageBaseMaterial != null)
            {
                _screwBaseRenderer.material = storageBaseMaterial;
                _screwNutBaseRenderer.ForEach(x => x.material = storageBaseMaterial);
                _screwNutBaseEndRenderer.material = storageBaseMaterial;
            }
        }

        public override void CheckForScrewSortCompletion()
        {
            return;
        }

        public override bool IsSorted()
        {
            return false;
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region EDITOR
#if UNITY_EDITOR
#endif
        #endregion
    }
}
