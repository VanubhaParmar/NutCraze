using Mediation.Runtime.Scripts;

namespace GameCoreSDK.Puzzle
{
    public class PuzzleController
    {
        private static PuzzleController _instance;

        private PuzzleController()
        {
        }

        public static PuzzleController GetInstance()
        {
            return _instance ??= new PuzzleController();
        }

        public void OnLevelStart(int levelNumber)
        {
            LevelNative.Instance.OnLevelStart(levelNumber);
        }

        public void OnLevelComplete(int levelNumber, int timeToClearLevel)
        {
            LevelNative.Instance?.OnLevelComplete(levelNumber, timeToClearLevel);
        }
    }
}