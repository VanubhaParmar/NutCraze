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
        private PlaySpecialLevelView PlaySpecialLevelView => MainSceneUIManager.Instance.GetView<PlaySpecialLevelView>();
        private GameplayView GameplayView => MainSceneUIManager.Instance.GetView<GameplayView>();
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            StartCoroutine(LoadManager());
        }
        #endregion

        #region PUBLIC_METHODS
        #endregion

        #region PRIVATE_METHODS
        private void OnMainSceneLoadingDone()
        {
            OnLoadingDone();

            if (IsSpecialLevelProgressStored())
            {
                LevelManager.Instance.CanLoadSpecialLevel(out LevelData levelData);
                int specialLevelNumber = LevelProgressManager.Instance.CurrentLevel;
                //PlaySpecialLevelView.Show(specialLevelNumber, GameplayManager.Instance.LoadSpecailLevel, GameplayManager.Instance.LoadNormalLevel);
            }
            else
            {
                GameplayManager.Instance.LoadNormalLevel();
            }
            GameplayView.Show();
            AutoOpenPopupHandler.Instance.OnCheckForAutoOpenPopUps();
            SoundHandler.Instance.PlayCoreBackgrondMusic();
        }

        private bool IsSpecialLevelProgressStored()
        {
            return LevelProgressManager.Instance.IsLevelProgressDataExist && LevelProgressManager.Instance.CurrentLevelType == LevelType.SPECIAL_LEVEL;
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        IEnumerator LoadManager()
        {
            WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
            LoadingProgress = 0f;
            yield return waitForEndOfFrame;

            for (int i = 0; i < managers.Count; i++)
            {
                managers[i].gameObject.SetActive(true);
                while (!managers[i].Loaded)
                {
                    yield return 0;
                }

                LoadingProgress = ((float)(i + 1)) / managers.Count;
                yield return waitForEndOfFrame;
            }
            yield return waitForEndOfFrame;
            LoadingProgress = 1f;
            yield return waitForEndOfFrame;
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