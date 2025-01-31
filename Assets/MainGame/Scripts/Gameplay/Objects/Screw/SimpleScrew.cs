using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.tag.nut_sort {
    public class SimpleScrew : BaseScrew
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void InitScrew(GridCellId myGridCellId, BaseScrewLevelDataInfo screwLevelDataInfo)
        {
            base.InitScrew(myGridCellId, screwLevelDataInfo);
        }
        #endregion

        #region PRIVATE_METHODS
        
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        //[Button]
        //public void TestSetup(int height)
        //{
        //    InitScrewNutsLevelData(new SimpleScrewLevelDataInfo() { screwHeight = height });
        //}
        #endregion
    }

    public class SimpleScrewLevelDataInfo : BaseScrewLevelDataInfo
    {
    }
}