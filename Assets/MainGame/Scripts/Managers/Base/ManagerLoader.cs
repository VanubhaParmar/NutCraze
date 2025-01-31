using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tag.nut_sort {
    public class ManagerLoader : MonoBehaviour
    {
        #region PUBLIC_VARS
        public float LoadingProgress { get; private set; }

        #endregion

        #region PRIVATE_VARS

        [SerializeField] private List<ManagerInstanceLoader> managers = new List<ManagerInstanceLoader>();

        #endregion

        #region UNITY_CALLBACKS

        public void Awake()
        {
            StartCoroutine(LoadManager());
        }
        #endregion

        #region PUBLIC_FUNCTIONS

        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region CO-ROUTINES

        private IEnumerator LoadManager()
        {
            for (int i = 0; i < managers.Count; i++)
            {
                managers[i].gameObject.SetActive(true);
                while (!managers[i].Loaded)
                {
                    yield return 0;
                }
            }
        }

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
