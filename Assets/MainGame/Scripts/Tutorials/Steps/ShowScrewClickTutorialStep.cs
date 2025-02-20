using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Tag.NutSort {
    public class ShowScrewClickTutorialStep : BaseTutorialStep
    {
        #region PUBLIC_VARIABLES
        public GridCellId targetScrewGridCellId;
        public Vector2 indicationOffset;

        [Space]
        public UnityEvent OnStartStep;
        public UnityEvent OnEndStep;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void OnStartStep1()
        {
            base.OnStartStep1();
            OnStartStep?.Invoke();

            var targetScrew = LevelManager.Instance.GetScrewOfGridCell(targetScrewGridCellId);
            targetScrew.SetScrewInteractableState(ScrewInteractibilityState.Interactable);

            if (targetScrew.TryGetScrewBehaviour(out ScrewInputBehaviour screwInputBehaviour))
            {
                screwInputBehaviour.RegisterClickAction(() => 
                {
                    screwInputBehaviour.UnregisterClickAction();
                    EndStep();
                });
            }

            Transform targetTransform = targetScrew.transform;
            CameraCache.TryFetchCamera(CameraCacheType.MAIN_SCENE_CAMERA, out Camera mainCamera);
            Vector2 screenPos = mainCamera.WorldToViewportPoint(targetTransform.position);

            Vector2 transformedPos = new Vector2(Mathf.Lerp(0f, TutorialElementHandler.mainParentRect.rect.width, screenPos.x), Mathf.Lerp(0f, TutorialElementHandler.mainParentRect.rect.height, screenPos.y));

            TutorialElementHandler.SetActiveTapHand_ByAnchoredPosition(true, transformedPos + indicationOffset);
            TutorialElementHandler.tutorialHandAnimation.PlayHandIdleMoveAnimation();
        }

        public override void EndStep()
        {
            var targetScrew = LevelManager.Instance.GetScrewOfGridCell(targetScrewGridCellId);
            targetScrew.SetScrewInteractableState(ScrewInteractibilityState.Locked);

            TutorialElementHandler.SetActiveTapHand(false);

            OnEndStep?.Invoke();
            base.EndStep();
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
}