using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "ScrewArrangementConfigSO", menuName = Constant.GAME_NAME + "/Level Data/ScrewArrangementConfigSO")]
    public class ScrewArrangementConfigSO : ScriptableObject
    {
        #region PUBLIC_VARIABLES
        [SerializeField, ScrewArrangementId] private int arrangementId;
        [SerializeField] private GridCellId gridsize;
        [SerializeField] private CustomVector2 cellSize;
        [SerializeField] private CustomVector3 cellSpacing;

        [Space]
        public List<GridCellId> arrangementCellIds;
        public int ArrangementId { get => arrangementId; set => arrangementId = value; }
        public GridCellId Gridsize => gridsize; 
        public CustomVector2 CellSize => cellSize;
        public CustomVector3 CellSpacing => cellSpacing;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public ScrewArrangementConfig GetArrangementConfig()
        {
            ScrewArrangementConfig arrangementConfig = new ScrewArrangementConfig();
            arrangementConfig.gridSize = Gridsize;
            arrangementConfig.cellSize = CellSize;
            arrangementConfig.cellSpacing = CellSpacing;
            return arrangementConfig;
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

        #region EDITOR
#if UNITY_EDITOR
#endif
        #endregion
    }
}
