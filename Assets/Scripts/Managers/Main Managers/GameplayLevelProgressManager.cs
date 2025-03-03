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

        public override void Awake()
        {
            base.Awake();
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
            var levelProgressData = PlayerPersistantData.GetPlayerLevelProgressData();

            if (levelProgressData == null)
            {
                OnStartNewLevel();
                return;
            }

            if (LevelManager.Instance.CurrentABType != levelProgressData.aBTestType)
            {
                OnStartNewLevel();
                return;
            }

            if (LevelManager.Instance.CurrentLevelDataSO.level != levelProgressData.currentPlayingLevel || LevelManager.Instance.CurrentLevelDataSO.levelType != levelProgressData.currentPlayingLevelType)
            {
                OnStartNewLevel();
                return;
            }

            LoadLevelProgress(levelProgressData);
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
                if (screw.IsSorted())
                {
                    NutsHolderScrewBehaviour currentSelectedScrewNutsHolder = screw.GetScrewBehaviour<NutsHolderScrewBehaviour>();
                    screw.OnScrewSortCompleteImmediate();
                    GameplayManager.Instance.GameplayStateData.OnNutColorSortCompletion(currentSelectedScrewNutsHolder.PeekNut().GetNutColorType());
                }
            }

            GameplayManager.Instance.GameplayStateData.levelRunTime = levelProgressData.currentRunningTime;
        }

        public void OnStartNewLevel()
        {
            var levelProgressData = new PlayerLevelProgressData();

            levelProgressData.currentPlayingLevel = LevelManager.Instance.CurrentLevelDataSO.level;
            levelProgressData.currentPlayingLevelType = LevelManager.Instance.CurrentLevelDataSO.levelType;
            levelProgressData.aBTestType = LevelManager.Instance.CurrentABType;

            PlayerPersistantData.SetPlayerLevelProgressData(levelProgressData);
        }

        public void OnBoosterUse(int boosterId)
        {
            var levelProgressData = PlayerPersistantData.GetPlayerLevelProgressData();
            if (boosterId == BoosterIdConstant.UNDO)
            {
                if (levelProgressData.playerLevelProgressMoveDataInfos.Count > 0)
                    levelProgressData.playerLevelProgressMoveDataInfos.RemoveAt(levelProgressData.playerLevelProgressMoveDataInfos.Count - 1);
            }
            else if (boosterId == BoosterIdConstant.EXTRA_SCREW)
            {
                levelProgressData.boosterScrewCapacityUpgrade++;
            }
            PlayerPersistantData.SetPlayerLevelProgressData(levelProgressData);
        }

        public void OnLevelTimerSave()
        {
            var levelProgressData = PlayerPersistantData.GetPlayerLevelProgressData();
            levelProgressData.currentRunningTime = (int)GameplayManager.Instance.GameplayStateData.levelRunTime;
            PlayerPersistantData.SetPlayerLevelProgressData(levelProgressData);
        }

        public void OnPlayerMoveConfirmed(GameplayMoveInfo gameplayMoveInfo)
        {
            var levelProgressData = PlayerPersistantData.GetPlayerLevelProgressData();
            UnityEngine.Debug.Log("levelProgressData " + (levelProgressData == null));
            levelProgressData.playerLevelProgressMoveDataInfos.Add(gameplayMoveInfo.GetPlayerLevelProgressMoveInfo());
            PlayerPersistantData.SetPlayerLevelProgressData(levelProgressData);
        }

        public void ResetLevelProgress()
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
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}