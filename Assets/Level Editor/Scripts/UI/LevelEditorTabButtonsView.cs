using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace tag.editor
{
    public class LevelEditorTabButtonsView : SerializedMonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private List<Button> tabButtons = new List<Button>();

        private int currentOpenTab;
        private Action<int> actionToCallOnTabChange;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void InitView(Action<int> actionToCallOnTabChange)
        {
            this.actionToCallOnTabChange = actionToCallOnTabChange;
            currentOpenTab = 0;

            OpenTab(currentOpenTab);
        }
        #endregion

        #region PRIVATE_METHODS
        private void OpenTab(int index)
        {
            currentOpenTab = index;
            actionToCallOnTabChange?.Invoke(currentOpenTab);

            tabButtons.ForEach(x => x.interactable = true);
            tabButtons[index].interactable = false;
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_OpenTab(int index)
        {
            OpenTab(index);
        }
        #endregion
    }
}