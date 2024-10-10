using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort
{
	public class DataManager : SerializedManager<DataManager>
	{
		#region private veriables
		[SerializeField] private PlayerPersistantDefaultDataHandler _playerPersistantDefaultDataHandler;
		#endregion

		#region public static

		#endregion

		#region propertices

		public static MainPlayerProgressData PlayerData
		{
			get
			{
				return PlayerPersistantData.GetMainPlayerProgressData();
			}

			set
			{
				PlayerPersistantData.SetMainPlayerProgressData(value);
			}
		}

		#endregion

		#region Unity_callback

		public override void Awake()
		{
			base.Awake();
			PlayerPrefbsHelper.SaveData = true;
			_playerPersistantDefaultDataHandler.CheckForDefaultDataAssignment();
			OnLoadingDone();
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			OnCurrencyUnload();
		}

		public Currency GetCurrency(int currencyID)
		{
			var currencyDict = PlayerPersistantData.GetCurrancyDictionary();
			if (currencyDict.ContainsKey(currencyID))
				return currencyDict[currencyID];
			return null;
		}

		public void SaveData(MainPlayerProgressData playerData)
		{
			PlayerData = playerData;
		}

		//public void TryToGetThisCurrency(int currencyID)
		//{
		//	if (currencyID == CurrencyConstant.GEMS && MainSceneUIManager.Instance != null)
		//		MainSceneUIManager.Instance.GetView<MainGemShopView>().ShowView();
		//	else if (currencyID == CurrencyConstant.COINS || currencyID == CurrencyConstant.STONE)
		//		GlobalUIManager.Instance.GetView<ToastMessageView>().ShowMessage("COMING SOON");
		//}

		#endregion

		#region private Methods

		private void OnCurrencyUnload()
		{
			var currencyDict = PlayerPersistantData.GetCurrancyDictionary();
			foreach (var item in currencyDict)
			{
				item.Value.OnDestroy();
			}
		}

		#endregion

		#region public methods

		#endregion

		#region UNITY_EDITOR_FUNCTIONS
#if UNITY_EDITOR
		[Button]
		public void Editor_PrintPlayerPersistantData()
		{
			var allPlayerData = PlayerPersistantData.GetPlayerPrefsData();
			foreach (var keyValuePair in allPlayerData)
			{
				Debug.Log(string.Format("{0} - {1}", keyValuePair.Key, keyValuePair.Value));
			}
		}
#endif
		#endregion
	}
}