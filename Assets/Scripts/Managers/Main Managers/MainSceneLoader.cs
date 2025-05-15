using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class MainSceneLoader : SerializedManager<MainSceneLoader>
    {
        #region PUBLIC_VARIABLES
        public List<ManagerInstanceLoader> managers;
        #endregion

        #region PRIVATE_VARIABLES
        public float LoadingProgress { get; private set; }
        public bool IsInitialized { get; private set; }
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            IsInitialized = false;
            StartCoroutine(LoadManager());
        }
        #endregion

        #region PUBLIC_METHODS
        #endregion

        #region PRIVATE_METHODS
        private void OnMainSceneLoadingDone()
        {
            OnLoadingDone();
            IsInitialized = true;
            GameplayManager.Instance.StartMainGamePlay();
            SoundHandler.Instance.PlayCoreBackgrondMusic();
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        IEnumerator LoadManager()
        {
            LoadingProgress = 0f;
            yield return WaitForUtils.EndOfFrame;

            for (int i = 0; i < managers.Count; i++)
            {
                managers[i].gameObject.SetActive(true);
                while (!managers[i].Loaded)
                {
                    yield return 0;
                }

                LoadingProgress = ((float)(i + 1)) / managers.Count;
                yield return WaitForUtils.EndOfFrame;
            }
            yield return WaitForUtils.EndOfFrame;
            LoadingProgress = 1f;
            yield return WaitForUtils.EndOfFrame;
            OnMainSceneLoadingDone();
        }
        #endregion

        #region UI_CALLBACKS
        #endregion
    }

    public enum SceneType
    {
        Loading,
        MainScene,
    }
}