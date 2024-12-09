namespace GameCoreSDK.Puzzle
{
    public class PuzzleController
    {
        private static PuzzleController _instance;
        private readonly LevelNativeBridge _levelNativeBridge;

        private PuzzleController()
        {
            _levelNativeBridge = new LevelNativeBridge();
        }

        public static PuzzleController GetInstance()
        {
            return _instance ??= new PuzzleController();
        }

        public void OnLevelStart(int levelNumber)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _levelNativeBridge.onLevelStart(levelNumber);
#endif
        }

        public void OnLevelComplete(int levelNumber, int timeToClearLevel)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _levelNativeBridge.onLevelComplete(levelNumber, timeToClearLevel);
#endif
        }
    }
}