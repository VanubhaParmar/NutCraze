using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class DailyGoalTaskUIView : SerializedMonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Text taskTitleText;
        [SerializeField] private RectFillBar taskProgressBar;
        [SerializeField] private Text taskProgressText;
        [SerializeField] private Color taskProgressColor;
        [SerializeField] private RectTransform taskCompletedParent;

        private string Task_Progress_Format { get { return $"<color=#{ColorUtility.ToHtmlStringRGBA(taskProgressColor)}>" + "{0}</color>/{1}"; } }

        private DailyGoalPlayerData dailyGoalPlayerData;
        private int currentShowingValue;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void InitializeDailyGoalTaskView(DailyGoalPlayerData dailyGoalPlayerData)
        {
            this.dailyGoalPlayerData = dailyGoalPlayerData;
            SetView();
        }

        public void SetViewProgress(int progress, bool isTaskCompleted)
        {
            currentShowingValue = progress;
            taskProgressBar.Fill(Mathf.InverseLerp(0f, dailyGoalPlayerData.dailyGoalTargetCount, progress));
            taskProgressText.text = string.Format(Task_Progress_Format, progress, dailyGoalPlayerData.dailyGoalTargetCount);

            taskCompletedParent.gameObject.SetActive(isTaskCompleted);
        }

        public bool CanPlayProgressAnimation()
        {
            return currentShowingValue != dailyGoalPlayerData.dailyGoalCurrentProgress;
        }

        public void PlayProgressAnimation(float animationTime = 0.5f)
        {
            int progressTarget = dailyGoalPlayerData.dailyGoalCurrentProgress;
            float fillTarget = Mathf.InverseLerp(0f, dailyGoalPlayerData.dailyGoalTargetCount, dailyGoalPlayerData.dailyGoalCurrentProgress);

            taskProgressBar.Fill(fillTarget, animationTime);
            taskProgressText.DoNumberAnimation(currentShowingValue, progressTarget, animationTime, Task_Progress_Format.Replace("{1}", dailyGoalPlayerData.dailyGoalTargetCount + "")).onComplete += () =>
            {
                taskCompletedParent.gameObject.SetActive(dailyGoalPlayerData.dailyGoalCurrentProgress >= dailyGoalPlayerData.dailyGoalTargetCount);
            };
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetView()
        {
            var taskData = DailyGoalsManager.Instance.DailyGoalsSystemDataSO.GetDailyGoalsTaskDataOf(dailyGoalPlayerData.dailyGoalsTaskType);

            taskTitleText.text = taskData.GetDailyGoalTaskTitle(dailyGoalPlayerData);
            taskProgressBar.Fill(Mathf.InverseLerp(0f, dailyGoalPlayerData.dailyGoalTargetCount, dailyGoalPlayerData.dailyGoalCurrentProgress));
            taskProgressText.text = string.Format(Task_Progress_Format, dailyGoalPlayerData.dailyGoalCurrentProgress, dailyGoalPlayerData.dailyGoalTargetCount);

            taskCompletedParent.gameObject.SetActive(dailyGoalPlayerData.dailyGoalCurrentProgress >= dailyGoalPlayerData.dailyGoalTargetCount);
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}