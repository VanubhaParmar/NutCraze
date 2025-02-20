using System;
using UnityEditor;
using UnityEngine.Networking;

namespace Tag.NutSort.Editor
{
    public class WebRequestInEditor
    {
        #region private veriables

        private UnityWebRequest www;
        private Action<UnityWebRequest> onResponce;

        #endregion

        #region public methods

        public void Request(string url, Action<UnityWebRequest> onResponce)
        {
            www = UnityWebRequest.Get(url);
            this.onResponce = onResponce;
            EditorApplication.update += EditorUpdate;
            www.SendWebRequest();
        }

        private void EditorUpdate()
        {
            if (!www.isDone)
                return;
            EditorApplication.update -= EditorUpdate;
            onResponce?.Invoke(www);
            www.Dispose();
        }
        #endregion
    }
}