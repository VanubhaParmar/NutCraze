using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class NamespaceAnalyzer : EditorWindow
{
    private Vector2 scrollPosition;
    private Dictionary<string, List<string>> namespaceToFiles;
    private string searchFilter = "";
    private bool showFiles = true;
    private string newNamespace = "";
    private string selectedNamespace = "";

    [MenuItem("Tools/Namespace Analyzer")]
    public static void ShowWindow()
    {
        GetWindow<NamespaceAnalyzer>("Namespace Analyzer");
    }

    private void OnEnable()
    {
        AnalyzeNamespaces();
    }

    private void AnalyzeNamespaces()
    {
        namespaceToFiles = new Dictionary<string, List<string>>();
        string[] csharpFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

        foreach (string filePath in csharpFiles)
        {
            string content = File.ReadAllText(filePath);
            string relativePath = filePath.Replace(Application.dataPath, "Assets");

            // Extract namespace using regex
            Match match = Regex.Match(content, @"namespace\s+([^\s{]+)");
            if (match.Success)
            {
                string namespaceName = match.Groups[1].Value;
                if (!namespaceToFiles.ContainsKey(namespaceName))
                {
                    namespaceToFiles[namespaceName] = new List<string>();
                }
                namespaceToFiles[namespaceName].Add(relativePath);
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Namespace Analyzer", EditorStyles.boldLabel);

        // Search filter
        searchFilter = EditorGUILayout.TextField("Filter Namespaces:", searchFilter);

        // Display total count
        GUILayout.Label($"Total Namespaces: {namespaceToFiles.Count}", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        showFiles = EditorGUILayout.Toggle("Show Files", showFiles);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (var kvp in namespaceToFiles.OrderBy(x => x.Key))
        {
            if (string.IsNullOrEmpty(searchFilter) || kvp.Key.ToLower().Contains(searchFilter.ToLower()))
            {
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button(kvp.Key, EditorStyles.label))
                {
                    selectedNamespace = kvp.Key;
                    newNamespace = kvp.Key;
                }

                if (GUILayout.Button("Edit", GUILayout.Width(50)))
                {
                    selectedNamespace = kvp.Key;
                    newNamespace = kvp.Key;
                }

                EditorGUILayout.EndHorizontal();

                if (selectedNamespace == kvp.Key)
                {
                    EditorGUI.indentLevel++;
                    newNamespace = EditorGUILayout.TextField("New Namespace:", newNamespace);
                    
                    if (GUILayout.Button("Apply Changes"))
                    {
                        UpdateNamespace(kvp.Key, newNamespace, kvp.Value);
                        AnalyzeNamespaces();
                        selectedNamespace = "";
                    }
                    EditorGUI.indentLevel--;
                }

                if (showFiles)
                {
                    EditorGUI.indentLevel++;
                    foreach (string file in kvp.Value)
                    {
                        EditorGUILayout.LabelField(file);
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Refresh"))
        {
            AnalyzeNamespaces();
        }
    }

    private void UpdateNamespace(string oldNamespace, string newNamespace, List<string> files)
    {
        foreach (string filePath in files)
        {
            string fullPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, filePath);
            string content = File.ReadAllText(fullPath);
            
            // Replace namespace declaration
            string pattern = $@"namespace\s+{Regex.Escape(oldNamespace)}\s*{{";
            string replacement = $"namespace {newNamespace} {{";
            content = Regex.Replace(content, pattern, replacement);
            
            File.WriteAllText(fullPath, content);
        }
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Namespace Updated", 
            $"Successfully updated namespace from '{oldNamespace}' to '{newNamespace}'", 
            "OK");
    }
}
