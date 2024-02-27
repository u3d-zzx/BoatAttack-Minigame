using System;
using System.Collections;
using GameplayIngredients;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using UnityEngine.Scripting;

namespace BoatAttack
{
    [ManagerDefaultPrefab("AppManager")]
    public class AppSettings : Manager
    {
        public enum RenderRes
        {
            _Native,
            _1440p,
            _1080p,
            _720p,
            _540p,
            _360p
        }

        public enum Framerate
        {
            _30,
            _60,
            _120
        }

        public enum SpeedFormat
        {
            _Kph,
            _Mph
        }

        public enum StarkSDKCallBack
        {
            _succeed,
            _closed,
            _error
        }

        public static AppSettings instance;
        private GameObject _loadingScreenObject;
        private static readonly string _LoadingScreenName = "LoadingScreen";
        private static readonly string _volumeManagerName = "DefaultVolume";
        public static Camera mainCamera;

        [Header("Resolution Settings")] public RenderRes maxRenderSize = RenderRes._720p;
        public bool variableResolution;
        [Range(0f, 1f)] public float axisBias = 0.5f;
        public float minScale = 0.5f;
        public Framerate targetFramerate = Framerate._30;
        private float _currentDynamicScale = 1.0f;
        private float _maxScale = 1.0f;
        public SpeedFormat speedFormat = SpeedFormat._Mph;
        private float _fps;
        private float _frameCount;
        private int _frameIndex;

        private void OnEnable()
        {
            Initialize();
            RenderPipelineManager.beginCameraRendering += SetRenderScale;
            SceneManager.sceneLoaded += LevelWasLoaded;
            StartCoroutine(nameof(Countframe));
            StartCoroutine(nameof(Setframe));
            ABManager.manifestLoaded += CreateVolume;
            var abManager = ABManager.instance;
        }

        private void Initialize()
        {
            instance = this;
            Application.targetFrameRate = 300;
            mainCamera = Camera.main;
        }

        public void CreateVolume()
        {
            if (DefaultVolume.instance == null)
            {
                StartCoroutine(ABFactory.instance.CreateFromABAsync(_volumeManagerName, _volumeManagerName));
            }
        }

        private void Start()
        {
            var obj = GameObject.Find("[Debug Updater]");
            if (obj != null)
                Destroy(obj);
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= SetRenderScale;
        }

        private static void LevelWasLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!mainCamera)
            {
                mainCamera = Camera.main;
            }
            else
            {
                var cams = GameObject.FindGameObjectsWithTag("MainCamera");
                foreach (var c in cams)
                {
                    if (c != mainCamera.gameObject) Destroy(c);
                }
            }

