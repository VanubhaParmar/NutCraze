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
        _levelNativeBridge.onLevelStart(levelNumber);
    }

    public void OnLevelComplete(int levelNumber, int timeToClearLevel)
    {
        _levelNativeBridge.onLevelComplete(levelNumber, timeToClearLevel);
    }
}