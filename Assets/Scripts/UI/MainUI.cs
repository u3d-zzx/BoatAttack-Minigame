using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    private Slider _sliderBG;
    private Slider _sliderSE;
    private const string _ctrlToggle = "CtrlToggle";
    private Toggle _gravityToggle;
    private Toggle _buttonToggle;

    //AdsUnLock
    public TextMeshProUGUI text;
    public Button raceButton;
    public GameObject unLockImageFollow;
    public GameObject unlockImageAds;
    public Button adsButton;
    public Button recordButton;
    public TextMeshProUGUI boatTime;
    public TextMeshProUGUI boatTimeOri;

    //IGEnumSelector
    public Sprite[] mapTexture;
    public Image background;
    public Image mapB;
    public Image mapU;
    public TextMeshProUGUI mapName;
    public TextMeshProUGUI description;
    public GameObject levelLock;
    public Button nextButton;
    public Button addTryButton;

    void Start()
    {
        EnableBackgroundImage();
        Init();
    }

    void EnableBackgroundImage()
    {
        var background = gameObject.transform.Find("Main/Background");
        if (background)
        {
            var image = background.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
                image.enabled = true;
        }
    }

    public void Init()
    {
        _gravityToggle = transform.Find("Settings/SelectorBtn/Gravity").gameObject.GetComponent<Toggle>();
        _buttonToggle = transform.Find("Settings/SelectorBtn/Button").gameObject.GetComponent<Toggle>();
        _sliderBG = transform.Find("Settings/BGSlider/Slider").gameObject.GetComponent<Slider>();
        _sliderBG.value = AudioManager.instance.GetBG();
        _sliderSE = transform.Find("Settings/SESlider/Slider").gameObject.GetComponent<Slider>();
        _sliderSE.value = AudioManager.instance.GetSE();

        if (!PlayerPrefs.HasKey(_ctrlToggle))
        {
            PlayerPrefs.SetInt(_ctrlToggle, 1);
            ToggleFirstSet();
        }
        else
        {
            ToggleReset();
        }
    }

    private void Update()
    {
        AudioManager.instance?.SetBG(_sliderBG.value);
        AudioManager.instance?.SetSE(_sliderSE.value);
    }

    void ToggleFirstSet()
    {
        _gravityToggle.isOn = true;
        _buttonToggle.isOn = false;
    }

    void ToggleReset()
    {
        if (PlayerPrefs.GetInt(_ctrlToggle) == 1)
        {
            _gravityToggle.isOn = true;
            _buttonToggle.isOn = false;
        }
        else
        {
            _gravityToggle.isOn = false;
            _buttonToggle.isOn = true;
        }
    }

    public void ToggleChange()
    {
        if (_gravityToggle.isOn == true)
        {
            PlayerPrefs.SetInt(_ctrlToggle, 1);
        }
        else
        {
            PlayerPrefs.SetInt(_ctrlToggle, 0);
        }
    }
}