using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

namespace Balaso
{
    [CustomEditor(typeof(Balaso.Settings))]
    public class SettingsInspector : Editor
    {
        private static string SETTINGS_ASSET_PATH = "Assets/Balaso Software/Editor/Settings.asset";
        private static Settings settings;
        public static Settings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = (Settings)AssetDatabase.LoadAssetAtPath(SETTINGS_ASSET_PATH, typeof(Balaso.Settings));
                    if (settings == null)
                    {
                        settings = CreateDefaultSettings();
                    }
                }

                return settings;
            }
        }

        private static Settings CreateDefaultSettings()
        {
            Settings asset = ScriptableObject.CreateInstance(typeof(Balaso.Settings)) as Settings;
            AssetDatabase.CreateAsset(asset, SETTINGS_ASSET_PATH);
            asset.PopupMessage = "Your data will only be used to deliver personalized ads to you.";
            return asset;
        }

        [MenuItem("Window/Balaso/App Tracking Transparency/Settings", false, 0)]
        static void SelectSettings()
        {
            Selection.activeObject = Settings;
        }

        public override void OnInspectorGUI()
        {
            settings = target as Balaso.Settings;

            FontStyle fontStyle = EditorStyles.label.fontStyle;
            bool wordWrap = GUI.skin.textField.wordWrap;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            GUI.skin.textField.wordWrap = true;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("App Tracking Transparency", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Present the app-tracking authorization request to the end user with this customizable message", EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PopupMessage"), new GUIContent("Popup Message"));
            DrawHorizontalLine(Color.grey);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("SkAdNetwork", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("SkAdNetworkIds specified will be automatically added to your Info.plist file.", EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("NOTICE: This plugin does not include the ability to show ads.\nYou will need to use your favorite ads platform SDK.", EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Load SkAdNetworkIds from file (xml or json)", GUILayout.Width(300), GUILayout.Height(50)))
            {
                LoadSkAdNetworkIdsFromFile();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("SkAdNetworkIds"), new GUIContent("SkAdNetworkIds"), true);

            serializedObject.ApplyModifiedProperties();
            GUI.skin.textField.wordWrap = wordWrap;
            EditorStyles.label.fontStyle = fontStyle;
        }

        private void LoadSkAdNetworkIdsFromFile()
        {
            SerializedProperty networkIdsSerializedProperty = serializedObject.FindProperty("SkAdNetworkIds");
            string path = EditorUtility.OpenFilePanel("Select SkAdNetworkIds file", "", "txt,json,xml");
            if (path.Length != 0)
            {
                int addedIds = 0;
                string fileContent = File.ReadAllText(path);
                var regex = new Regex(@"[a-z0-9]+\.skadnetwork");
                MatchCollection collection = regex.Matches(fileContent);
                foreach (Match match in collection)
                {
                    string skAdNetworkId = match.Value;
                    bool alreadyAdded = false;
                    int listSize = networkIdsSerializedProperty.arraySize;

                    if (listSize > 0)
                    {
                        for (int i = 0; i < listSize && !alreadyAdded; i++)
                        {
                            if (networkIdsSerializedProperty.GetArrayElementAtIndex(i).stringValue == skAdNetworkId)
                            {
                                alreadyAdded = true;
                            }
                        }
                    }

                    if (!alreadyAdded)
                    {
                        networkIdsSerializedProperty.InsertArrayElementAtIndex(Mathf.Max(0, listSize - 1));
                        networkIdsSerializedProperty.GetArrayElementAtIndex(Mathf.Max(0, listSize - 1)).stringValue = skAdNetworkId;
                        addedIds++;
                    }
                }

                if (addedIds > 0)
                {
                    EditorUtility.DisplayDialog("SkAdNetwork IDs import", string.Format("Successfully added {0} SkAdNetwork IDs", addedIds), "Done");
                }
                else
                {
                    EditorUtility.DisplayDialog("SkAdNetwork IDs import", "No new SkAdNetwork IDs found to be added", "Done");
                }
            }
        }

        private void DrawHorizontalLine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }
    }
}