            instance.Invoke(nameof(CleanupLoadingScreen), 0.5f);
        }

        private void CleanupLoadingScreen()
        {
            if (instance._loadingScreenObject != null)
            {
                Destroy(instance._loadingScreenObject);
            }
        }

        private void SetRenderScale(ScriptableRenderContext context, Camera cam)
        {
            float res;
            switch (maxRenderSize)
            {
                case RenderRes._360p:
                    res = 640f;
                    break;
                case RenderRes._540p:
                    res = 960f;
                    break;
                case RenderRes._720p:
                    res = 1280f;
                    break;
                case RenderRes._1080p:
                    res = 1920f;
                    break;
                case RenderRes._1440p:
                    res = 2560f;
                    break;
                default:
                    res = cam.pixelWidth;
                    break;
            }

            var renderScale = Mathf.Clamp(res / cam.pixelWidth, 0.1f, 1.0f);
            _maxScale = renderScale;
            UniversalRenderPipeline.asset.renderScale = renderScale;
        }

        private void Update()
        {
            if (!mainCamera) return;

            if (variableResolution)
            {
                mainCamera.allowDynamicResolution = false;
            }
            else
            {
                mainCamera.allowDynamicResolution = false;
            }
        }

        [Preserve]
        public IEnumerator Countframe()
        {
            while (true)
            {
                _frameCount += 1;
                yield return new WaitForEndOfFrame();
            }
        }

        [Preserve]
        public IEnumerator Setframe()
        {
            while (true)
            {
                if (_frameCount < 20)
                {
                    _frameIndex -= 1;
                }

                if (_frameCount > 40)
                {
                    _frameIndex += 1;
                }

                _fps = _frameCount;
                _frameCount = 0;
                if (_frameIndex >= 0)
                {
                    maxRenderSize = RenderRes._720p;
                }
                else if (_frameIndex <= -4 && _frameIndex > -8)
                {
                    maxRenderSize = RenderRes._540p;
                }
                else if (_frameIndex < -8)
                {
                    maxRenderSize = RenderRes._360p;
                }

                yield return new WaitForSeconds(1.0f);
            }
        }

        public void ToggleSRPBatcher(bool enabled)
        {
            UniversalRenderPipeline.asset.useSRPBatcher = enabled;
        }

        public static void LoadScene(string scenePath, LoadSceneMode mode = LoadSceneMode.Single)
        {
            LoadScene(SceneUtility.GetBuildIndexByScenePath(scenePath), mode);
        }

        public static void LoadScene(int buildIndex, LoadSceneMode mode)
        {
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            switch (mode)
            {
                case LoadSceneMode.Single:
                    instance.StartCoroutine(LoadScene(buildIndex));
                    break;
                case LoadSceneMode.Additive:
                    SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        private static IEnumerator LoadScene(int scene)
        {
            yield return ABFactory.instance.CreateFromABAsync("ui_" + _LoadingScreenName, _LoadingScreenName,
                (uifromAB) => { instance._loadingScreenObject = uifromAB; });
            Debug.Log($"loading scene {SceneUtility.GetScenePathByBuildIndex(scene)} at build index {scene}");
            SceneManager.LoadScene(scene);
            if (scene == 0)
            {
                AudioManager.instance.ResetBoatNum();
            }
        }

        public static void ResetSensor()
        {
#if !UNITY_EDITOR
            InputSystem.DisableDevice(UnityEngine.InputSystem.Gyroscope.current);
            InputSystem.DisableDevice(UnityEngine.InputSystem.GravitySensor.current);
#endif
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

        //StarkSDK
        public void StarkSDKFollow(Action<StarkSDKCallBack> action)
        {
            action(StarkSDKCallBack._succeed);
        }

        public void StarkSDKAds(Action<StarkSDKCallBack> action)
        {
            action(StarkSDKCallBack._succeed);
        }

        public void StarkSDKAddToDesk(Action<StarkSDKCallBack> action)
        {
            action(StarkSDKCallBack._succeed);
        }

        public void StarkSDKStartRecord()
        {
        }

        public void StarkSDKStopRecord()
        {
        }

        public void StarkSDKShareVideoWithTitleTopics(Action<StarkSDKCallBack> action)
        {
            action(StarkSDKCallBack._succeed);
        }
    }

    public static class ConstantData
    {
        private static readonly string[] _levels =
        {
            "Island",
            //"Island2",
            //"Cycle",
            //"Cycle1",
        };

        public static readonly string[] aiNames =
        {
            "菲利普",
            "安德鲁",
            "艾薇儿",
            "乔纳森",
            "埃瑞卡",
            "提姆",
            "洛里安",
            "安迪",
            "哈昆",
            "索菲亚",
            "马丁",
        };

        public static string firstSetBoatTime = "FirstSetBoatTime";

        public static int[] boatAdsNum = new int[]
        {
            0,
            1
        };

        public static string[] boatAdsName = new string[]
        {
            "拦截者",
            "背叛者"
        };

        public static string[] mapDescription = new string[]
        {
            "岛屿", "碧绿的海水和高耸的石灰岩岛屿 顶部是热带雨林",
            //"山崖", "高耸的岩壁和曲折的洞穴",
            //"海滩", "南国风光的赛艇胜地",
            //"海湾", "遥远的海湾"
        };

        public static readonly int[] laps =
        {
            1,
            3,
            6,
            9
        };

        public static string ctrlToggle = "CtrlToggle";
        public static string qualityToggle = "QualityToggle";
        public static Color[] colorPalette;
        private static Texture2D _colorPaletteRaw;

        public static int SeedNow
        {
            get
            {
                DateTime dt = DateTime.Now;
                return dt.Year + dt.Month + dt.Day + dt.Hour + dt.Minute + dt.Second;
            }
        }

        public static Color GetRandomPaletteColor
        {
            get
            {
                GenerateColors();
                Random.InitState(SeedNow + Random.Range(0, 1000));
                return colorPalette[Random.Range(0, colorPalette.Length)];
            }
        }

        public static string GetLevelName(int level)
        {
            return $"level_{_levels[level]}";
        }

        public static Color GetPaletteColor(int index)
        {
            GenerateColors();
            return colorPalette[index];
        }

        private static void GenerateColors()
        {
            if (colorPalette != null && colorPalette.Length != 0) return;

            if (_colorPaletteRaw == null)
                _colorPaletteRaw = Resources.Load<Texture2D>("textures/colorSwatch");

            colorPalette = _colorPaletteRaw.GetPixels();
            Debug.Log($"Found {colorPalette.Length} colors.");
        }
    }
}