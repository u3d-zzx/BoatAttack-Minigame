using UnityEngine;
using System.Linq;
using BoatAttack;

public class MenuCtrl : MonoBehaviour
{
    private MainUI _mainUI;

    void Awake()
    {
        _mainUI = gameObject.GetComponent<MainUI>();
    }

    void Start()
    {
        PlayerPrefs.SetFloat("InitGas", 0);
        if (PlayerPrefs.GetInt(ConstantData.firstSetBoatTime) == 1 ||
            !PlayerPrefs.HasKey(ConstantData.firstSetBoatTime))
        {
            for (int i = 0; i < ConstantData.boatAdsName.Length; i++)
            {
                PlayerPrefs.SetInt(ConstantData.boatAdsName[i], ConstantData.boatAdsNum[i]);
            }

            PlayerPrefs.SetInt(ConstantData.firstSetBoatTime, 0);
        }
    }

    public static int Getlevelfromname(string boatname)
    {
        if (ConstantData.boatAdsName.Contains(boatname))
        {
            int i = ConstantData.boatAdsName.ToList().IndexOf(boatname);
            return ConstantData.boatAdsNum[i];
        }
        else
        {
            return 0;
        }
    }

    public void SelectedChange()
    {
        for (int i = 0; i < ConstantData.boatAdsName.Length; i++)
        {
            if (ConstantData.boatAdsName[i] == _mainUI.text.text)
            {
                _mainUI.boatTimeOri.text = ConstantData.boatAdsNum[i].ToString();
                _mainUI.boatTime.text = (ConstantData.boatAdsNum[i] - PlayerPrefs.GetInt(_mainUI.text.text)).ToString();
            }
        }

        if ((!PlayerPrefs.HasKey(_mainUI.text.text) || PlayerPrefs.GetInt(_mainUI.text.text) != 0) &&
            _mainUI.text.text == "背叛者")
        {
            _mainUI.unLockImageFollow.SetActive(true);
            _mainUI.unlockImageAds.SetActive(false);
            _mainUI.raceButton.interactable = false;
        }
        else if ((!PlayerPrefs.HasKey(_mainUI.text.text) || PlayerPrefs.GetInt(_mainUI.text.text) != 0) &&
                 !(_mainUI.text.text == "背叛者"))
        {
            _mainUI.unLockImageFollow.SetActive(false);
            _mainUI.unlockImageAds.SetActive(true);
            _mainUI.raceButton.interactable = false;
        }
        else
        {
            _mainUI.unLockImageFollow.SetActive(false);
            _mainUI.unlockImageAds.SetActive(false);
            _mainUI.raceButton.interactable = true;
        }
    }

    public void PlayFollow()
    {
        Follow();
    }

    public void AdsComplete()
    {
        PlayerPrefs.SetInt(_mainUI.text.text, PlayerPrefs.GetInt(_mainUI.text.text) - 1);
        SelectedChange();
    }

    public void Follow()
    {
        AppSettings.instance.StarkSDKFollow((action) =>
        {
            if (action == AppSettings.StarkSDKCallBack._succeed)
            {
                AdsComplete();
            }
            else if (action == AppSettings.StarkSDKCallBack._closed)
            {
            }
            else
            {
            }
        });
    }

    public void Recordvideo()
    {
        AppSettings.instance.StarkSDKStartRecord();
        PlayerPrefs.SetInt("Hasvideo", 1);
        _mainUI.recordButton.interactable = false;
    }

    //0:Gas 1:Unlock Boat
    public void PlayAds(int Type)
    {
        AppSettings.instance.StarkSDKAds((action) =>
        {
            if (action == AppSettings.StarkSDKCallBack._succeed)
            {
                if (Type == 0)
                {
                    _mainUI.adsButton.interactable = false;
                    PlayerPrefs.SetFloat("InitGas", 1);
                }
                else if (Type == 1)
                {
                    AdsComplete();
                }
            }
            else if (action == AppSettings.StarkSDKCallBack._closed)
            {
            }
            else
            {
            }
        });
    }

    public void addToDesk()
    {
        AppSettings.instance.StarkSDKAddToDesk((action) =>
        {
            if (action == AppSettings.StarkSDKCallBack._succeed)
            {
            }
            else if (action == AppSettings.StarkSDKCallBack._closed)
            {
            }
            else
            {
            }
        });
    }

    public void NextOption(int _currentOption)
    {
        LockUpdate(_currentOption);
        UpdateMap(_currentOption);
    }

    public void PreviousOption(int _currentOption)
    {
        LockUpdate(_currentOption);
        UpdateMap(_currentOption);
    }

    public void LockUpdate(int _currentOption)
    {
        if (BoatAttack.RaceManager.CheckPlayerLevel(_currentOption) ||
            BoatAttack.UI.MainMenuHelper.CheckTryLevel(_currentOption))
        {
            _mainUI.levelLock.SetActive(false);
            _mainUI.nextButton.interactable = true;
        }
        else
        {
            _mainUI.levelLock.SetActive(true);
            _mainUI.nextButton.interactable = false;
        }
    }

    private void UpdateMap(int _currentOption)
    {
        if (_mainUI.mapTexture.Length != 0)
        {
            _mainUI.background.overrideSprite = _mainUI.mapTexture[_currentOption * 3];
            _mainUI.mapB.overrideSprite = _mainUI.mapTexture[_currentOption * 3 + 1];
            _mainUI.mapU.overrideSprite = _mainUI.mapTexture[_currentOption * 3 + 2];
        }

        if (ConstantData.mapDescription.Length != 0)
        {
            _mainUI.mapName.text = ConstantData.mapDescription[_currentOption * 2];
            _mainUI.description.text = ConstantData.mapDescription[_currentOption * 2 + 1];
        }
    }

    public void AddtryLevel(int _currentOption)
    {
        AppSettings.instance.StarkSDKAds((action) =>
        {
            if (action == AppSettings.StarkSDKCallBack._succeed)
            {
                BoatAttack.UI.MainMenuHelper.SetTryLevel(_currentOption);
                LockUpdate(_currentOption);
            }
            else if (action == AppSettings.StarkSDKCallBack._closed)
            {
            }
            else
            {
            }
        });
    }
}