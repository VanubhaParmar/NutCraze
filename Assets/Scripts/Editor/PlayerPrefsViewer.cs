using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Microsoft.Win32;
using System.IO;
using System.Xml;
using System;
using System.Collections;
using System.Text;

namespace Tag.NutSort.Editor
{
    public class PlayerPrefsViewer : EditorWindow
    {
        [MenuItem("Window/PlayerPrefs Viewer")]
        static void Init()
        {
            GetWindow(typeof(PlayerPrefsViewer)).titleContent = new GUIContent("PlayerPrefs Viewer");
        }

        private const string UNIQUE_STRING = "0987654321qwertyuiopasdfghjklzxcvbnm[];,.";
        private const int UNIQUE_INT = int.MinValue;
        private const float UNIQUE_FLOAT = Mathf.NegativeInfinity;
        private const int VERT_LIMIT = 65535;
        private const int VERTS_IN_CHARACTER = 4;
        private const int CHARACTER_LIMIT = VERT_LIMIT / VERTS_IN_CHARACTER;
        private static readonly string[] UNITY_HIDDEN_SETTINGS = new string[]
        {
            "UnityGraphicsQuality",
            "unity.cloud_userid",
            "unity.player_session_background_time",
            "unity.player_session_elapsed_time",
            "unity.player_sessionid",
            "unity.player_session_count"
        };
        private const string UNITY_SPECIAL_CHARACTERS = @"$%&|\<>/~";
        private const string UNITY_REPLACEMENT_CHARACTER = "_";
        private const float UpdateIntervalInSeconds = 1.0F;
        private bool waitTillPlistHasBeenWritten = false;
        private FileInfo tmpPlistFile;
        private List<PlayerPrefsEntry> ppeList = new List<PlayerPrefsEntry>();
        private Vector2 scrollPos;
        private string newKey = "";
        private string newValueString = "";
        private int newValueInt = 0;
        private float newValueFloat = 0;
        private float rotation = 0;
        private ValueType selectedType = ValueType.String;
        private bool showNewEntryBox = false;
        private bool isOneSelected = false;
        private bool sortAscending = true;
        private float oldTime = 0;
        private string _searchString = string.Empty;
        private SearchFilterType _searchFilter = SearchFilterType.All;
        private List<PlayerPrefsEntry> filteredPpeList = new List<PlayerPrefsEntry>();


        private bool AutoRefresh
        {
            get { return EditorPrefs.GetBool("APPW-AutoRefresh", false); }
            set { EditorPrefs.SetBool("APPW-AutoRefresh", value); }
        }

        private void OnEnable()
        {
            if (!IsUnityWritingToPlist())
                RefreshKeys();

            //Make sure we never subscribe twice as OnEnable will be called more often then you think :)
#if UNITY_2017_3_OR_NEWER
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#else
			EditorApplication.playmodeStateChanged -= OnPlaymodeStateChanged;
			EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
#endif
        }

#if UNITY_2017_3_OR_NEWER
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            OnPlaymodeStateChanged();
        }
#endif

        private void OnPlaymodeStateChanged()
        {
            waitTillPlistHasBeenWritten = IsUnityWritingToPlist();

            if (!waitTillPlistHasBeenWritten)
                RefreshKeys();
        }

        private void Update()
        {
            //Auto refresh on Windows. On Mac this would be annoying because it takes longer so the user must manually refresh.
            if (AutoRefresh && Application.platform == RuntimePlatform.WindowsEditor
                && EditorApplication.isPlaying)
            {
                float newtime = Mathf.Repeat(Time.timeSinceLevelLoad, UpdateIntervalInSeconds);
                if (newtime < oldTime)
                    RefreshKeys();

                oldTime = newtime;
            }

            if (waitTillPlistHasBeenWritten)
            {
                if (tmpPlistFile != null && new FileInfo(tmpPlistFile.FullName).Exists)
                {
                    //Keep on waiting
                }
                else
                {
                    RefreshKeys();
                    waitTillPlistHasBeenWritten = false;
                }

                rotation += 0.05F;
                Repaint();
            }
            isOneSelected = false;
            foreach (PlayerPrefsEntry item in filteredPpeList)
            {
                if (item.IsSelected)
                {
                    isOneSelected = true;
                    break;
                }
            }
        }

