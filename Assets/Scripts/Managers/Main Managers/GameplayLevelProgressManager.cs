using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class GameplayLevelProgressManager : SerializedManager<GameplayLevelProgressManager>
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void OnEnable()
        {
            GameplayManager.onGameplayLevelLoadComplete += GameplayManager_onGameplayLevelLoadComplete;
            GameplayManager.onGameplayLevelStart += GameplayManager_onGameplayLevelStart;
            GameplayManager.onGameplayLevelResume += GameplayManager_onGameplayLevelResume;
        }

        private void OnDisable()
        {
            GameplayManager.onGameplayLevelLoadComplete -= GameplayManager_onGameplayLevelLoadComplete;
            GameplayManager.onGameplayLevelStart -= GameplayManager_onGameplayLevelStart;
            GameplayManager.onGameplayLevelResume -= GameplayManager_onGameplayLevelResume;
        }
        #endregion

        #region PUBLIC_METHODS
        public void LoadLevelProgressData()
        {
            var levelProgressData = PlayerPersistantData.GetPlayerLevelProgressData();

            if (levelProgressData == null)
                return;

            if (LevelManager.Instance.CurrentLevelDataSO.level != levelProgressData.currentPlayingLevel || LevelManager.Instance.CurrentLevelDataSO.levelType != levelProgressData.currentPlayingLevelType)
                return;

            // update booster screw
            if (levelProgressData.boosterScrewCapacityUpgrade > 0)
            {
                var boosterScrew = LevelManager.Instance.LevelScrews.Find(x => x is BoosterActivatedScrew) as BoosterActivatedScrew;

                for (int i = 0; i < levelProgressData.boosterScrewCapacityUpgrade; i++)
                {
                    if (boosterScrew != null && boosterScrew.CanExtendScrew())
                        boosterScrew.ExtendScrew();
                }
            }

            // transfer moves data nuts
            foreach (var moveInfo in levelProgressData.playerLevelProgressMoveDataInfos)
            {
                var fromScrew = LevelManager.Instance.GetScrewOfGridCell(moveInfo.moveFromScrew);
                var fromScrewNutsHolder = fromScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();

                var toScrew = LevelManager.Instance.GetScrewOfGridCell(moveInfo.moveToScrew);
                var toScrewNutsHolder = toScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();

                for (int i = 0; i < moveInfo.transferredNumberOfNuts; i++)
                {
                    var nutTransfer = fromScrewNutsHolder.PopNut();
                    toScrewNutsHolder.AddNut(nutTransfer);
                }

                GameplayManager.Instance.GameplayStateData.gameplayMoveInfos.Add(new GameplayMoveInfo(moveInfo.moveFromScrew, moveInfo.moveToScrew, moveInfo.transferredNumberOfNuts));

                // Reveal surprise nuts
                int myNutCheckColorId = -1;
                for (int i = 0; i < fromScrewNutsHolder.CurrentNutCount; i++)
                {
                    BaseNut nextNut = fromScrewNutsHolder.PeekNut(i);

                    if (nextNut is SurpriseColorNut surpriseNextNut && surpriseNextNut.SurpriseColorNutState == SurpriseColorNutState.COLOR_NOT_REVEALED)
                    {
                        if (myNutCheckColorId == -1 || myNutCheckColorId == surpriseNextNut.GetRealNutColorType())
                        {
                            myNutCheckColorId = surpriseNextNut.GetRealNutColorType();
                            surpriseNextNut.OnRevealColorOfNut();
                        }
                        else
                            break;
                    }
                    else
                        break;
                }
            }

            // check for screw completion
            foreach (var screw in LevelManager.Instance.LevelScrews)
            {
                if (GameplayManager.Instance.IsScrewSortCompleted(screw))
                {
                    NutsHolderScrewBehaviour currentSelectedScrewNutsHolder = screw.GetScrewBehaviour<NutsHolderScrewBehaviour>();

                    screw.OnScrewSortCompleteImmediate();
                    GameplayManager.Instance.GameplayStateData.OnNutColorSortCompletion(currentSelectedScrewNutsHolder.PeekNut().GetNutColorType());
                }
            }
        }

        public void OnStartNewLevel()
        {
            var levelProgressData = new PlayerLevelProgressData();

            levelProgressData.currentPlayingLevel = LevelManager.Instance.CurrentLevelDataSO.level;
            levelProgressData.currentPlayingLevelType = LevelManager.Instance.CurrentLevelDataSO.levelType;

            PlayerPersistantData.SetPlayerLevelProgressData(levelProgressData);
        }

        public void OnBoosterScrewStateUpgrade()
        {
            var levelProgressData = PlayerPersistantData.GetPlayerLevelProgressData();
            levelProgressData.boosterScrewCapacityUpgrade++;
            PlayerPersistantData.SetPlayerLevelProgressData(levelProgressData);
        }

        public void OnPlayerMoveConfirmed(GameplayMoveInfo gameplayMoveInfo)
        {
            var levelProgressData = PlayerPersistantData.GetPlayerLevelProgressData();
            levelProgressData.playerLevelProgressMoveDataInfos.Add(gameplayMoveInfo.GetPlayerLevelProgressMoveInfo());
            PlayerPersistantData.SetPlayerLevelProgressData(levelProgressData);
        }

        public void OnUndoBoosterUsed()
        {
            var levelProgressData = PlayerPersistantData.GetPlayerLevelProgressData();
            if (levelProgressData.playerLevelProgressMoveDataInfos.Count > 0)
                levelProgressData.playerLevelProgressMoveDataInfos.RemoveAt(levelProgressData.playerLevelProgressMoveDataInfos.Count - 1);
            PlayerPersistantData.SetPlayerLevelProgressData(levelProgressData);
        }

        public void OnResetLevelProgress()
        {
            PlayerPersistantData.SetPlayerLevelProgressData(null);
        }

        public bool DoesLevelProgressDataExist()
        {
            return PlayerPersistantData.GetPlayerLevelProgressData() != null;
        }

        public LevelType GetLevelProgressDataLevelType()
        {
            return PlayerPersistantData.GetPlayerLevelProgressData().currentPlayingLevelType;
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        private void GameplayManager_onGameplayLevelLoadComplete()
        {
            LoadLevelProgressData();
        }

        private void GameplayManager_onGameplayLevelStart()
        {
            OnStartNewLevel();
        }

        private void GameplayManager_onGameplayLevelResume()
        {
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}