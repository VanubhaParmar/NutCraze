using Sirenix.OdinInspector;

namespace Tag.NutSort
{
    public class GameplayLevelProgressManager : SerializedManager<GameplayLevelProgressManager>
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [ShowInInspector] private PlayerLevelProgressData playerLevelProgressData;
        #endregion

        #region PROPERTIES
        public int CurrentPlayingLevel => playerLevelProgressData.currentPlayingLevel;
        #endregion

        #region UNITY_CALLBACKS

        public override void Awake()
        {
            base.Awake();
            LoadSaveData();
            LevelManager.Instance.RegisterOnLevelLoad(LoadLevelProgress);
            BoosterManager.RegisterOnBoosterUse(OnBoosterUse);
        }


        public override void OnDestroy()
        {
            LevelManager.Instance.DeRegisterOnLevelLoad(LoadLevelProgress);
            BoosterManager.DeRegisterOnBoosterUse(OnBoosterUse);
            base.OnDestroy();
        }

        #endregion

        #region PUBLIC_METHODS
        private void LoadLevelProgress()
        {
            if (playerLevelProgressData == null)
            {
                OnStartNewLevel();
                return;
            }

            if (LevelManager.Instance.CurrentABType != playerLevelProgressData.aBTestType)
            {
                OnStartNewLevel();
                return;
            }

            if (LevelManager.Instance.CurrentLevelDataSO.level != playerLevelProgressData.currentPlayingLevel || LevelManager.Instance.CurrentLevelDataSO.levelType != playerLevelProgressData.currentPlayingLevelType)
            {
                OnStartNewLevel();
                return;
            }

            LoadLevelProgress(playerLevelProgressData);
        }

        private void LoadLevelProgress(PlayerLevelProgressData levelProgressData)
        {
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
                var toScrew = LevelManager.Instance.GetScrewOfGridCell(moveInfo.moveToScrew);

                for (int i = 0; i < moveInfo.transferredNumberOfNuts; i++)
                {
                    var nutTransfer = fromScrew.PopNut();
                    toScrew.AddNut(nutTransfer);
                }

                GameplayManager.Instance.GameplayStateData.gameplayMoveInfos.Add(new GameplayMoveInfo(moveInfo.moveFromScrew, moveInfo.moveToScrew, moveInfo.transferredNumberOfNuts));

                // Reveal surprise nuts
                int myNutCheckColorId = -1;
                for (int i = 0; i < fromScrew.CurrentNutCount; i++)
                {
                    BaseNut nextNut = fromScrew.PeekNut(i);

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
                if (screw.IsSorted())
                {
                    screw.OnScrewSortCompleteImmediate();
                    GameplayManager.Instance.GameplayStateData.OnNutColorSortCompletion(screw.PeekNut().GetNutColorType());
                }
            }

            GameplayManager.Instance.GameplayStateData.levelRunTime = levelProgressData.currentRunningTime;
        }

        public void OnStartNewLevel()
        {
            playerLevelProgressData = new PlayerLevelProgressData();

            playerLevelProgressData.currentPlayingLevel = LevelManager.Instance.CurrentLevelDataSO.level;
            playerLevelProgressData.currentPlayingLevelType = LevelManager.Instance.CurrentLevelDataSO.levelType;
            playerLevelProgressData.aBTestType = LevelManager.Instance.CurrentABType;

            SaveData();
        }

        public void OnBoosterUse(int boosterId)
        {
            if (playerLevelProgressData == null)
                return;

            if (boosterId == BoosterIdConstant.UNDO)
            {
                if (playerLevelProgressData.playerLevelProgressMoveDataInfos.Count > 0)
                    playerLevelProgressData.playerLevelProgressMoveDataInfos.RemoveAt(playerLevelProgressData.playerLevelProgressMoveDataInfos.Count - 1);
            }
            else if (boosterId == BoosterIdConstant.EXTRA_SCREW)
            {
                playerLevelProgressData.boosterScrewCapacityUpgrade++;
            }
            SaveData();
        }

        public void OnLevelTimerSave()
        {
            playerLevelProgressData.currentRunningTime = (int)GameplayManager.Instance.GameplayStateData.levelRunTime;
            SaveData();
        }

        public void OnPlayerMoveConfirmed(GameplayMoveInfo gameplayMoveInfo)
        {
            playerLevelProgressData.playerLevelProgressMoveDataInfos.Add(gameplayMoveInfo.GetPlayerLevelProgressMoveInfo());
            SaveData();
        }

        public void ResetLevelProgress()
        {
            playerLevelProgressData = null;
            SaveData();
        }

        public bool DoesLevelProgressDataExist()
        {
            return playerLevelProgressData != null;
        }

        public LevelType GetLevelProgressDataLevelType()
        {
            return playerLevelProgressData.currentPlayingLevelType;
        }
        #endregion

        #region PRIVATE_METHODS
        private void LoadSaveData()
        {
            playerLevelProgressData = PlayerPersistantData.GetPlayerLevelProgressData();
        }

        private void SaveData()
        {
            PlayerPersistantData.SetPlayerLevelProgressData(playerLevelProgressData);
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}