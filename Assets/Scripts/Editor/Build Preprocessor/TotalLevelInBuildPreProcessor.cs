using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Tag.NutSort;

public class TotalLevelInBuildPreProcessor : IPreprocessBuildWithReport
{
    #region PUBLIC_VARIABLES
    // Set the callback order (lower values execute earlier)
    public int callbackOrder => 0;
    #endregion

    #region PRIVATE_VARIABLES
    #endregion

    #region PROPERTIES
    #endregion

    #region UNITY_CALLBACKS
    #endregion

    #region PUBLIC_METHODS
    public void OnPreprocessBuild(BuildReport report)
    {
        // Load the ScriptableObject you want to modify before the build
        GameMainDataSO gameMainDataSO = AssetDatabase.LoadAssetAtPath<GameMainDataSO>(EditorConstant.GameMainDataSO_Path);

        if (gameMainDataSO != null)
        {
            gameMainDataSO.totalLevelsInBuild = gameMainDataSO.GetTotalNumberOfLevels();
            // Mark the asset as dirty so Unity knows it has changed
            EditorUtility.SetDirty(gameMainDataSO);

            // Save the asset to apply changes to the .asset file
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Total Levels in Build : {gameMainDataSO.totalLevelsInBuild}");
        }
        else
        {
            Debug.LogError("Could not find the GameMainDataSO at the specified path... Aborting Process");
        }
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
