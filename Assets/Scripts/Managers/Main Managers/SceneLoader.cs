using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scene = UnityEngine.SceneManagement.SceneManager;

namespace Tag.NutSort
{
    public class SceneLoader : Manager<SceneLoader>
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        #endregion

        #region UNITY_CALLBACKS

        private void Start()
        {
            LoadingPercentageSetterAction loadingPercentageSetterAction = new LoadingPercentageSetterAction((x) =>
            {
                GlobalUIManager.Instance.GetView<LoadingView>().SetLoadingBar(x);
            }, () => 
            {
                return GlobalUIManager.Instance.GetView<LoadingView>().GetFillProgress();
            }
            , new Vector2(0.2f, 1f));

            StartCoroutine(DoLoadMainScene(() =>
            {
                GlobalUIManager.Instance.GetView<LoadingView>().Hide();
                OnLoadingDone();
            }, loadingPercentageSetterAction));
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        #endregion

        #region PRIVATE_FUNCTIONS
        #endregion

        #region CO-ROUTINES
        private IEnumerator DoLoadMainScene(Action actionToCallAfterLoad = null, LoadingPercentageSetterAction prorgessSetter = null)
        {
            yield return StartCoroutine(LoadAsync(BuildInSceneName.MAIN_SCENE, null));

            prorgessSetter?.SetProgress(1f, prorgessSetter.targetLoadingRange.x, prorgessSetter.targetLoadingRange.y * 0.5f); // Progress from 0% to 50%
            yield return WaitForUtils.EndOfFrame;

            while (MainSceneLoader.Instance == null)
            {
                yield return null;
            }

            while (!MainSceneLoader.Instance.IsLoaded)
            {
                prorgessSetter?.SetProgress(MainSceneLoader.Instance.LoadingProgress, prorgessSetter.targetLoadingRange.y * 0.5f, prorgessSetter.targetLoadingRange.y); // Progress from 50% to 100%
                yield return null;
            }

            while (prorgessSetter != null && prorgessSetter.GetProgress() < prorgessSetter.targetLoadingRange.y)
            {
                yield return null;
            }

            actionToCallAfterLoad?.Invoke();
        }

        IEnumerator LoadAsync(string sceneName, Action onload)
        {
            AsyncOperation asyncOperation = Scene.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            while (!asyncOperation.isDone)
            {
                yield return null;
            }

            Resources.UnloadUnusedAssets();
            onload?.Invoke();
        }

        IEnumerator UnLoadAsync(string sceneName, Action onUnloadDone)
        {
            AsyncOperation asyncOperation = Scene.UnloadSceneAsync(sceneName);

            while (!asyncOperation.isDone)
            {
                yield return null;
            }

            Resources.UnloadUnusedAssets();
            onUnloadDone?.Invoke();
        }

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }

    public class BuildInSceneName
    {
        public const string MAIN_SCENE = "MainScene";
    }

    public class LoadingPercentageSetterAction
    {
        public Action<float> setAction;
        public Func<float> getAction;
        public Vector2 targetLoadingRange;

        public LoadingPercentageSetterAction() { }

        public LoadingPercentageSetterAction(Action<float> setAction, Func<float> getAction, Vector2 targetLoadingRange)
        {
            this.setAction = setAction;
            this.getAction = getAction;
            this.targetLoadingRange = targetLoadingRange;
        }

        public LoadingPercentageSetterAction(LoadingPercentageSetterAction loadingPercentageSetterAction)
        {
            this.setAction = loadingPercentageSetterAction.setAction;
            this.targetLoadingRange = loadingPercentageSetterAction.targetLoadingRange;
        }

        /// <summary>
        /// Sets progress of the loading
        /// </summary>
        /// <param name="progress">Progress value between 0 to 1.</param>
        public void SetProgress(float progress)
        {
            setAction?.Invoke(Mathf.Lerp(targetLoadingRange.x, targetLoadingRange.y, progress));
        }

        public void SetProgress(float progress, float newRangeX, float newRangeY)
        {
            setAction?.Invoke(Mathf.Lerp(newRangeX, newRangeY, progress));
        }

        public float GetProgress()
        {
            return getAction.Invoke();
        }
    }
}
