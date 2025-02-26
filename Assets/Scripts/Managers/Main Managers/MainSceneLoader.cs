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
                int specialLevelNumber = PlayerPersistantData.GetPlayerLevelProgressData().currentPlayingLevel;
                MainSceneUIManager.Instance.GetView<PlaySpecialLevelView>().Show(specialLevelNumber,
                    () => GameplayManager.Instance.OnLoadSpecialLevelAndStartGame(specialLevelNumber, true, true),
                    () => { GameplayManager.Instance.OnLoadCurrentReachedLevel(); GameplayManager.Instance.StartGame(); });
            }
            else if (IsNormalLevelProgressStored())
            {
                GameplayManager.Instance.OnLoadCurrentReachedLevel();
                GameplayManager.Instance.ResumeGame();
            }
            else
            {
                GameplayManager.Instance.OnLoadCurrentReachedLevel();
                GameplayManager.Instance.StartGame();
            }

            MainSceneUIManager.Instance.GetView<GameplayView>().Show();
            AutoOpenPopupHandler.Instance.OnCheckForAutoOpenPopUps();

            SoundHandler.Instance.PlayCoreBackgrondMusic();
        }

        private bool IsSpecialLevelProgressStored()
        {
            return GameplayLevelProgressManager.Instance.DoesLevelProgressDataExist() && GameplayLevelProgressManager.Instance.GetLevelProgressDataLevelType() == LevelType.SPECIAL_LEVEL;
        }

        private bool IsNormalLevelProgressStored()
        {
            return GameplayLevelProgressManager.Instance.DoesLevelProgressDataExist() && GameplayLevelProgressManager.Instance.GetLevelProgressDataLevelType() == LevelType.NORMAL_LEVEL;
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