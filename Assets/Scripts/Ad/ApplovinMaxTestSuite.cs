using UnityEngine;
//using AMR;

namespace Tag.Ad
{
    public class ApplovinMaxTestSuite : MonoBehaviour
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public void ShowMediationTestSuite()
        {
            Debug.Log("ShowMediationTestSuite");
            //AMRSDK.startTestSuite(new string[] { "5294d0fa-672b-44d9-b344-1ef4cc268d00", "8a5d54d6-dbe8-4650-8aea-ab57af8dc187" });
            MaxSdk.ShowMediationDebugger();
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
    }
}