using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class MainView : BaseView
    {
        #region PUBLIC_VARS
        #endregion

        #region PRIVATE_VARS

        [SerializeField] private TMP_InputField levelNo;
        [SerializeField] private TMP_Text metaButtonText;
        [SerializeField] private RectFillBar fillBar;
        [SerializeField] private Button newAreaButton;
        [SerializeField] private Dictionary<LevelType, LevelButtonView> levelButton = new Dictionary<LevelType, LevelButtonView>();
        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show();
            SetUI();
            //MainSceneUIManager.Instance.OnMainView();
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        public void SetUI()
        {
            foreach (var item in levelButton)
            {
                item.Value.gameObject.SetActive(false);
            }
            levelNo.text = DataManager.PlayerLevel.Value.ToString();
            LevelDataSO levelDataSO = ResourceManager.Instance.GetLevelData(DataManager.PlayerLevel.Value);
            if (levelButton.ContainsKey(levelDataSO.LevelType))
            {
                levelButton[levelDataSO.LevelType].SetView(OnClick_Level);
            }
            else
            {
                levelButton[LevelType.NORMAL_LEVEL].SetView(OnClick_Level);
            }
        }

        #endregion

        #region CO-ROUTINES

        public Coroutine WaitAndCallAction(Action action, DateTime endTime)
        {
            return StartCoroutine(WaitForActionCO(action, endTime));
        }

        private IEnumerator WaitForActionCO(Action action, DateTime endTime)
        {
            WaitForSeconds wait = new WaitForSeconds(1f);
            while (DateTime.Now < endTime)
            {
                yield return wait;
            }
            action?.Invoke();
        }
        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       
        public void OnClick_Level()
        {
            if (DataManager.Instance.HasEnoughCurrency(CurrencyConstant.LIFE, 1))
            {
                MainSceneUIManager.Instance.GetView<NotEnoughLifeView>().ShowView(() =>
                {
                    if (!string.IsNullOrEmpty(levelNo.text) && !string.IsNullOrWhiteSpace(levelNo.text))
                    {
                        DataManager.PlayerLevel.SetValue(int.Parse(levelNo.text));
                    }
                    GameplayManager.Instance.StartMainGameLevel();
                }, null);
            }
        }

        public void OnSetting()
        {
            MainSceneUIManager.Instance.GetView<SettingsView>().Show();
        }

        public void OnPlayerProfileClick()
        {
            //MainSceneUIManager.Instance.GetView<PlayerProfileview>().Show();
        }

        #endregion
    }
}
