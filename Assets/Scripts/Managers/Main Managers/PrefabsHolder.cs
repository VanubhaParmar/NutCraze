using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.NutSort
{
    public class PrefabsHolder : SerializedManager<PrefabsHolder>
    {
        #region PUBLIC_VARIABLES
        public GameObject BigConfettiPsPrefab => bigConfettiPsPrefab;
        public ParticleSystem StackFullIdlePsPrefab => _stackFullIdlePS;
        public SurpriseNutAnimation NutRevealAnimation => nutRevealAnimation;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private List<BaseScrew> screwPrefabs;
        [SerializeField] private List<BaseNut> nutPrefabs;
        [SerializeField] private GameObject bigConfettiPsPrefab;
        [SerializeField] private ParticleSystem _stackFullIdlePS;
        [SerializeField] private List<ScrewParticalSystemConfig> _screwParticleSystemsConfig;
        [SerializeField] private SurpriseNutAnimation nutRevealAnimation;
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

        public ParticleSystem GetStackFullParticlesByID(int colorId)
        {
            return _screwParticleSystemsConfig.Find(x => x.nutColorId == colorId).particleSystem;
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