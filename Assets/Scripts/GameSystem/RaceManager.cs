using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using BoatAttack.UI;
using UnityEngine.Playables;
using Random = UnityEngine.Random;

namespace BoatAttack
{
    public class RaceManager : MonoBehaviour
    {
        #region Enums

        [Serializable]
        public enum GameType
        {
            Singleplayer = 0,
            LocalMultiplayer = 1,
            Multiplayer = 2,
            Spectator = 3,
            Benchmark = 4
        }

        [Serializable]
        public enum RaceType
        {
            Race,
            PointToPoint,
            TimeTrial
        }

        [Serializable]

        #endregion

        public class Race
        {
            //Race options
            public GameType game;
            public RaceType type;
            public int boatCount = 4; // currently hardcoded to 4

            //Level options
            public string level;
            public int laps = 3;
            public bool reversed;

            //Competitors
            public List<BoatData> boats;
        }

        public static RaceManager instance;
        [NonSerialized] public static bool isRaceStarted;
        [NonSerialized] public static Race raceData = new Race();
        [HideInInspector] public Race demoRaceData = new Race();
        [NonSerialized] public static float raceTime;
        private readonly Dictionary<int, float> _boatTimes = new Dictionary<int, float>();
        public static Action<bool> raceStarted;
        public static int playerBoatType;
        [HideInInspector] public int[] boats;
        public static readonly string raceUiTouchPrefab = "Race_Canvas_touch";
        private const int _playerLevel = 0x03;
        private const string _playerUnlockLevel = "playerlevel_v01";

        public static void UnlockLevel(int levelID)
        {
            int levelflag = PlayerPrefs.GetInt(_playerUnlockLevel);
            levelflag = levelflag | (1 << levelID);
            PlayerPrefs.SetInt(_playerUnlockLevel, levelflag);
        }

        public static bool CheckPlayerLevel(int levelIndex)
        {
            int levelflag = PlayerPrefs.HasKey(_playerUnlockLevel)
                ? PlayerPrefs.GetInt(_playerUnlockLevel)
                : _playerLevel;
            if (0 != (levelflag & (1 << levelIndex)))
                return true;
            return false;
        }

