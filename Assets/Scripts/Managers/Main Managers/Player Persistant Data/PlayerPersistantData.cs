using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
	public static class PlayerPersistantData
	{
		#region PUBLIC_VARIABLES
		#endregion

		#region PRIVATE_VARIABLES
		private static PersistantVariable<MainPlayerProgressData> _mainPlayerProgressData = new PersistantVariable<MainPlayerProgressData>(PlayerPrefsKeys.Main_Player_Progress_Data_Key, null);
		private static PersistantVariable<TutorialsPlayerData> _tutorialsPlayerData = new PersistantVariable<TutorialsPlayerData>(PlayerPrefsKeys.Tutorial_Player_Data_Key, null);
		private static Dictionary<int, Currency> _currencyDict = new Dictionary<int, Currency>();
		#endregion

		#region PROPERTIES
		#endregion

		#region UNITY_CALLBACKS
		#endregion

		#region PUBLIC_METHODS
		public static MainPlayerProgressData GetMainPlayerProgressData()
		{
			return _mainPlayerProgressData.Value;
		}

		public static void SetMainPlayerProgressData(MainPlayerProgressData mainPlayerProgressData)
		{
			_mainPlayerProgressData.Value = mainPlayerProgressData;
		}

		public static TutorialsPlayerData GetTutorialsPlayerPersistantData()
		{
			return _tutorialsPlayerData.Value;
		}

		public static void SetTutorialsPlayerPersistantData(TutorialsPlayerData tutorialsPlayerData)
		{
			_tutorialsPlayerData.Value = tutorialsPlayerData;
		}

		public static Dictionary<int, Currency> GetCurrancyDictionary()
		{
			return _currencyDict;
		}

		public static void SetCurrancyDictionary(Dictionary<int, Currency> currencyDict)
		{
			_currencyDict = currencyDict;
		}

		// For Playfab Use Only >>>
		public static Dictionary<string, string> GetPlayerPersistantCurrancyData()
		{
			Dictionary<string, string> dataDictionary = new Dictionary<string, string>();
			foreach (var pair in _currencyDict)
			{
				dataDictionary.Add(pair.Value.key, pair.Value.Value.ToString());
			}
			return dataDictionary;
		}

		// For Playfab Use Only >>>
		public static void SetPlayerPersistantCurrancyData(Dictionary<string, string> currancyData)
		{
			foreach (var pair in currancyData)
			{
				foreach (var values in _currencyDict.Values)
				{
					if (values.key == pair.Key && int.TryParse(pair.Value, out int currancyVal))
					{
						values.SetValue(currancyVal);
						break;
					}
				}
			}
		}

		public static Dictionary<string, string> GetPlayerPrefsData()
		{
			Dictionary<string, string> dataDictionary = new Dictionary<string, string>();
			dataDictionary.Add(PlayerPrefsKeys.Currancy_Data_Key, SerializeUtility.SerializeObject(GetPlayerPersistantCurrancyData()));
			dataDictionary.Add(_mainPlayerProgressData._key, _mainPlayerProgressData.RawValue);
			dataDictionary.Add(_tutorialsPlayerData._key, _tutorialsPlayerData.RawValue);
			return dataDictionary;
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

	#region MAIN_PLAYER_PROGRESS_DATA
	public class MainPlayerProgressData
	{
		[JsonProperty("pglev")] public int playerGameplayLevel;

		[JsonProperty("ubc")] public int undoBoostersCount;
        [JsonProperty("esbc")] public int extraScrewBoostersCount;
    }

    public class CurrencyMappingData
	{
		[JsonProperty("cid"), CurrencyId] public int currencyID;
		[JsonProperty("cur")] public Currency currency;
	}
    #endregion

    public class PlayerPrefsKeys
	{
		public const string Currancy_Data_Key = "CurrancyPlayerData";
		public const string Main_Player_Progress_Data_Key = "MainPlayerProgressData";
		public const string Tutorial_Player_Data_Key = "TutorialPlayerData";
	}
}