using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "BaseConfig", menuName = Constant.GAME_NAME + "/Remote Config Data/BaseConfig")]
    public class BaseConfig : SerializedScriptableObject
    {
        #region PUBLIC_VARS

        public string defaultConfigString = "";
        public string remoteConfingID;

        #endregion

        #region PRIVATE_VARS

        [ShowInInspector, ReadOnly, TextArea] private string remoteConfigValue;

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public string GetRemoteId(ConfigType configType)
        {
            string remoteConfigID = "";
            switch (configType)
            {
                case ConfigType.Live:
                    remoteConfigID = remoteConfingID;
                    break;
                case ConfigType.QA:
                    remoteConfigID = configType.ToString().ToLower() + "_" + remoteConfingID;
                    break;
                default:
                    throw new Exception("Set Remote Config in Firebase.");
            }
#if UNITY_IOS
            remoteConfigID += "_ios";
#endif
            return remoteConfigID;
        }

        public virtual string GetDefaultString()
        {
            return defaultConfigString;
        }

        public T GetValue<T>()
        {
            return SerializeUtility.DeserializeObject<T>(remoteConfigValue);
        }

        public virtual void Init(string configString)
        {
            remoteConfigValue = configString;
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion

        #region EDITOR_CALLBACKS
        [Button]
        public void Editor_CopyDefaultConfigString()
        {
            Utility.CopyToClipboard(GetDefaultString());
        }
        #endregion
    }
}