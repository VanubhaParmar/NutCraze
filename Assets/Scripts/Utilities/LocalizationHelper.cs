using I2.Loc;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public static class LocalizationHelper
    {
        #region PRIVATE_VARS
        private static List<string> categories = null;
        #endregion

        #region properties
        #endregion

        #region Override_method
        #endregion

        #region UNITY_CALLBACKS
        public static string GetTranslate(string term)
        {
            string translatedTerm = null;
            
            if (categories == null)
                categories = LocalizationManager.GetCategories();

            translatedTerm = LocalizationManager.GetTranslation(term);
            if (translatedTerm == null)
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    if (translatedTerm == null)
                        translatedTerm = LocalizationManager.GetTranslation(categories[i] + "/" + term);
                    if (translatedTerm != null)
                        break;
                }
            }
            if (translatedTerm == null && term != "")
            {
                Debug.Log("<color=red> " + term + " Word Can not be found !!!</color>");
                return term;
            }
            return translatedTerm;
        }
        #endregion

        #region PUBLIC_FUNCTIONS
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
