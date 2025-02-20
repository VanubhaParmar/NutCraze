using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "ScrewDimensionsDataSO", menuName = Constant.GAME_NAME + "/Gameplay/Screw/ScrewDimensionsDataSO")]
    public class ScrewDimensionsDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARIABLES
        public float baseHeight;
        public float baseScale;

        [Space]
        public float repeatingTipHeight;
        public float repeatingTipScale;

        [Space]
        public float lastTipHeight;
        public float lastTipScale;

        [Space]
        public float screwCapHeight;
        public float screwCapScale;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public ScrewObjectDimensionInfo GetScrewObjectDimensionInfo(int tipCount)
        {
            ScrewObjectDimensionInfo screwObjectDimensionInfo = new ScrewObjectDimensionInfo();

            int repeatingTipsCount = Mathf.Max(0, tipCount - 1);

            for (int i = 0; i < repeatingTipsCount; i++)
            {
                Vector3 tipPosition = Vector3.zero;
                tipPosition.y = (i * repeatingTipHeight);
                screwObjectDimensionInfo.repeatingTipsPositionOffsetFromBase.Add(tipPosition);
            }

            screwObjectDimensionInfo.lastTipPositionOffsetFromBase.y = repeatingTipsCount * repeatingTipHeight;
            screwObjectDimensionInfo.screwCapPositionOffsetFromBase.y = (repeatingTipsCount * repeatingTipHeight) + lastTipHeight;

            return screwObjectDimensionInfo;
        }

        public Vector3 GetNutPositionOffsetFromBase(int nutIndex)
        {
            Vector3 nutPos = Vector3.zero;
            nutPos.y = Mathf.Max(0, nutIndex) * repeatingTipHeight;
            return nutPos;
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

    public class ScrewObjectDimensionInfo
    {
        public List<Vector3> repeatingTipsPositionOffsetFromBase = new List<Vector3>();
        public Vector3 lastTipPositionOffsetFromBase = Vector3.zero;
        public Vector3 screwCapPositionOffsetFromBase = Vector3.zero;
    }
}