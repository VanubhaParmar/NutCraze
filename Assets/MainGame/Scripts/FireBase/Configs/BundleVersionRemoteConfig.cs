using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tag.nut_sort {
    [CreateAssetMenu(fileName = "BundleVersionRemoteConfig", menuName = Constant.GAME_NAME + "/Remote Config Data/BundleVersionRemoteConfig")]
    public class BundleVersionRemoteConfig : BaseConfig
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override string GetDefaultString()
        {
            BundleVersionRCData defaultBundleVersionRCData = new BundleVersionRCData();
            defaultBundleVersionRCData.latestBundleVersion = VersionCodeFetcher.GetBundleVersionCode();
            defaultBundleVersionRCData.minimumAllowedBundleVersion = defaultBundleVersionRCData.latestBundleVersion;

            return SerializeUtility.SerializeObject(defaultBundleVersionRCData);
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

    public class BundleVersionRCData
    {
        public int latestBundleVersion;
        public int minimumAllowedBundleVersion;
    }
}