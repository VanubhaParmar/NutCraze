using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort
{
    public class PlayerPersistantDefaultDataHandler : SerializedMonoBehaviour
    {
        #region PUBLIC_VARIABLES
        [SerializeField] private MainPlayerProgressData _defaultMainPlayerProgressData;
        [SerializeField] private List<CurrencyMappingData> _currencyMappingDatas = new List<CurrencyMappingData>();
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void CheckForDefaultDataAssignment()
        {
            var mainProgressData = PlayerPersistantData.GetMainPlayerProgressData();
            if (mainProgressData == null)
                PlayerPersistantData.SetMainPlayerProgressData(_defaultMainPlayerProgressData);

            MapCurrency();
            CurrencyInit();
        }

        public float GetDefaultCurrencyAmount(int currencyId)
        {
            var currency = _currencyMappingDatas.Find(x => x.currencyID == currencyId);
            if (currency != null)
                return currency.currency.defaultValue;
            return 0f;
        }
        #endregion

        #region PRIVATE_METHODS

        private void MapCurrency()
        {
            var currencyDict = PlayerPersistantData.GetCurrancyDictionary();
            for (int i = 0; i < _currencyMappingDatas.Count; i++)
            {
                currencyDict.Add(_currencyMappingDatas[i].currencyID, _currencyMappingDatas[i].currency);
            }
            PlayerPersistantData.SetCurrancyDictionary(currencyDict);
        }

        private void CurrencyInit()
        {
            var currencyDict = PlayerPersistantData.GetCurrancyDictionary();
            foreach (var item in currencyDict)
            {
                item.Value.Init();
            }
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