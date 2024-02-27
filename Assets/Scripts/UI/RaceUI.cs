using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoatAttack.UI
{
    public class RaceUI : MonoBehaviour
    {
        private Boat _boat;
        public TextMeshProUGUI lapCounter;
        public TextMeshProUGUI positionNumber;
        public TextMeshProUGUI timeTotal;
        public TextMeshProUGUI timeLap;
        public TextMeshProUGUI speedText;
        public TextMeshProUGUI speedFormatText;
        public RectTransform map;
        public GameObject gameplayUi;
        public GameObject raceStat;
        public GameObject matchEnd;

        [Header("Assets")] public static readonly string PlayerMarker = "PlayerMarker";
        public static readonly string PlayerMapMarker = "PlayerMapMarker";
        public static readonly string RaceStatsPlayer = "RaceStats_Player";

        private int playerIndex;
        private int _totalLaps;
        private int _totalPlayers;
        private float _timeOffset;
        private float _smoothedSpeed;
        private float _smoothSpeedVel;
        private AppSettings.SpeedFormat _speedFormat;
        private RaceStatsPlayer[] _raceStats;

        //instantGame
        private Animator _ani;

        [HideInInspector] public GameObject raceCanvas;
        [HideInInspector] public Slider sliderBG;
        [HideInInspector] public Slider sliderSE;
        [HideInInspector] public Toggle gravityToggle;
        [HideInInspector] public Toggle buttonToggle;
        [HideInInspector] public Image gasIndexImage;
        [HideInInspector] public Text gasPercentText;
        [HideInInspector] public GameObject leftStick;
        [HideInInspector] public GameObject accButton;
        [HideInInspector] public Toggle engineToggle;
        [HideInInspector] public Button addButton;
        [HideInInspector] public Button shareButton;

        //地图2的小地图
        public Texture2D mapB2;
        public Texture2D mapU2;
        public Texture2D mapB3;
        public Texture2D mapU3;
        public Texture2D mapB4;
        public Texture2D mapU4;
        public Texture2D mapB5;
        public Texture2D mapU5;
        public Image mapIB;
        public Image mapIU;
        public Vector3 pos1;
        public Vector3 pos2;
        private RaceCtrl _raceCtrl;
        private HumanController _humanController;

        private void OnEnable()
        {
            _raceCtrl = gameObject.GetComponent<RaceCtrl>();
            _humanController = GameObject.FindObjectOfType<HumanController>();
            RaceManager.raceStarted += SetGameplayUi;
            _ani = transform.GetComponent<Animator>();

            raceCanvas = transform.gameObject;
            gasIndexImage = raceCanvas.transform.Find("Gameplay/GasUI/Gasindex").gameObject.GetComponent<Image>();
            gasPercentText = raceCanvas.transform.Find("Gameplay/GasUI/Gaspercent").gameObject.GetComponent<Text>();
            sliderBG = raceCanvas.transform.Find("Settings/BGSlider/Slider").gameObject.GetComponent<Slider>();
            sliderSE = raceCanvas.transform.Find("Settings/SESlider/Slider").gameObject.GetComponent<Slider>();
            gravityToggle = raceCanvas.transform.Find("Settings/SelectorBtn/Gravity").gameObject.GetComponent<Toggle>();
            buttonToggle = raceCanvas.transform.Find("Settings/SelectorBtn/Button").gameObject.GetComponent<Toggle>();
            pos1 = raceCanvas.transform.Find("Gameplay/pos1").position;
            pos2 = raceCanvas.transform.Find("Gameplay/pos2").position;
            accButton = raceCanvas.transform.Find("Gameplay/Accelerate1").gameObject;
            addButton = raceCanvas.transform.Find("Settings/Button_Ads").gameObject.GetComponent<Button>();
            shareButton = raceCanvas.transform.Find("EndMatch/ShareButton").gameObject.GetComponent<Button>();
            leftStick = raceCanvas.transform.Find("Gameplay/Steer").gameObject;
            engineToggle = raceCanvas.transform.Find("Gameplay/Toggle").gameObject.GetComponent<Toggle>();


            engineToggle.onValueChanged.AddListener((tru) =>
            {
                _raceCtrl.engineon = tru;
                _humanController.humanEngineon = _raceCtrl.engineon;
                Debug.Log("engineToggle.onValueChanged" + tru);
                GameObject.FindObjectOfType<HumanController>()._throttle = tru ? 1f : 0f;
            });

            sliderBG.value = AudioManager.instance.GetBG();
            sliderSE.value = AudioManager.instance.GetSE();
        }

        private void Update()
        {
            AudioManager.instance.SetBG(sliderBG.value);
            AudioManager.instance.SetSE(sliderSE.value);
        }

        public void Setup(int player)
        {
            playerIndex = player;
            _boat = RaceManager.raceData.boats[playerIndex].boat;
            _totalLaps = RaceManager.GetLapCount();
            _totalPlayers = RaceManager.raceData.boats.Count;
            _timeOffset = Time.time;

            switch (AppSettings.instance.speedFormat)
            {
                case AppSettings.SpeedFormat._Kph:
                    _speedFormat = AppSettings.SpeedFormat._Kph;
                    speedFormatText.text = "时速";
                    break;
                case AppSettings.SpeedFormat._Mph:
                    _speedFormat = AppSettings.SpeedFormat._Mph;
                    speedFormatText.text = "时速";
                    break;
            }

            StartCoroutine(SetupPlayerMarkers(player));
        }

        public void OnPause()
        {
            AppSettings.ResetSensor();
            addButton.interactable = true;
            Time.timeScale = 0;
        }

        public void OnPauseOff()
        {
            Time.timeScale = 1;
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        public void SetGameplayUi(bool enable)
        {
            if (enable)
            {
                foreach (var stat in _raceStats)
                {
                    stat.UpdateStats();
                }
            }

            gameplayUi.SetActive(enable);
            ToggleChange();
            _ani.SetTrigger("FirstOpen");
        }

        public void SetGameStats(bool enable)
        {
            raceStat.SetActive(enable);
        }

        public void MatchEnd()
        {
            matchEnd.SetActive(true);
            if (!(PlayerPrefs.GetInt("Hasvideo") == 1))
            {
                shareButton.interactable = false;
            }

            _ani.SetTrigger("CloseGameplay");

            SetGameStats(true);
            SetGameplayUi(false);
            foreach (var stat in raceStat.gameObject.GetComponentsInChildren<RaceStatsPlayer>())
            {
                stat.transform.SetSiblingIndex(stat.boat.place);
            }
        }

        public void ReMatch()
        {
            Time.timeScale = 1;

            var appSettings = AppSettings.instance;
            appSettings.StarkSDKStopRecord();
            appSettings.StarkSDKStartRecord();

            RaceManager.instance.StopAllCoroutines();
            RaceManager.instance.PartlyReset();
            RaceManager.LoadGame();

            AudioManager.instance.ResetBoatNum();
        }

        private IEnumerator CreateGameStats()
        {
            _raceStats = new RaceStatsPlayer[RaceManager.raceData.boatCount];
            for (var i = 0; i < RaceManager.raceData.boatCount; i++)
            {
                GameObject uiresult = null;
                yield return ABFactory.instance.CreateFromABAsync("ui_" + RaceStatsPlayer, RaceStatsPlayer,
                    (uifromAB) => { uiresult = uifromAB; });
                if (uiresult != null)
                {
                    uiresult.name += RaceManager.raceData.boats[i].boatName;
                    uiresult.transform.SetParent(raceStat.transform, false);
                    uiresult.TryGetComponent(out _raceStats[i]);
                    _raceStats[i].Setup(RaceManager.raceData.boats[i]);
                }
            }
        }

        private IEnumerator SetupPlayerMarkers(int player)
        {
            for (int i = 0; i < RaceManager.raceData.boats.Count; i++)
            {
                if (i == player) continue;
                GameObject uiresult = null;
                yield return ABFactory.instance.CreateFromABAsync("ui_" + PlayerMarker, PlayerMarker,
                    (uifromAB) => { uiresult = uifromAB; });
                if (uiresult != null)
                {
                    uiresult.transform.SetParent(gameplayUi.transform, false);
                    uiresult.name += RaceManager.raceData.boats[i].boatName;
                    if (uiresult.TryGetComponent<PlayerMarker>(out var pm))
                        pm.Setup(RaceManager.raceData.boats[i]);
                }
            }

            yield return StartCoroutine(SetupPlayerMapMarkers());
        }

        private IEnumerator SetupPlayerMapMarkers()
        {
            foreach (var boatData in RaceManager.raceData.boats)
            {
                GameObject uiresult = null;
                yield return ABFactory.instance.CreateFromABAsync("ui_" + PlayerMapMarker, PlayerMapMarker,
                    (uifromAB) => { uiresult = uifromAB; });
                if (uiresult != null)
                {
                    GameObject uiresult1 = Instantiate(uiresult, map);
                    GameObject.Destroy(uiresult);
                    if (uiresult1.TryGetComponent<PlayerMapMarker>(out var pm))
                        pm.Setup(boatData);
                }
            }

            yield return StartCoroutine(CreateGameStats());
        }

        public void UpdateLapCounter(int lap)
        {
            lapCounter.text = $"{lap}/{_totalLaps}";
        }

        public void UpdatePlaceCounter(int place)
        {
            positionNumber.text = $"{place}/{_totalPlayers}";
        }

        public void UpdateSpeed(float velocity)
        {
            var speed = 0f;

            switch (_speedFormat)
            {
                case AppSettings.SpeedFormat._Kph:
                    speed = velocity * 3.6f;
                    break;
                case AppSettings.SpeedFormat._Mph:
                    speed = velocity * 2.23694f;
                    break;
            }

            _smoothedSpeed = Mathf.SmoothDamp(_smoothedSpeed, speed, ref _smoothSpeedVel, 1f);
            speedText.text = (_smoothedSpeed / 5).ToString("000");
        }

        public void FinishMatch()
        {
            AppSettings.instance.StarkSDKStopRecord();
            PlayerPrefs.SetInt("Hasvideo", 0);
            Time.timeScale = 1;
            RaceManager.UnloadRace();
        }

        public void ShareVideo()
        {
            var appSettings = AppSettings.instance;
            appSettings.StarkSDKStopRecord();
            appSettings.StarkSDKShareVideoWithTitleTopics((action) =>
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
            PlayerPrefs.SetInt("Hasvideo", 0);
        }

        public void LateUpdate()
        {
            var rawTime = RaceManager.raceTime;
            timeTotal.text = $"总时长 {FormatRaceTime(rawTime)}";
            if (_boat)
            {
                var l = (_boat.splitTimes.Count > 0) ? rawTime - _boat.splitTimes[_boat.lapCount - 1] : 0f;
                timeLap.text = $"圈时长 {FormatRaceTime(l)}";
            }
        }

        public static string FormatRaceTime(float seconds)
        {
            var t = TimeSpan.FromSeconds(seconds);
            return $"{t.Minutes:D2}:{t.Seconds:D2}.{t.Milliseconds:D3}";
        }

        public static string OrdinalNumber(int num)
        {
            var number = num.ToString();
            if (number.EndsWith("11")) return $"{number}th";
            if (number.EndsWith("12")) return $"{number}th";
            if (number.EndsWith("13")) return $"{number}th";
            if (number.EndsWith("1")) return $"{number}st";
            if (number.EndsWith("2")) return $"{number}nd";
            if (number.EndsWith("3")) return $"{number}rd";
            return $"{number}th";
        }

        public static float BestLapFromSplitTimes(List<float> splits)
        {
            // ignore 0 as it's the beginning of the race
            if (splits.Count <= 1) return 0;
            var fastestLap = Mathf.Infinity;

            for (var i = 1; i < splits.Count; i++)
            {
                var lap = splits[i] - splits[i - 1];
                fastestLap = lap < fastestLap ? lap : fastestLap;
            }

            return fastestLap;
        }

        public void EnableCountDown()
        {
            GameObject o = transform.Find("Countdown").gameObject;
            o.SetActive(true);
        }

        public void EnableButtonModle()
        {
            GameObject o = transform.Find("ButtonModle").gameObject;
            o.SetActive(true);
            OnPause();
        }

        public void EnableGravityModle()
        {
            GameObject o = transform.Find("GravityModle").gameObject;
            o.SetActive(true);
            OnPause();
        }

        public void AddinRace()
        {
            AppSettings.instance.StarkSDKAds((action) =>
            {
                if (action == AppSettings.StarkSDKCallBack._succeed)
                {
                    addButton.interactable = false;
                    GameObject.FindObjectOfType<HumanController>().FillGas();
                    gasIndexImage.fillAmount = 1f;
                    SetGaspercent(gasIndexImage.fillAmount);
                }
                else if (action == AppSettings.StarkSDKCallBack._closed)
                {
                }
                else
                {
                }
            });
        }

        void QTReFind()
        {
            raceCanvas = transform.gameObject;
        }

        public void SetGasFillAmount(float gastarget)
        {
            gasIndexImage.fillAmount = gastarget;
            SetGaspercent(gasIndexImage.fillAmount);
        }

        public void GasFillAmountAdd(float gasadd)
        {
            gasIndexImage.fillAmount += gasadd;
            SetGaspercent(gasIndexImage.fillAmount);
        }

        public void SetGaspercent(float gasptarget)
        {
            gasPercentText.text = string.Format("{0:0%}", gasptarget);
        }

        public IEnumerator GasFillTween()
        {
            while (gasIndexImage.fillAmount > 0f)
            {
                gasIndexImage.fillAmount -= Time.deltaTime * 0.1f;
                SetGaspercent(gasIndexImage.fillAmount);
                yield return new WaitForEndOfFrame();
            }

            gasIndexImage.fillAmount = 0f;
            SetGaspercent(gasIndexImage.fillAmount);
        }

        public void ToggleChange()
        {
            raceCanvas = transform.gameObject;
            gravityToggle = raceCanvas.transform.Find("Settings/SelectorBtn/Gravity").gameObject.GetComponent<Toggle>();
            buttonToggle = raceCanvas.transform.Find("Settings/SelectorBtn/Button").gameObject.GetComponent<Toggle>();
            if (gravityToggle.isOn == true)
            {
                PlayerPrefs.SetInt(ConstantData.ctrlToggle, 1);
                leftStick.SetActive(false);
                accButton.transform.position = pos1;
            }
            else
            {
                PlayerPrefs.SetInt(ConstantData.ctrlToggle, 0);
                leftStick.SetActive(true);
                accButton.transform.position = pos2;
            }

            HumanController.ResetCtrlMode();
        }

        public void ToggleFirstSet()
        {
            Debug.Log("ToggleFirstSet");
            gravityToggle.isOn = true;
            buttonToggle.isOn = false;
        }

        public void ToggleReset()
        {
            if (PlayerPrefs.GetInt(ConstantData.ctrlToggle) == 1)
            {
                Debug.Log("ToggleReset Gravity");
                gravityToggle.isOn = true;
                buttonToggle.isOn = false;
            }
            else
            {
                Debug.Log("ToggleReset Button");
                gravityToggle.isOn = false;
                buttonToggle.isOn = true;
            }
        }
    }
}