        void OnGUI()
        {
            GUIStyle boldNumberFieldStyle = new GUIStyle(EditorStyles.numberField);
            boldNumberFieldStyle.font = EditorStyles.boldFont;

            GUIStyle boldToggleStyle = new GUIStyle(EditorStyles.toggle);
            boldToggleStyle.font = EditorStyles.boldFont;

            GUI.enabled = !waitTillPlistHasBeenWritten;

            //Toolbar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                Rect optionsRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(false));

                if (GUILayout.Button(new GUIContent("Sort   " + (sortAscending ? "▼" : "▲"), "Change sorting to " + (sortAscending ? "descending" : "ascending")), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                {
                    OnChangeSortModeClicked();
                }

                if (GUILayout.Button(new GUIContent("Options", "Contains additional functionality like \"Add new entry\" and \"Delete all entries\" "), EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(false)))
                {
                    GenericMenu options = new GenericMenu();
                    options.AddItem(new GUIContent("New Entry..."), false, OnNewEntryClicked);
                    options.AddSeparator("");
                    options.AddItem(new GUIContent("Import..."), false, OnImport);
                    options.AddItem(new GUIContent("Export Selected..."), false, OnExportSelected);
                    options.AddItem(new GUIContent("Export All Entries"), false, OnExportAllClicked);
                    options.AddSeparator("");
                    options.AddItem(new GUIContent("Delete Selected Entries"), false, OnDeleteSelectedClicked);
                    options.AddItem(new GUIContent("Delete All Entries"), false, OnDeleteAllClicked);
                    options.DropDown(optionsRect);
                }

                GUILayout.Space(5);

                //Searchfield
                Rect position = GUILayoutUtility.GetRect(50, 250, 10, 50, EditorStyles.toolbarTextField);
                position.width -= 16;
                position.x += 16;
                SearchString = GUI.TextField(position, SearchString, EditorStyles.toolbarTextField);

                position.x = position.x - 18;
                position.width = 20;
                if (GUI.Button(position, "", ToolbarSeachTextFieldPopup))
                {
                    GenericMenu options = new GenericMenu();
                    options.AddItem(new GUIContent("All"), SearchFilter == SearchFilterType.All, OnSearchAllClicked);
                    options.AddItem(new GUIContent("Key"), SearchFilter == SearchFilterType.Key, OnSearchKeyClicked);
                    options.AddItem(new GUIContent("Value (Strings only)"), SearchFilter == SearchFilterType.Value, OnSearchValueClicked);
                    options.DropDown(position);
                }

                position = GUILayoutUtility.GetRect(10, 10, ToolbarSeachCancelButton);
                position.x -= 5;
                if (GUI.Button(position, "", ToolbarSeachCancelButton))
                {
                    SearchString = string.Empty;
                }

                GUILayout.FlexibleSpace();

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    string refreshTooltip = "Should all entries be automaticly refreshed every " + UpdateIntervalInSeconds + " seconds?";
                    AutoRefresh = GUILayout.Toggle(AutoRefresh, new GUIContent("Auto Refresh ", refreshTooltip), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false), GUILayout.MinWidth(75));
                }

                if (GUILayout.Button(new GUIContent("Refresh", "Force a refresh, could take a few seconds."), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                {
                    if (Application.platform == RuntimePlatform.OSXEditor)
                        waitTillPlistHasBeenWritten = IsUnityWritingToPlist();

                    RefreshKeys();
                }

                Rect r;
                if (Application.platform == RuntimePlatform.OSXEditor)
                    r = GUILayoutUtility.GetRect(16, 16);
                else
                    r = GUILayoutUtility.GetRect(9, 16);

                if (waitTillPlistHasBeenWritten)
                {
                    EditorGUI.ProgressBar(new Rect(r.x + 3, r.y, 16, 16), Mathf.Repeat(rotation, 1), "");
                }
            }
            EditorGUILayout.EndHorizontal();

            GUI.enabled = !waitTillPlistHasBeenWritten;

            if (showNewEntryBox)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                {
                    GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                    {
                        newKey = EditorGUILayout.TextField("Key", newKey);

                        switch (selectedType)
                        {
                            default:
                            case ValueType.String:
                                newValueString = EditorGUILayout.TextField("Value", newValueString);
                                break;
                            case ValueType.Float:
                                newValueFloat = EditorGUILayout.FloatField("Value", newValueFloat);
                                break;
                            case ValueType.Integer:
                                newValueInt = EditorGUILayout.IntField("Value", newValueInt);
                                break;
                        }

                        selectedType = (ValueType)EditorGUILayout.EnumPopup("Type", selectedType);
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(GUILayout.Width(1));
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();

                            if (GUILayout.Button(new GUIContent("X", "Close"), EditorStyles.boldLabel, GUILayout.ExpandWidth(false)))
                            {
                                showNewEntryBox = false;
                            }
                        }
                        GUILayout.EndHorizontal();

                        if (GUILayout.Button(new GUIContent("Add", "Add a new key-value.")))
                        {
                            if (!string.IsNullOrEmpty(newKey))
                            {
                                switch (selectedType)
                                {
                                    case ValueType.Integer:
                                        PlayerPrefs.SetInt(newKey, newValueInt);
                                        ppeList.Add(new PlayerPrefsEntry(newKey, newValueInt));
                                        break;
                                    case ValueType.Float:
                                        PlayerPrefs.SetFloat(newKey, newValueFloat);
                                        ppeList.Add(new PlayerPrefsEntry(newKey, newValueFloat));
                                        break;
                                    default:
                                    case ValueType.String:
                                        PlayerPrefs.SetString(newKey, newValueString);
                                        ppeList.Add(new PlayerPrefsEntry(newKey, newValueString));
                                        break;
                                }
                                PlayerPrefs.Save();
                                Sort();
                            }

                            newKey = newValueString = "";
                            newValueInt = 0;
                            newValueFloat = 0;
                            GUIUtility.keyboardControl = 0;
                            showNewEntryBox = false;
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(2);

            GUI.backgroundColor = Color.white;

            EditorGUI.indentLevel++;

            //Show all PlayerPrefs
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            {
                EditorGUILayout.BeginVertical();
                {
                    for (int i = 0; i < filteredPpeList.Count; i++)
                    {
                        if (filteredPpeList[i].Value != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                filteredPpeList[i].IsSelected = GUILayout.Toggle(filteredPpeList[i].IsSelected, new GUIContent("", "Toggle selection."), filteredPpeList[i].HasChanged ? boldToggleStyle : EditorStyles.toggle, GUILayout.MaxWidth(20), GUILayout.MinWidth(20), GUILayout.ExpandWidth(false));
                                filteredPpeList[i].Key = EditorGUILayout.TextField(filteredPpeList[i].Key, filteredPpeList[i].HasChanged ? boldNumberFieldStyle : EditorStyles.numberField, GUILayout.MaxWidth(125), GUILayout.MinWidth(40), GUILayout.ExpandWidth(true));

                                GUIStyle numberFieldStyle = filteredPpeList[i].HasChanged ? boldNumberFieldStyle : EditorStyles.numberField;
                                switch (filteredPpeList[i].Type)
                                {
                                    default:
                                    case ValueType.String:
                                        string textValue = (string)filteredPpeList[i].Value;
                                        if (textValue.Length < CHARACTER_LIMIT)
                                        {
                                            filteredPpeList[i].Value = EditorGUILayout.TextField("", textValue, numberFieldStyle, GUILayout.MinWidth(40));
                                        }
                                        else
                                        {
                                            GUI.backgroundColor = Color.red;
                                            EditorGUILayout.TextField("", string.Format("Cannot display value as it exceeds the textfield character limitation. Value has {0} characters while Unity only supports {1}.", textValue.Length, CHARACTER_LIMIT), EditorStyles.numberField, GUILayout.MinWidth(40));
                                            GUI.backgroundColor = Color.white;
                                        }
                                        break;

                                    case ValueType.Float:
                                        filteredPpeList[i].Value = EditorGUILayout.FloatField("", (float)filteredPpeList[i].Value, numberFieldStyle, GUILayout.MinWidth(40));
                                        break;

                                    case ValueType.Integer:
                                        filteredPpeList[i].Value = EditorGUILayout.IntField("", (int)filteredPpeList[i].Value, numberFieldStyle, GUILayout.MinWidth(40));
                                        break;
                                }

                                GUILayout.Label(new GUIContent("?", filteredPpeList[i].Type.ToString()), GUILayout.ExpandWidth(false));

                                GUI.enabled = filteredPpeList[i].HasChanged && !waitTillPlistHasBeenWritten;
                                if (GUILayout.Button(new GUIContent("Save", "Save changes made to this value."), GUILayout.ExpandWidth(false)))
                                {
                                    filteredPpeList[i].SaveChanges();
                                    EditorGUI.FocusTextInControl(string.Empty);
                                }

                                if (GUILayout.Button(new GUIContent("Undo", "Discard changes made to this value."), GUILayout.ExpandWidth(false)))
                                {
                                    filteredPpeList[i].RevertChanges();
                                    EditorGUI.FocusTextInControl(string.Empty);
                                }

                                GUI.enabled = !waitTillPlistHasBeenWritten;

                                if (GUILayout.Button(new GUIContent("Delete", "Delete this key-value."), GUILayout.ExpandWidth(false)))
                                {
                                    PlayerPrefs.DeleteKey(filteredPpeList[i].Key);
                                    ppeList.Remove(filteredPpeList[i]);
                                    PlayerPrefs.Save();

                                    UpdateFilteredList();
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
            EditorGUI.indentLevel--;
        }

        #region Menu Actions

        private void OnChangeSortModeClicked()
        {
            sortAscending = !sortAscending;
            Sort();
        }

        private void OnNewEntryClicked()
        {
            showNewEntryBox = true;
        }

        private void OnImport()
        {
            string importPath = EditorUtility.OpenFilePanel("Import PlayerPrefs", "", "ppe");
            if (!string.IsNullOrEmpty(importPath))
            {
                FileInfo fi = new FileInfo(importPath);
                Dictionary<string, object> plist = (Dictionary<string, object>)Plist.readPlist(fi.FullName);

                foreach (KeyValuePair<string, object> kvp in plist)
                {
                    PlayerPrefsEntry entry = null;

                    if (kvp.Value is float)
                        entry = new PlayerPrefsEntry(kvp.Key, (float)kvp.Value);
                    else if (kvp.Value is int)
                        entry = new PlayerPrefsEntry(kvp.Key, (int)kvp.Value);
                    else if (kvp.Value is string)
                        entry = new PlayerPrefsEntry(kvp.Key, (string)kvp.Value);

                    if (entry != null)
                    {
                        PlayerPrefsEntry existingEntry = ppeList.Find(p => p.Key == entry.Key);
                        if (existingEntry == null)
                        {
                            ppeList.Add(entry);
                            entry.SaveChanges();
                        }
                        else if (!entry.Value.Equals(existingEntry.Value) && EditorUtility.DisplayDialog("Duplicate entry found", string.Format("Key \"{0}\" with value \"{1}\" already exists.{2}Do you want to overwrite it with value \"{3}\" ?", existingEntry.Key, existingEntry.Value, Environment.NewLine, entry.Value), "Yes", "No"))
                        {
                            ppeList.Remove(existingEntry);
                            ppeList.Add(entry);
                            entry.SaveChanges();
                        }
                    }
                }

                Sort();
                Repaint();
                EditorGUI.FocusTextInControl(string.Empty);
            }
        }

        private void OnExportSelected()
        {
            Export(true);
        }

        private void OnExportAllClicked()
        {
            Export(false);
        }

        private void OnDeleteSelectedClicked()
        {
            if (isOneSelected)
            {
                if (!waitTillPlistHasBeenWritten)
                {
                    if (EditorUtility.DisplayDialog("Are you sure?", "Are you sure you want to delete the selected keys? There is no undo!", "Delete", "Cancel"))
                    {
                        int count = filteredPpeList.Count - 1;
                        for (int i = count; i >= 0; i--)
                        {
                            if (filteredPpeList[i].IsSelected)
                            {
                                PlayerPrefs.DeleteKey(filteredPpeList[i].Key);
                                ppeList.Remove(filteredPpeList[i]);
                            }
                        }

                        PlayerPrefs.Save();
                        UpdateFilteredList();
                    }
                }
                else
                    Debug.LogError("Cannot delete PlayerPrefs entries because it is still loading.");
            }
            else
                Debug.LogError("Cannot delete PlayerPrefs entries because no entries has been selected.");
        }

        private void OnDeleteAllClicked()
        {
            for (int i = 0; i < ppeList.Count; i++)
                ppeList[i].IsSelected = true;
            isOneSelected = true;
            OnDeleteSelectedClicked();
        }

        private void Export(bool onlySelected)
        {
            Dictionary<string, object> entries = new Dictionary<string, object>();
            for (int i = 0; i < filteredPpeList.Count; i++)
            {
                PlayerPrefsEntry entry = filteredPpeList[i];
                if (!entries.ContainsKey(entry.Key))
                {
                    if (onlySelected == false)
                        entries.Add(entry.Key, entry.Value);
                    else if (entry.IsSelected)
                        entries.Add(entry.Key, entry.Value);
                }
            }

            if (onlySelected && entries.Count == 0)
                Debug.LogError("Cannot export selected entries as no entries has been selected.");
            else
            {
                string exportPath = EditorUtility.SaveFilePanel("Export all PlayPrefs entries", Application.dataPath, PlayerSettings.productName + "_PlayerPrefs", "ppe");

                if (!string.IsNullOrEmpty(exportPath))
                {
                    string xml = Plist.writeXml(entries);
                    File.WriteAllText(exportPath, xml);
                    AssetDatabase.Refresh();
                }
            }
        }

        #endregion

        #region SearchMenu Actions

        private void OnSearchAllClicked()
        {
            SearchFilter = SearchFilterType.All;
        }

        private void OnSearchKeyClicked()
        {
            SearchFilter = SearchFilterType.Key;
        }

        private void OnSearchValueClicked()
        {
            SearchFilter = SearchFilterType.Value;
        }

        private void UpdateFilteredList()
        {
            filteredPpeList.Clear();

            if (!string.IsNullOrEmpty(SearchString))
            {
                for (int i = 0; i < ppeList.Count; i++)
                {
                    if (SearchFilter == SearchFilterType.Key || SearchFilter == SearchFilterType.All)
                    {
                        if (ppeList[i].Key.ToLowerInvariant().Contains(SearchString.Trim().ToLowerInvariant()))
                            filteredPpeList.Add(ppeList[i]);
                    }

                    if ((SearchFilter == SearchFilterType.Value || SearchFilter == SearchFilterType.All) && ppeList[i].Type == ValueType.String)
                    {
                        if (!filteredPpeList.Contains(ppeList[i]))
                        {
                            if (((string)ppeList[i].Value).ToLowerInvariant().Contains(SearchString.Trim().ToLowerInvariant()))
                                filteredPpeList.Add(ppeList[i]);
                        }
                    }
                }
            }
            else
                filteredPpeList.AddRange(ppeList);
        }

        #endregion

        private void Sort()
        {
            if (sortAscending)
                ppeList.Sort(PlayerPrefsEntry.SortByNameAscending);
            else
                ppeList.Sort(PlayerPrefsEntry.SortByNameDescending);

            UpdateFilteredList();
        }

        private void RefreshKeys()
        {
            ppeList.Clear();
            string[] allKeys = GetAllKeys();

            for (int i = 0; i < allKeys.Length; i++)
                ppeList.Add(new PlayerPrefsEntry(allKeys[i]));
            Sort();
            Repaint();
        }

        private string[] GetAllKeys()
        {
            List<string> result = new List<string>();

            if (Application.platform == RuntimePlatform.WindowsEditor)
                result.AddRange(GetAllWindowsKeys());
            else if (Application.platform == RuntimePlatform.OSXEditor)
                result.AddRange(GetAllMacKeys());
#if UNITY_EDITOR_LINUX
			else if (Application.platform == RuntimePlatform.LinuxEditor) 
				result.AddRange(GetAllLinuxKeys());
#endif
            else
                Debug.LogError("Unsupported platform detected, please contact support@rejected-games.com and let us know.");

            //Remove something Unity sometimes saves in your PlayerPrefs
            for (int i = 0; i < UNITY_HIDDEN_SETTINGS.Length; i++)
            {
                if (result.Contains(UNITY_HIDDEN_SETTINGS[i]))
                    result.Remove(UNITY_HIDDEN_SETTINGS[i]);
            }

            return result.ToArray();
        }

        private string[] GetAllMacKeys()
        {
            string companyName = ReplaceSpecialCharacters(PlayerSettings.companyName);
            string productName = ReplaceSpecialCharacters(PlayerSettings.productName);
            string plistPath = string.Format("{0}/Library/Preferences/unity.{1}.{2}.plist", Environment.GetFolderPath(Environment.SpecialFolder.Personal), companyName, productName);
            string[] keys = new string[0];

            if (File.Exists(plistPath))
            {
                FileInfo fi = new FileInfo(plistPath);
                Dictionary<string, object> plist = (Dictionary<string, object>)Plist.readPlist(fi.FullName);

                keys = new string[plist.Count];
                plist.Keys.CopyTo(keys, 0);
            }

            return keys;
        }

        private string[] GetAllWindowsKeys()
        {
            RegistryKey cuKey = Registry.CurrentUser;
            RegistryKey unityKey;

            //The default location of PlayerPrefs pre Unity 5_5
#if UNITY_5_5_OR_NEWER
            unityKey = cuKey.CreateSubKey("Software\\Unity\\UnityEditor\\" + PlayerSettings.companyName + "\\" + PlayerSettings.productName);
#else
			unityKey = cuKey.CreateSubKey("Software\\" + PlayerSettings.companyName + "\\" + PlayerSettings.productName);

			if (unityKey.GetValueNames().Length == 0)
			{
				//On some machines (Windows 7 & 8 64bit using Unity5.4) PlayersPrefs are saved in HKEY_CURRENT_USER\SOFTWARE\AppDataLow\Software\CompanyName\ProjectName weird enough...
				unityKey = cuKey.CreateSubKey("Software\\AppDataLow\\Software\\" + PlayerSettings.companyName + "\\" + PlayerSettings.productName);
			}
#endif

            string[] values = unityKey.GetValueNames();
            for (int i = 0; i < values.Length; i++)
                values[i] = values[i].Substring(0, values[i].LastIndexOf("_"));
            return values;
        }

        private string[] GetAllLinuxKeys()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/.config/unity3d/" + PlayerSettings.companyName + "/" + PlayerSettings.productName + "/prefs";
            List<string> keys = new List<string>();

            XmlDocument xmlDoc = new XmlDocument();
            if (System.IO.File.Exists(path))
                xmlDoc.LoadXml(System.IO.File.ReadAllText(path));
            foreach (XmlElement node in xmlDoc.SelectNodes("unity_prefs/pref"))
                keys.Add(node.GetAttribute("name"));
            return keys.ToArray();
        }

        private string ReplaceSpecialCharacters(string str)
        {
            for (int i = 0; i < UNITY_SPECIAL_CHARACTERS.Length; i++)
            {
                string specialCharacter = UNITY_SPECIAL_CHARACTERS.Substring(i, 1);
                if (str.Contains(specialCharacter))
                    str = str.Replace(specialCharacter, UNITY_REPLACEMENT_CHARACTER);
            }
            return str;
        }

        private bool IsUnityWritingToPlist()
        {
            bool result = false;

            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                string preferencesPath = string.Format("{0}/Library/Preferences/", Environment.GetFolderPath(Environment.SpecialFolder.Personal));
                FileInfo plistFile = new FileInfo(string.Format("{0}unity.{1}.{2}.plist", preferencesPath, PlayerSettings.companyName, PlayerSettings.productName));
                DirectoryInfo di = new DirectoryInfo(preferencesPath);
                FileInfo[] allPreferenceFiles = di.GetFiles();

                foreach (FileInfo fi in allPreferenceFiles)
                {
                    if (fi.FullName.Contains(plistFile.FullName))
                    {
                        if (!fi.FullName.EndsWith(".plist"))
                        {
                            tmpPlistFile = fi;
                            result = true;
                        }
                    }
                }
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
                result = false;

            return result;
        }

        private string SearchString
        {
            get { return _searchString; }
            set
            {
                if (_searchString != value)
                {
                    _searchString = value;
                    UpdateFilteredList();
                }
            }
        }

        private SearchFilterType SearchFilter
        {
            get { return _searchFilter; }
            set
            {
                if (_searchFilter != value)
                {
                    _searchFilter = value;
                    UpdateFilteredList();
                }
            }
        }

        private class PlayerPrefsEntry
        {
            private string key;
            private object value;

            public ValueType Type;
            public bool IsSelected = false;
            public bool HasChanged = false;

            private string oldKey;

            public PlayerPrefsEntry(string key)
            {
                this.key = key;
                oldKey = key;
                RetrieveValue();
            }

            public PlayerPrefsEntry(string key, string value)
            {
                this.key = key;
                this.value = value;
                this.Type = ValueType.String;
            }

            public PlayerPrefsEntry(string key, float value)
            {
                this.key = key;
                this.value = value;
                this.Type = ValueType.Float;
            }

            public PlayerPrefsEntry(string key, int value)
            {
                this.key = key;
                this.value = value;
                this.Type = ValueType.Integer;
            }

            public void SaveChanges()
            {
                switch (Type)
                {
                    default:
                    case ValueType.String:
                        PlayerPrefs.SetString(Key, (string)Value);
                        break;
                    case ValueType.Float:
                        PlayerPrefs.SetFloat(Key, (float)Value);
                        break;
                    case ValueType.Integer:
                        PlayerPrefs.SetInt(Key, (int)Value);
                        break;
                }

                if (oldKey != Key)
                {
                    PlayerPrefs.DeleteKey(oldKey);
                    oldKey = Key;
                }

                HasChanged = false;
                PlayerPrefs.Save();
            }

            public void RevertChanges()
            {
                RetrieveValue();
            }

            public void RetrieveValue()
            {
                Key = oldKey;

                if (PlayerPrefs.GetString(Key, UNIQUE_STRING) != UNIQUE_STRING)
                {
                    Type = ValueType.String;
                    value = PlayerPrefs.GetString(Key);
                }
                else if (PlayerPrefs.GetInt(Key, UNIQUE_INT) != UNIQUE_INT)
                {
                    Type = ValueType.Integer;
                    value = PlayerPrefs.GetInt(Key);
                }
                else if (PlayerPrefs.GetFloat(Key, UNIQUE_FLOAT) != UNIQUE_FLOAT)
                {
                    Type = ValueType.Float;
                    value = PlayerPrefs.GetFloat(Key);
                }
                oldKey = Key;
                HasChanged = false;
            }

            public string Key
            {
                get
                {
                    return key;
                }

                set
                {
                    if (value != key)
                    {
                        HasChanged = true;
                        key = value;
                    }
                }
            }

            public object Value
            {
                get
                {
                    return this.value;
                }

                set
                {
                    if (!value.Equals(this.value))
                    {
                        this.value = value;

                        HasChanged = true;
                    }
                }
            }

            public static int SortByNameAscending(PlayerPrefsEntry a, PlayerPrefsEntry b)
            {
                return string.Compare(a.Key, b.Key);
            }

            public static int SortByNameDescending(PlayerPrefsEntry a, PlayerPrefsEntry b)
            {
                return string.Compare(b.Key, a.Key);
            }
        }

        private enum ValueType
        {
            String,
            Float,
            Integer
        }

        private enum SearchFilterType
        {
            All,
            Key,
            Value
        }

        public static void Separator(Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            Rect lineRect = GUILayoutUtility.GetRect(10, 1);
            GUI.DrawTexture(new Rect(lineRect.x, lineRect.y, lineRect.width, 1), EditorGUIUtility.whiteTexture);
            GUI.color = old;
        }
        private GUIStyle ToolbarSearchField { get { return GetStyle("ToolbarSearchTextField", "ToolbarSeachTextField"); } }
        private GUIStyle ToolbarSeachTextFieldPopup { get { return GetStyle("ToolbarSearchTextFieldPopup", "ToolbarSeachTextFieldPopup"); } }
        private GUIStyle ToolbarSeachCancelButton { get { return GetStyle("ToolbarSearchCancelButton", "ToolbarSeachCancelButton"); } }
        private GUIStyle ToolbarSeachCancelButtonEmpty { get { return GetStyle("ToolbarSearchCancelButtonEmpty", "ToolbarSeachCancelButtonEmpty"); } }

        private GUIStyle GetStyle(string styleName, string fallbackStyle)
        {
            GUIStyle guiStyle = GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);

            if (guiStyle == null)
                guiStyle = GUI.skin.FindStyle(fallbackStyle) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(fallbackStyle);

            if (guiStyle == null)
                guiStyle = GUI.skin.button;
            return guiStyle;
        }
    }
    public static class Plist
    {
        private static List<int> offsetTable = new List<int>();
        private static List<byte> objectTable = new List<byte>();
        private static int refCount;
        private static int objRefSize;
        private static int offsetByteSize;
        private static long offsetTableOffset;

        #region Public Functions

        public static object readPlist(string path)
        {
            using (FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return readPlist(f, plistType.Auto);
            }
        }

        public static object readPlistSource(string source)
        {
            return readPlist(System.Text.Encoding.UTF8.GetBytes(source));
        }

        public static object readPlist(byte[] data)
        {
            return readPlist(new MemoryStream(data), plistType.Auto);
        }

        public static plistType getPlistType(Stream stream)
        {
            byte[] magicHeader = new byte[8];
            stream.Read(magicHeader, 0, 8);

            if (BitConverter.ToInt64(magicHeader, 0) == 3472403351741427810)
                return plistType.Binary;
            else
                return plistType.Xml;
        }

        public static object readPlist(Stream stream, plistType type)
        {
            if (type == plistType.Auto)
            {
                type = getPlistType(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }

            if (type == plistType.Binary)
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    byte[] data = reader.ReadBytes((int)reader.BaseStream.Length);
                    return readBinary(data);
                }
            }
            else
            {
                XmlDocument xml = new XmlDocument();
                xml.XmlResolver = null;
                xml.Load(stream);
                return readXml(xml);
            }
        }

        public static void writeXml(object value, string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(writeXml(value));
            }
        }

        public static void writeXml(object value, Stream stream)
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(writeXml(value));
            }
        }

        public static string writeXml(object value)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Encoding = new System.Text.UTF8Encoding(false);
                xmlWriterSettings.ConformanceLevel = ConformanceLevel.Document;
                xmlWriterSettings.Indent = true;

                using (XmlWriter xmlWriter = XmlWriter.Create(ms, xmlWriterSettings))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteDocType("plist", "-//Apple Computer//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null);
                    xmlWriter.WriteStartElement("plist");
                    xmlWriter.WriteAttributeString("version", "1.0");
                    compose(value, xmlWriter);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();
                    xmlWriter.Flush();
                    xmlWriter.Close();
                    return System.Text.Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }

        public static void writeBinary(object value, string path)
        {
            using (BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.Create)))
            {
                writer.Write(writeBinary(value));
            }
        }

