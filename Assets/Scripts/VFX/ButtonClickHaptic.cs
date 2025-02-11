using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    [RequireComponent(typeof(Button))]
    public class ButtonClickHaptic : MonoBehaviour
    {
        #region PUBLIC_VARS
        [SerializeField] private VibrateIntensity intensity = VibrateIntensity.Light;
        #endregion

        #region PRIVATE_VARS
        #endregion

        #region UNITY_CALLBACKS
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(PlayButtonHaptic);
        }
        #endregion

        #region PUBLIC_FUNCTIONS
        #endregion

        #region PRIVATE_FUNCTIONS
        private void PlayButtonHaptic()
        {
            Vibrator.Vibrate(intensity);
        }
        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}
