using UnityEngine;
using Action = System.Action;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.tag.nut_sort {
    public static class LevelEditorUtility
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        public static void SetDirty(Object target)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(target);
#endif
        }
        public static Object GetPrefabParent(GameObject gameObject)
        {
#if UNITY_EDITOR
            return PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
#else
            return null;
#endif
        }
        public static GameObject SaveAsPrefabAsset(string path, GameObject gameObj)
        {
#if UNITY_EDITOR
            return PrefabUtility.SaveAsPrefabAsset(gameObj, path);
#else
            return null;
#endif
        }
        public static void SaveAssets()
        {
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
#endif
        }
        public static Texture2D GetAssetPreview(Object asset)
        {
#if UNITY_EDITOR
            return AssetPreview.GetAssetPreview(asset);
#else
            return null;
#endif
        }
        public static void ActiveObject(Object obj)
        {
#if UNITY_EDITOR
            Selection.activeObject = obj;
#endif
        }
        public static string GetAssetPath(Object obj)
        {
#if UNITY_EDITOR
            return AssetDatabase.GetAssetPath(obj);
#else
            return "";
#endif
        }

        public static void RegisterEditorApplicationUpdate(ActionBinder actionToCall)
        {
#if UNITY_EDITOR
            EditorApplication.update += actionToCall.InvokeAction;
#endif
        }

        public static void UnregisterEditorApplicationUpdate(ActionBinder actionToCall)
        {
#if UNITY_EDITOR
            EditorApplication.update -= actionToCall.InvokeAction;
#endif
        }

        public static string GetAssetPath(int obj)
        {
#if UNITY_EDITOR
            return AssetDatabase.GetAssetPath(obj);
#else
            return "";
#endif
        }
        public static string GenerateUniqueAssetPath(string path)
        {
#if UNITY_EDITOR
            return AssetDatabase.GenerateUniqueAssetPath(path);
#else
            return "";
#endif
        }
        public static void CreateAsset(Object obj, string path)
        {
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(obj, path);
#endif
        }
        public static bool IsValidFolder(string path)
        {
#if UNITY_EDITOR
            return AssetDatabase.IsValidFolder(path);
#else
            return false;
#endif
        }
        public static string CreateFolder(string parentFolder, string folderName)
        {
#if UNITY_EDITOR
            return AssetDatabase.CreateFolder(parentFolder, folderName);
#else
            return "";
#endif
        }
        public static void RenameAsset(string pathName, string newName)
        {
#if UNITY_EDITOR
            AssetDatabase.RenameAsset(pathName, newName);
#endif
        }
        public static void Refresh()
        {
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
        public static bool DeleteAsset(string path)
        {
#if UNITY_EDITOR
            return AssetDatabase.DeleteAsset(path);
#else
            return false;
#endif
        }
        public static void PingObject(Object obj)
        {
#if UNITY_EDITOR
            EditorGUIUtility.PingObject(obj);
#endif
        }

        public static T LoadAssetAtPath<T>(string path) where T : Object
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<T>(path);
#else
            return null;
#endif
        }

        public static Object GetCorrespondingObjectFromSource(Object componentOrGameObject)
        {
#if UNITY_EDITOR
            return PrefabUtility.GetCorrespondingObjectFromSource(componentOrGameObject);
#else
            return null;
#endif
        }

        public static Object InstantiatePrefab(Object assetComponentOrGameObject)
        {
#if UNITY_EDITOR
            return PrefabUtility.InstantiatePrefab(assetComponentOrGameObject);
#else
            return null;
#endif
        }

        public static GameObject SaveAsPrefabAsset(GameObject instanceRoot, string assetPath)
        {
#if UNITY_EDITOR
            return PrefabUtility.SaveAsPrefabAsset(instanceRoot, assetPath);
#else
            return null;
#endif
        }
#endregion

        #region PRIVATE_FUNCTIONS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }

    public class ActionBinder
    {
        public Action actionToCall;

        public ActionBinder(Action actionToCall)
        {
            this.actionToCall = actionToCall;
        }

        public void InvokeAction()
        {
            actionToCall?.Invoke();
        }
    }
}