        public static void writeBinary(object value, Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(writeBinary(value));
            }
        }

        public static byte[] writeBinary(object value)
        {
            offsetTable.Clear();
            objectTable.Clear();
            refCount = 0;
            objRefSize = 0;
            offsetByteSize = 0;
            offsetTableOffset = 0;
            int totalRefs = countObject(value) - 1;
            refCount = totalRefs;
            objRefSize = RegulateNullBytes(BitConverter.GetBytes(refCount)).Length;
            composeBinary(value);
            writeBinaryString("bplist00", false);
            offsetTableOffset = (long)objectTable.Count;
            offsetTable.Add(objectTable.Count - 8);
            offsetByteSize = RegulateNullBytes(BitConverter.GetBytes(offsetTable[offsetTable.Count - 1])).Length;
            List<byte> offsetBytes = new List<byte>();
            offsetTable.Reverse();

            for (int i = 0; i < offsetTable.Count; i++)
            {
                offsetTable[i] = objectTable.Count - offsetTable[i];
                byte[] buffer = RegulateNullBytes(BitConverter.GetBytes(offsetTable[i]), offsetByteSize);
                Array.Reverse(buffer);
                offsetBytes.AddRange(buffer);
            }
            objectTable.AddRange(offsetBytes);
            objectTable.AddRange(new byte[6]);
            objectTable.Add(Convert.ToByte(offsetByteSize));
            objectTable.Add(Convert.ToByte(objRefSize));
            var a = BitConverter.GetBytes((long)totalRefs + 1);
            Array.Reverse(a);
            objectTable.AddRange(a);
            objectTable.AddRange(BitConverter.GetBytes((long)0));
            a = BitConverter.GetBytes(offsetTableOffset);
            Array.Reverse(a);
            objectTable.AddRange(a);
            return objectTable.ToArray();
        }

        #endregion

        #region Private Functions

        private static object readXml(XmlDocument xml)
        {
            XmlNode rootNode = xml.DocumentElement.ChildNodes[0];
            return (Dictionary<string, object>)parse(rootNode);
        }

        private static object readBinary(byte[] data)
        {
            offsetTable.Clear();
            List<byte> offsetTableBytes = new List<byte>();
            objectTable.Clear();
            refCount = 0;
            objRefSize = 0;
            offsetByteSize = 0;
            offsetTableOffset = 0;

            List<byte> bList = new List<byte>(data);

            List<byte> trailer = bList.GetRange(bList.Count - 32, 32);

            parseTrailer(trailer);

            objectTable = bList.GetRange(0, (int)offsetTableOffset);

            offsetTableBytes = bList.GetRange((int)offsetTableOffset, bList.Count - (int)offsetTableOffset - 32);

            parseOffsetTable(offsetTableBytes);

            return parseBinary(0);
        }

        private static Dictionary<string, object> parseDictionary(XmlNode node)
        {
            XmlNodeList children = node.ChildNodes;
            if (children.Count % 2 != 0)
            {
                throw new DataMisalignedException("Dictionary elements must have an even number of child nodes");
            }

            Dictionary<string, object> dict = new Dictionary<string, object>();

            for (int i = 0; i < children.Count; i += 2)
            {
                XmlNode keynode = children[i];
                XmlNode valnode = children[i + 1];

                if (keynode.Name != "key")
                {
                    throw new ApplicationException("expected a key node");
                }

                object result = parse(valnode);

                if (result != null)
                {
                    dict.Add(keynode.InnerText, result);
                }
            }

            return dict;
        }

        private static List<object> parseArray(XmlNode node)
        {
            List<object> array = new List<object>();

            foreach (XmlNode child in node.ChildNodes)
            {
                object result = parse(child);
                if (result != null)
                {
                    array.Add(result);
                }
            }

            return array;
        }

        private static void composeArray(List<object> value, XmlWriter writer)
        {
            writer.WriteStartElement("array");
            foreach (object obj in value)
            {
                compose(obj, writer);
            }
            writer.WriteEndElement();
        }

        private static object parse(XmlNode node)
        {
            switch (node.Name)
            {
                case "dict":
                    return parseDictionary(node);
                case "array":
                    return parseArray(node);
                case "string":
                    return node.InnerText;
                case "integer":
                    return Convert.ToInt32(node.InnerText, System.Globalization.NumberFormatInfo.InvariantInfo);
                case "float":
                    return float.Parse(node.InnerText, System.Globalization.NumberFormatInfo.InvariantInfo);
                case "false":
                    return false;
                case "true":
                    return true;
                case "null":
                    return null;
                case "date":
                    return XmlConvert.ToDateTime(node.InnerText, XmlDateTimeSerializationMode.Utc);
                case "data":
                    return Convert.FromBase64String(node.InnerText);
            }

            throw new ApplicationException(String.Format("Plist Node `{0}' is not supported", node.Name));
        }

        private static void compose(object value, XmlWriter writer)
        {

            if (value == null || value is string)
            {
                writer.WriteElementString("string", value as string);
            }
            else if (value is int || value is long)
            {
                writer.WriteElementString("integer", ((int)value).ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            }
            else if (value is System.Collections.Generic.Dictionary<string, object> ||
              value.GetType().ToString().StartsWith("System.Collections.Generic.Dictionary`2[System.String"))
            {
                Dictionary<string, object> dic = value as Dictionary<string, object>;
                if (dic == null)
                {
                    dic = new Dictionary<string, object>();
                    IDictionary idic = (IDictionary)value;
                    foreach (var key in idic.Keys)
                    {
                        dic.Add(key.ToString(), idic[key]);
                    }
                }
                writeDictionaryValues(dic, writer);
            }
            else if (value is List<object>)
            {
                composeArray((List<object>)value, writer);
            }
            else if (value is byte[])
            {
                writer.WriteElementString("data", Convert.ToBase64String((Byte[])value));
            }
            else if (value is float || value is double)
            {
                writer.WriteElementString("float", ((float)value).ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            }
            else if (value is DateTime)
            {
                DateTime time = (DateTime)value;
                string theString = XmlConvert.ToString(time, XmlDateTimeSerializationMode.Utc);
                writer.WriteElementString("date", theString);//, "yyyy-MM-ddTHH:mm:ssZ"));
            }
            else if (value is bool)
            {
                writer.WriteElementString(value.ToString().ToLower(), "");
            }
            else
            {
                throw new Exception(String.Format("Value type '{0}' is unhandled", value.GetType().ToString()));
            }
        }

        private static void writeDictionaryValues(Dictionary<string, object> dictionary, XmlWriter writer)
        {
            writer.WriteStartElement("dict");
            foreach (string key in dictionary.Keys)
            {
                object value = dictionary[key];
                writer.WriteElementString("key", key);
                compose(value, writer);
            }
            writer.WriteEndElement();
        }

        private static int countObject(object value)
        {
            int count = 0;
            switch (value.GetType().ToString())
            {
                case "System.Collections.Generic.Dictionary`2[System.String,System.Object]":
                    Dictionary<string, object> dict = (Dictionary<string, object>)value;
                    foreach (string key in dict.Keys)
                    {
                        count += countObject(dict[key]);
                    }
                    count += dict.Keys.Count;
                    count++;
                    break;
                case "System.Collections.Generic.List`1[System.Object]":
                    List<object> list = (List<object>)value;
                    foreach (object obj in list)
                    {
                        count += countObject(obj);
                    }
                    count++;
                    break;
                default:
                    count++;
                    break;
            }
            return count;
        }

        private static byte[] writeBinaryDictionary(Dictionary<string, object> dictionary)
        {
            List<byte> buffer = new List<byte>();
            List<byte> header = new List<byte>();
            List<int> refs = new List<int>();
            for (int i = dictionary.Count - 1; i >= 0; i--)
            {
                var o = new object[dictionary.Count];
                dictionary.Values.CopyTo(o, 0);
                composeBinary(o[i]);
                offsetTable.Add(objectTable.Count);
                refs.Add(refCount);
                refCount--;
            }
            for (int i = dictionary.Count - 1; i >= 0; i--)
            {
                var o = new string[dictionary.Count];
                dictionary.Keys.CopyTo(o, 0);
                composeBinary(o[i]);//);
                offsetTable.Add(objectTable.Count);
                refs.Add(refCount);
                refCount--;
            }

            if (dictionary.Count < 15)
            {
                header.Add(Convert.ToByte(0xD0 | Convert.ToByte(dictionary.Count)));
            }
            else
            {
                header.Add(0xD0 | 0xf);
                header.AddRange(writeBinaryInteger(dictionary.Count, false));
            }


            foreach (int val in refs)
            {
                byte[] refBuffer = RegulateNullBytes(BitConverter.GetBytes(val), objRefSize);
                Array.Reverse(refBuffer);
                buffer.InsertRange(0, refBuffer);
            }

            buffer.InsertRange(0, header);


            objectTable.InsertRange(0, buffer);

            return buffer.ToArray();
        }

        private static byte[] composeBinaryArray(List<object> objects)
        {
            List<byte> buffer = new List<byte>();
            List<byte> header = new List<byte>();
            List<int> refs = new List<int>();

            for (int i = objects.Count - 1; i >= 0; i--)
            {
                composeBinary(objects[i]);
                offsetTable.Add(objectTable.Count);
                refs.Add(refCount);
                refCount--;
            }

            if (objects.Count < 15)
            {
                header.Add(Convert.ToByte(0xA0 | Convert.ToByte(objects.Count)));
            }
            else
            {
                header.Add(0xA0 | 0xf);
                header.AddRange(writeBinaryInteger(objects.Count, false));
            }

            foreach (int val in refs)
            {
                byte[] refBuffer = RegulateNullBytes(BitConverter.GetBytes(val), objRefSize);
                Array.Reverse(refBuffer);
                buffer.InsertRange(0, refBuffer);
            }

            buffer.InsertRange(0, header);

            objectTable.InsertRange(0, buffer);

            return buffer.ToArray();
        }

        private static byte[] composeBinary(object obj)
        {
            byte[] value;
            switch (obj.GetType().ToString())
            {
                case "System.Collections.Generic.Dictionary`2[System.String,System.Object]":
                    value = writeBinaryDictionary((Dictionary<string, object>)obj);
                    return value;

                case "System.Collections.Generic.List`1[System.Object]":
                    value = composeBinaryArray((List<object>)obj);
                    return value;

                case "System.Byte[]":
                    value = writeBinaryByteArray((byte[])obj);
                    return value;

                case "System.Double":
                    value = writeBinaryDouble((double)obj);
                    return value;

                case "System.Int32":
                    value = writeBinaryInteger((int)obj, true);
                    return value;

                case "System.String":
                    value = writeBinaryString((string)obj, true);
                    return value;

                case "System.DateTime":
                    value = writeBinaryDate((DateTime)obj);
                    return value;

                case "System.Boolean":
                    value = writeBinaryBool((bool)obj);
                    return value;

                default:
                    return new byte[0];
            }
        }

        public static byte[] writeBinaryDate(DateTime obj)
        {
            List<byte> buffer = new List<byte>(RegulateNullBytes(BitConverter.GetBytes(PlistDateConverter.ConvertToAppleTimeStamp(obj)), 8));
            buffer.Reverse();
            buffer.Insert(0, 0x33);
            objectTable.InsertRange(0, buffer);
            return buffer.ToArray();
        }

        public static byte[] writeBinaryBool(bool obj)
        {
            List<byte> buffer = new List<byte>(new byte[1] { (bool)obj ? (byte)9 : (byte)8 });
            objectTable.InsertRange(0, buffer);
            return buffer.ToArray();
        }

        private static byte[] writeBinaryInteger(int value, bool write)
        {
            List<byte> buffer = new List<byte>(BitConverter.GetBytes((long)value));
            buffer = new List<byte>(RegulateNullBytes(buffer.ToArray()));
            while (buffer.Count != Math.Pow(2, Math.Log(buffer.Count) / Math.Log(2)))
                buffer.Add(0);
            int header = 0x10 | (int)(Math.Log(buffer.Count) / Math.Log(2));

            buffer.Reverse();

            buffer.Insert(0, Convert.ToByte(header));

            if (write)
                objectTable.InsertRange(0, buffer);

            return buffer.ToArray();
        }

        private static byte[] writeBinaryDouble(double value)
        {
            List<byte> buffer = new List<byte>(RegulateNullBytes(BitConverter.GetBytes(value), 4));
            while (buffer.Count != Math.Pow(2, Math.Log(buffer.Count) / Math.Log(2)))
                buffer.Add(0);
            int header = 0x20 | (int)(Math.Log(buffer.Count) / Math.Log(2));
            buffer.Reverse();
            buffer.Insert(0, Convert.ToByte(header));
            objectTable.InsertRange(0, buffer);
            return buffer.ToArray();
        }

        private static byte[] writeBinaryByteArray(byte[] value)
        {
            List<byte> buffer = new List<byte>(value);
            List<byte> header = new List<byte>();
            if (value.Length < 15)
            {
                header.Add(Convert.ToByte(0x40 | Convert.ToByte(value.Length)));
            }
            else
            {
                header.Add(0x40 | 0xf);
                header.AddRange(writeBinaryInteger(buffer.Count, false));
            }

            buffer.InsertRange(0, header);

            objectTable.InsertRange(0, buffer);

            return buffer.ToArray();
        }

        private static byte[] writeBinaryString(string value, bool head)
        {
            List<byte> buffer = new List<byte>();
            List<byte> header = new List<byte>();
            foreach (char chr in value.ToCharArray())
                buffer.Add(Convert.ToByte(chr));

            if (head)
            {
                if (value.Length < 15)
                {
                    header.Add(Convert.ToByte(0x50 | Convert.ToByte(value.Length)));
                }
                else
                {
                    header.Add(0x50 | 0xf);
                    header.AddRange(writeBinaryInteger(buffer.Count, false));
                }
            }

            buffer.InsertRange(0, header);

            objectTable.InsertRange(0, buffer);

            return buffer.ToArray();
        }

        private static byte[] RegulateNullBytes(byte[] value)
        {
            return RegulateNullBytes(value, 1);
        }

        private static byte[] RegulateNullBytes(byte[] value, int minBytes)
        {
            Array.Reverse(value);
            List<byte> bytes = new List<byte>(value);
            for (int i = 0; i < bytes.Count; i++)
            {
                if (bytes[i] == 0 && bytes.Count > minBytes)
                {
                    bytes.Remove(bytes[i]);
                    i--;
                }
                else
                    break;
            }

            if (bytes.Count < minBytes)
            {
                int dist = minBytes - bytes.Count;
                for (int i = 0; i < dist; i++)
                    bytes.Insert(0, 0);
            }

            value = bytes.ToArray();
            Array.Reverse(value);
            return value;
        }

        private static void parseTrailer(List<byte> trailer)
        {
            offsetByteSize = BitConverter.ToInt32(RegulateNullBytes(trailer.GetRange(6, 1).ToArray(), 4), 0);
            objRefSize = BitConverter.ToInt32(RegulateNullBytes(trailer.GetRange(7, 1).ToArray(), 4), 0);
            byte[] refCountBytes = trailer.GetRange(12, 4).ToArray();
            Array.Reverse(refCountBytes);
            refCount = BitConverter.ToInt32(refCountBytes, 0);
            byte[] offsetTableOffsetBytes = trailer.GetRange(24, 8).ToArray();
            Array.Reverse(offsetTableOffsetBytes);
            offsetTableOffset = BitConverter.ToInt64(offsetTableOffsetBytes, 0);
        }

        private static void parseOffsetTable(List<byte> offsetTableBytes)
        {
            for (int i = 0; i < offsetTableBytes.Count; i += offsetByteSize)
            {
                byte[] buffer = offsetTableBytes.GetRange(i, offsetByteSize).ToArray();
                Array.Reverse(buffer);
                offsetTable.Add(BitConverter.ToInt32(RegulateNullBytes(buffer, 4), 0));
            }
        }

        private static object parseBinaryDictionary(int objRef)
        {
            Dictionary<string, object> buffer = new Dictionary<string, object>();
            List<int> refs = new List<int>();
            int refCount = 0;

            int refStartPosition;
            refCount = getCount(offsetTable[objRef], out refStartPosition);


            if (refCount < 15)
                refStartPosition = offsetTable[objRef] + 1;
            else
                refStartPosition = offsetTable[objRef] + 2 + RegulateNullBytes(BitConverter.GetBytes(refCount), 1).Length;

            for (int i = refStartPosition; i < refStartPosition + refCount * 2 * objRefSize; i += objRefSize)
            {
                byte[] refBuffer = objectTable.GetRange(i, objRefSize).ToArray();
                Array.Reverse(refBuffer);
                refs.Add(BitConverter.ToInt32(RegulateNullBytes(refBuffer, 4), 0));
            }

            for (int i = 0; i < refCount; i++)
            {
                buffer.Add((string)parseBinary(refs[i]), parseBinary(refs[i + refCount]));
            }

            return buffer;
        }

        private static object parseBinaryArray(int objRef)
        {
            List<object> buffer = new List<object>();
            List<int> refs = new List<int>();
            int refCount = 0;

            int refStartPosition;
            refCount = getCount(offsetTable[objRef], out refStartPosition);


            if (refCount < 15)
                refStartPosition = offsetTable[objRef] + 1;
            else
                refStartPosition = offsetTable[objRef] + 2 + RegulateNullBytes(BitConverter.GetBytes(refCount), 1).Length;

            for (int i = refStartPosition; i < refStartPosition + refCount * objRefSize; i += objRefSize)
            {
                byte[] refBuffer = objectTable.GetRange(i, objRefSize).ToArray();
                Array.Reverse(refBuffer);
                refs.Add(BitConverter.ToInt32(RegulateNullBytes(refBuffer, 4), 0));
            }

            for (int i = 0; i < refCount; i++)
            {
                buffer.Add(parseBinary(refs[i]));
            }

            return buffer;
        }

        private static int getCount(int bytePosition, out int newBytePosition)
        {
            byte headerByte = objectTable[bytePosition];
            byte headerByteTrail = Convert.ToByte(headerByte & 0xf);
            int count;
            if (headerByteTrail < 15)
            {
                count = headerByteTrail;
                newBytePosition = bytePosition + 1;
            }
            else
                count = (int)parseBinaryInt(bytePosition + 1, out newBytePosition);
            return count;
        }

        private static object parseBinary(int objRef)
        {
            byte header = objectTable[offsetTable[objRef]];
            switch (header & 0xF0)
            {
                case 0:
                    {
                        return (objectTable[offsetTable[objRef]] == 0) ? (object)null : ((objectTable[offsetTable[objRef]] == 9) ? true : false);
                    }
                case 0x10:
                    {
                        return parseBinaryInt(offsetTable[objRef]);
                    }
                case 0x20:
                    {
                        return parseBinaryReal(offsetTable[objRef]);
                    }
                case 0x30:
                    {
                        return parseBinaryDate(offsetTable[objRef]);
                    }
                case 0x40:
                    {
                        return parseBinaryByteArray(offsetTable[objRef]);
                    }
                case 0x50://String ASCII
                    {
                        return parseBinaryAsciiString(offsetTable[objRef]);
                    }
                case 0x60://String Unicode
                    {
                        return parseBinaryUnicodeString(offsetTable[objRef]);
                    }
                case 0xD0:
                    {
                        return parseBinaryDictionary(objRef);
                    }
                case 0xA0:
                    {
                        return parseBinaryArray(objRef);
                    }
            }
            throw new Exception("This type is not supported");
        }

        public static object parseBinaryDate(int headerPosition)
        {
            byte[] buffer = objectTable.GetRange(headerPosition + 1, 8).ToArray();
            Array.Reverse(buffer);
            double appleTime = BitConverter.ToDouble(buffer, 0);
            DateTime result = PlistDateConverter.ConvertFromAppleTimeStamp(appleTime);
            return result;
        }

        private static object parseBinaryInt(int headerPosition)
        {
            int output;
            return parseBinaryInt(headerPosition, out output);
        }

        private static object parseBinaryInt(int headerPosition, out int newHeaderPosition)
        {
            byte header = objectTable[headerPosition];
            int byteCount = (int)Math.Pow(2, header & 0xf);
            byte[] buffer = objectTable.GetRange(headerPosition + 1, byteCount).ToArray();
            Array.Reverse(buffer);
            newHeaderPosition = headerPosition + byteCount + 1;
            return BitConverter.ToInt32(RegulateNullBytes(buffer, 4), 0);
        }

        private static object parseBinaryReal(int headerPosition)
        {
            byte header = objectTable[headerPosition];
            int byteCount = (int)Math.Pow(2, header & 0xf);
            byte[] buffer = objectTable.GetRange(headerPosition + 1, byteCount).ToArray();
            Array.Reverse(buffer);
            return BitConverter.ToDouble(RegulateNullBytes(buffer, 8), 0);
        }

        private static object parseBinaryAsciiString(int headerPosition)
        {
            int charStartPosition;
            int charCount = getCount(headerPosition, out charStartPosition);
            var buffer = objectTable.GetRange(charStartPosition, charCount);
            return buffer.Count > 0 ? Encoding.ASCII.GetString(buffer.ToArray()) : string.Empty;
        }

        private static object parseBinaryUnicodeString(int headerPosition)
        {
            int charStartPosition;
            int charCount = getCount(headerPosition, out charStartPosition);
            charCount = charCount * 2;

            byte[] buffer = new byte[charCount];
            byte one, two;

            for (int i = 0; i < charCount; i += 2)
            {
                one = objectTable.GetRange(charStartPosition + i, 1)[0];
                two = objectTable.GetRange(charStartPosition + i + 1, 1)[0];

                if (BitConverter.IsLittleEndian)
                {
                    buffer[i] = two;
                    buffer[i + 1] = one;
                }
                else
                {
                    buffer[i] = one;
                    buffer[i + 1] = two;
                }
            }

            return Encoding.Unicode.GetString(buffer);
        }

        private static object parseBinaryByteArray(int headerPosition)
        {
            int byteStartPosition;
            int byteCount = getCount(headerPosition, out byteStartPosition);
            return objectTable.GetRange(byteStartPosition, byteCount).ToArray();
        }

        #endregion
    }

    public enum plistType
    {
        Auto, Binary, Xml
    }

    public static class PlistDateConverter
    {
        public static long timeDifference = 978307200;

        public static long GetAppleTime(long unixTime)
        {
            return unixTime - timeDifference;
        }

        public static long GetUnixTime(long appleTime)
        {
            return appleTime + timeDifference;
        }

        public static DateTime ConvertFromAppleTimeStamp(double timestamp)
        {
            DateTime origin = new DateTime(2001, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        public static double ConvertToAppleTimeStamp(DateTime date)
        {
            DateTime begin = new DateTime(2001, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - begin;
            return Math.Floor(diff.TotalSeconds);
        }
    }
}