        public static void BoatFinished(int player)
        {
            switch (raceData.game)
            {
                case GameType.Singleplayer:
                    if (player == 0)
                    {
                        if (raceData.boats[0].boat.place == 1)
                        {
                            UnlockLevel(SceneManager.GetActiveScene().buildIndex + 1);
                        }

                        if (raceData.boats[0].boat.place < 3)
                        {
                            UnlockLevel(SceneManager.GetActiveScene().buildIndex);
                        }

                        var raceUi = raceData.boats[0].boat.raceUi;
                        raceUi.MatchEnd();
                        ReplayCamera.instance.EnableSpectatorMode();
                    }

                    break;
                case GameType.LocalMultiplayer:
                    break;
                case GameType.Multiplayer:
                    break;
                case GameType.Spectator:
                    break;
                case GameType.Benchmark:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Awake()
        {
            Debug.Log("RaceManager Loaded");
            instance = this;
        }

        private void Reset()
        {
            isRaceStarted = false;
            raceData.boats.Clear();
            raceTime = 0f;
            _boatTimes.Clear();
            raceStarted = null;
        }

        public void PartlyReset()
        {
            isRaceStarted = false;
            raceTime = 0f;
            _boatTimes.Clear();
            raceStarted = null;
        }

        public static void Setup(Scene scene, LoadSceneMode mode)
        {
            instance.StartCoroutine(SetupRace());
        }

        public static IEnumerator SetupRace()
        {
            // make sure we have the data, otherwise default to demo data
            if (raceData == null) raceData = instance.demoRaceData;
            // setup waypoints
            WaypointGroup.instance.Setup(raceData.reversed);
            // spawn boats;
            yield return instance.StartCoroutine(instance.CreateBoats());

            switch (raceData.game)
            {
                case GameType.Singleplayer:
                    yield return instance.StartCoroutine(CreatePlayerUi(0));
                    // setup camera for player 1
                    SetupCamera(0);
                    break;
                case GameType.LocalMultiplayer:
                    break;
                case GameType.Multiplayer:
                    break;
                case GameType.Spectator:
                    ReplayCamera.instance.EnableSpectatorMode();
                    break;
                case GameType.Benchmark:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            instance.StartCoroutine(BeginRace());
        }


        public static void FixPlayerBoatType()
        {
            switch (playerBoatType)
            {
                case 0:
                    raceData.boats[0].boatType = ConstantData.boatAdsName[0];
                    break;
                case 1:
                    raceData.boats[0].boatType = ConstantData.boatAdsName[1];
                    break;
                default:
                    raceData.boats[0].boatType = "default";
                    break;
            }
        }

        public static void FixSetPlayerBoat()
        {
            Debug.Log("玩家选定船型" + playerBoatType);
            SetHull(0, playerBoatType);
        }

        public static void FixSetAIBoat()
        {
            GenerateRandomBoats(raceData.boatCount - 1);
        }


        public static void SetGameType(GameType gameType)
        {
            raceData = new Race
            {
                game = gameType,
                boats = new List<BoatData>(),
                boatCount = 4,
                laps = 3,
                type = RaceType.Race
            };

            Debug.Log($"Game type set to:{raceData.game}");
            switch (raceData.game)
            {
                case GameType.Singleplayer:
                    var b = new BoatData();
                    // single player is human
                    b.human = true;
                    switch (playerBoatType)
                    {
                        case 0:
                            b.boatType = ConstantData.boatAdsName[0];
                            break;
                        case 1:
                            b.boatType = ConstantData.boatAdsName[1];
                            break;
                        default:
                            b.boatType = "default";
                            break;
                    }

                    // add player boat
                    raceData.boats.Add(b);
                    break;
                case GameType.Spectator:
                    GenerateRandomBoats(raceData.boatCount);
                    break;
                case GameType.LocalMultiplayer:
                    Debug.LogError("Not Implemented");
                    break;
                case GameType.Multiplayer:
                    Debug.LogError("Not Implemented");
                    break;
                case GameType.Benchmark:
                    Debug.LogError("Not Implemented");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void SetLevel(int levelIndex)
        {
            raceData.level = ConstantData.GetLevelName(levelIndex);
            Debug.Log($"Level set to:{levelIndex} with path:{raceData.level}");
        }

        /// <summary>
        /// Triggered to begin the race
        /// </summary>
        /// <returns></returns>
        public static IEnumerator BeginRace()
        {
            var introCams = GameObject.FindWithTag("introCameras");
            introCams.TryGetComponent<PlayableDirector>(out var introDirector);

            if (introDirector)
            {
                while (introDirector.state == PlayState.Playing)
                {
                    yield return null;
                }

                introCams.SetActive(false);
            }

            if (!(PlayerPrefs.GetInt("FirstEnterGame") == 2))
            {
                if (PlayerPrefs.GetInt("CtrlToggle") == 1)
                    raceData.boats[0].boat.raceUi.EnableGravityModle();
                else
                    raceData.boats[0].boat.raceUi.EnableButtonModle();
            }

            if (PlayerPrefs.GetInt("FirstEnterGame") == 2)
                raceData.boats[0].boat.raceUi.EnableCountDown();
            // countdown 3..2..1..
            yield return new WaitForSeconds(3f);

            isRaceStarted = true;
            PlayerPrefs.SetInt("FirstEnterGame", 2);
            raceStarted?.Invoke(isRaceStarted);
            SceneManager.sceneLoaded -= Setup;
        }

        /// <summary>
        /// Triggered when the race has finished
        /// </summary>
        private static void EndRace()
        {
            isRaceStarted = false;
            switch (raceData.game)
            {
                case GameType.Spectator:
                    UnloadRace();
                    break;
                case GameType.Singleplayer:
                    SetupCamera(0, true);
                    break;
                case GameType.LocalMultiplayer:
                    break;
                case GameType.Multiplayer:
                    break;
                case GameType.Benchmark:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void LateUpdate()
        {
            if (!isRaceStarted) return;

            int finished = raceData.boatCount;
            for (var i = 0; i < raceData.boats.Count; i++)
            {
                var boat = raceData.boats[i].boat;
                if (boat.matchComplete)
                {
                    // completed the race so no need to update
                    _boatTimes[i] = Mathf.Infinity;
                    --finished;
                }
                else
                {
                    _boatTimes[i] = boat.lapPercentage + boat.lapCount;
                }
            }

            if (isRaceStarted && finished == 0)
                EndRace();

            var mySortedList = _boatTimes.OrderBy(d => d.Value).ToList();
            var place = raceData.boatCount;
            foreach (var boat in mySortedList.Select(index => raceData.boats[index.Key].boat)
                         .Where(boat => !boat.matchComplete))
            {
                boat.place = place;
                place--;
            }

            raceTime += Time.deltaTime;
        }

        #region Utilities

        public static void LoadGame()
        {
            AppSettings.LoadScene(raceData.level);
            SceneManager.sceneLoaded += Setup;
        }

        public static void UnloadRace()
        {
            instance.Reset();
            AppSettings.LoadScene(0, LoadSceneMode.Single);
        }

        public static void SetHull(int player, int hull)
        {
            raceData.boats[player].boatABIndex = instance.boats[hull];
        }

        IEnumerator CreateBoats()
        {
            for (int i = 0; i < raceData.boats.Count; i++)
            {
                // boat to setup
                var boat = raceData.boats[i];
                instance._boatTimes.Add(i, 0f);
                // Load prefab
                var startingPosition = WaypointGroup.instance.startingPositions[i];
                int typeindex;
                if (i == 0)
                    typeindex = playerBoatType;
                else
                    typeindex = raceData.boats[i].boatABIndex;
                GameObject resultboat = null;
                yield return StartCoroutine(ABFactory.instance.CreateFromABAsync(
                    ABManager.instance.GetBoatABNameFromIndex(typeindex),
                    ABManager.instance.GetBoatPrefabNameFromIndex(typeindex, true),
                    (boatfromAB) => { resultboat = boatfromAB; }));
                if (resultboat != null)
                {
                    var boatComp = resultboat.GetComponent<Boat>();
                    if (boatComp != null)
                    {
                        boatComp.SetBoatPosition(startingPosition.GetColumn(3),
                            Quaternion.LookRotation(startingPosition.GetColumn(2)));
                    }

                    resultboat.name = boat.boatName;
                    resultboat.TryGetComponent<Boat>(out var boatController);
                    boat.SetController(resultboat, boatController);
                    boatController.Setup(i + 1, boat.human, boat.livery);
                }
            }
        }

        private static void GenerateRandomBoats(int count, bool ai = true)
        {
            List<String> occnames = new List<string>();
            Debug.Log("玩家船型" + playerBoatType);
            for (var i = 0; i < count; i++)
            {
                var boat = new BoatData();
                Random.InitState(ConstantData.SeedNow + i);
                boat.boatName = ConstantData.aiNames[Random.Range(0, ConstantData.aiNames.Length)];
                while (occnames.Contains(boat.boatName))
                {
                    boat.boatName = ConstantData.aiNames[Random.Range(0, ConstantData.aiNames.Length)];
                }

                occnames.Add(boat.boatName);
                BoatLivery livery = new BoatLivery
                {
                    primaryColor = ConstantData.GetRandomPaletteColor,
                    trimColor = ConstantData.GetRandomPaletteColor
                };
                boat.livery = livery;
                int boat_type = Random.Range(0, instance.boats.Length);
                boat.boatABIndex = boat_type;
                switch (boat_type)
                {
                    case 0:
                        boat.boatType = ConstantData.boatAdsName[0];
                        break;
                    case 1:
                        boat.boatType = ConstantData.boatAdsName[1];
                        break;
                    default:
                        boat.boatType = "default";
                        break;
                }

                if (ai)
                    boat.human = false;

                raceData.boats.Add(boat);
            }

            occnames.Clear();
        }

        private static IEnumerator CreatePlayerUi(int player)
        {
            GameObject resultobj = null;
            yield return ABFactory.instance.CreateFromABAsync("ui_" + raceUiTouchPrefab, raceUiTouchPrefab,
                (uifromAB) => { resultobj = uifromAB; });
            if (resultobj != null)
            {
                if (resultobj.TryGetComponent(out RaceUI uiComponent))
                {
                    var boatData = raceData.boats[player];
                    boatData.boat.raceUi = uiComponent;
                    uiComponent.Setup(player);
                }
            }
        }

        private static void SetupCamera(int player, bool remove = false)
        {
            // Setup race camera
            if (remove)
                AppSettings.mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer($"Player{player + 1}"));
            else
                AppSettings.mainCamera.cullingMask |= 1 << LayerMask.NameToLayer($"Player{player + 1}");
        }

        public static int GetLapCount()
        {
            if (raceData != null && raceData.type == RaceType.Race)
            {
                return raceData.laps;
            }

            return -1;
        }

        #endregion
    }
}