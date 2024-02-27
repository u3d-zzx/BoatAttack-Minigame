using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BoatAttack.UI
{
    public class SpeedEnhance : MonoBehaviour
    {
        public TextMeshProUGUI text;
        public TextMeshProUGUI boatName;
        private int _curEnhanceLevel;
        public int enhanceMaxLevel;
        public Button enhanceButton;
        private string _boatSpdEnhLevel;
        private string _speedEnhanceLevel = "SpdEnhanceLevel";

        private void Awake()
        {
            EnhanceLevelUpdate();
        }

        public void PlayAds()
        {
            AppSettings.instance.StarkSDKAds((action) =>
            {
                if (action == AppSettings.StarkSDKCallBack._succeed)
                {
                    UpGreade();
                }
                else if (action == AppSettings.StarkSDKCallBack._closed)
                {
                }
                else
                {
                }
            });
        }

        public void EnhanceLevelUpdate()
        {
            _boatSpdEnhLevel = string.Format("{0}{1}", boatName.text, _speedEnhanceLevel);
            if (!PlayerPrefs.HasKey(_boatSpdEnhLevel))
            {
                PlayerPrefs.SetInt(_boatSpdEnhLevel, 0);
            }

            _curEnhanceLevel = PlayerPrefs.GetInt(_boatSpdEnhLevel, 0);
            if (_curEnhanceLevel >= enhanceMaxLevel)
            {
                enhanceButton.interactable = false;
            }
            else
            {
                enhanceButton.interactable = true;
            }

            Debug.Log(string.Format("{0},{1}", _boatSpdEnhLevel, _curEnhanceLevel));
            text.text = string.Format("等级 {0}", _curEnhanceLevel + 1);
        }

        void UpGreade()
        {
            _curEnhanceLevel += 1;
            if (_curEnhanceLevel >= enhanceMaxLevel)
            {
                _curEnhanceLevel = enhanceMaxLevel;
                enhanceButton.interactable = false;
            }

            PlayerPrefs.SetInt(_boatSpdEnhLevel, _curEnhanceLevel);
            EnhanceLevelUpdate();
        }
    }
}