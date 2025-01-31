using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace com.tag.editor {
    public static class EditorExtension
    {
        public static bool IsSprite(this string file)
        {
            file = file.ToLower();
            if (file.Contains(".png") || file.Contains(".jpg") || file.Contains(".jpeg"))
            {
                return true;
            }

            return false;
        }

        public static Texture GetEditorTexture(string name)
        {
            return (Texture)EditorGUIUtility.Load(name);
        }

        public static string GetAssetPath(this string path)
        {
            path = path.Replace(Application.dataPath, "");
            return "Assets/" + path;
        }

        public static void ShowConformDialog(string title, string msg, string ok = "OK", Action okAction = null, string cancel = "CANCEL", Action cancelAction = null)
        {
            if (EditorUtility.DisplayDialog(title, msg, ok, cancel))
            {
                if (okAction != null)
                    okAction.Invoke();
            }
            else
            {
                if (cancelAction != null)
                    cancelAction.Invoke();
            }
        }

        public static string GetMostChildFloderName(this string path)
        {
            string[] pathSplit = path.Split('/');
            return pathSplit[pathSplit.Length - 1];
        }
    }
}
