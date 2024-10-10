using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.NutSort
{
    public class PrefabsHolder : SerializedManager<PrefabsHolder>
    {
        #region PUBLIC_VARIABLES
        public GameObject SmallConfettiPsPrefab => smallConfettiPsPrefab;
        public GameObject BigConfettiPsPrefab => bigConfettiPsPrefab;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private List<BaseScrew> screwPrefabs;
        [SerializeField] private List<BaseNut> nutPrefabs;
        [SerializeField] private GameObject smallConfettiPsPrefab;
        [SerializeField] private GameObject bigConfettiPsPrefab;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public BaseScrew GetScrewPrefab(int screwType)
        {
            return screwPrefabs.Find(x => x.ScrewType == screwType);
        }

        public BaseNut GetNutPrefab(int nutType)
        {
            return nutPrefabs.Find(x => x.NutType == nutType);
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
}