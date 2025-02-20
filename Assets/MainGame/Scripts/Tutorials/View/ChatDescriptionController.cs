using UnityEngine;

namespace Tag.NutSort {
    public class ChatDescriptionController : MonoBehaviour
    {
        #region PRIVATE_VARS

        [SerializeField] private Transform _dialogueLocation;
        [SerializeField] private string description;

        #endregion

        #region PUBLIC_METHODS

        public void ActivateChatView()
        {
            TutorialChatView tutorialChatView = TutorialElementHandler.Instance.tutorialChatView;
            tutorialChatView.transform.position = _dialogueLocation.position;
            tutorialChatView.ShowView(description);
        }

        public void DeactivateChatView()
        {
            TutorialElementHandler.Instance.tutorialChatView.Hide();
        }

        #endregion
    }
}

