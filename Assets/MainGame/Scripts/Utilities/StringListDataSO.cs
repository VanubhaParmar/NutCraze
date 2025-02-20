using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.NutSort {
    [CreateAssetMenu(fileName = "StringListDataSO", menuName = Constant.GAME_NAME + "/Common/StringListDataSO")]
    public class StringListDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARIABLES
        public List<string> data;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public List<string> GetRandomList(int count)
        {
            List<string> newList = new List<string>();
            newList.AddRange(data);
            newList.Shuffle();
            return newList.Take(count).ToList();
        }

        public string GetDataAtIndex(int index)
        {
            if(index >= 0 && index < data.Count)
                return data[index];

            return data.GetRandomItemFromList();
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

        #region EDITOR_FUNCTIONS
        [Button]
        public void AddData(string dataString, bool isDataDuplicationAllowed = true)
        {
            if (data == null)
                data = new List<string>();

            if (dataString[0] == '{' && dataString[dataString.Length - 1] == '}')
            {
                List<string> inputStrings = JsonConvert.DeserializeObject<List<string>>(dataString);
                inputStrings.ForEach(x => { if (!data.Contains(x)) data.Add(x); });
            }
            else
            {
                List<string> inputStrings = dataString.Split(" ").ToList();
                inputStrings.ForEach(x => { if (!data.Contains(x) || isDataDuplicationAllowed) data.Add(x); });
            }

            LevelEditorUtility.SetDirty(this);
            LevelEditorUtility.SaveAssets();
            LevelEditorUtility.Refresh();
        }
        #endregion
    }
}