using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort
{
    public class ManagerInstanceLoader : MonoBehaviour
    {
        #region public veriables

        public bool Loaded => loaded;

        public LoadingType loadingType;
        [ShowInInspector, ReadOnly] private bool loaded;

        #endregion

        #region unity callback

        public void Awake()
        {
            if (loadingType == LoadingType.auto)
            {
                loaded = true;
            }
        }

        public void OnLoadingStart()
        {
            if (loadingType == LoadingType.Manual)
                loaded = false;
        }

        public void OnLoadingDone()
        {
            if (loadingType == LoadingType.Manual)
                loaded = true;
        }

        #endregion
    }
}