using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;
using BoatAttack.UI;

namespace BoatAttack
{
    /// <summary>
    /// This is an overall controller for a boat
    /// </summary>
    public class Boat : MonoBehaviour
    {
        // Boat stats
        public Renderer boatRenderer;
        public Renderer engineRenderer;
        public Engine engine;
        private float _spawnHeight;

        // RaceStats
        [NonSerialized] public int place = 0;
        [NonSerialized] public float lapPercentage;
        [NonSerialized] public int lapCount;
        [NonSerialized] public bool matchComplete;
        private int _wpCount = -1;
        public int lastCheckpoint = -1;
        [NonSerialized] public readonly List<float> splitTimes = new List<float>();
        public CinemachineVirtualCamera cam;
        private float _camFovVel;
        [NonSerialized] public RaceUI raceUi;
        private BaseController _controller;
        public int playerIndex;
        private bool _isHuman;
        private int _nextwp = 0;
        public bool isAcc;
        public float idleTime;

        public float gasIndex;

        // Shader Props
        private static readonly int _liveryPrimary = Shader.PropertyToID("_Color1");
        private static readonly int _liveryTrim = Shader.PropertyToID("_Color2");

        public int boatNum;

        private void Awake()
        {
            boatNum = AudioManager.instance.SetBoatNum();

            _spawnHeight = transform.localToWorldMatrix.GetColumn(3).y;
            TryGetComponent(out engine.rigidbody);
            lastCheckpoint = 0;
        }

        public void Setup(int player = 1, bool _isHuman = true, BoatLivery livery = new BoatLivery())
        {
            playerIndex = player - 1;
            cam.gameObject.layer = LayerMask.NameToLayer("Player" + player);
            SetupController(_isHuman);
            Colorize(livery);
        }

        void SetupController(bool _isHuman)
        {
            var controllerType = _isHuman ? typeof(HumanController) : typeof(AiController);
            this._isHuman = _isHuman;

            // If controller exists then make sure it's teh right one, if not add it
            if (_controller)
            {
                if (_controller.GetType() == controllerType) return;
                Destroy(_controller);
                _controller = (BaseController)gameObject.AddComponent(controllerType);
            }
            else
            {
                _controller = (BaseController)gameObject.AddComponent(controllerType);
            }
        }

        private void Update()
        {
            UpdateLaps();

            if (raceUi)
            {
                raceUi.UpdatePlaceCounter(place);
                raceUi.UpdateSpeed(engine.velocityMag);
            }

            if (WaypointGroup.instance != null && lastCheckpoint > -1)
            {
                var waypointIndex = WaypointGroup.instance.DoWaypointCheck(transform.position, lastCheckpoint);
                if (waypointIndex != lastCheckpoint)
                {
                    lastCheckpoint = waypointIndex;
                    AiController bc = _controller as AiController;
                    if (bc != null)
                    {
                        var targetWP = WaypointGroup.instance.GetWaypoint(lastCheckpoint + 2);
                        bc.AssignWp(targetWP.point);
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (cam)
            {
                var fov = Mathf.SmoothStep(80f, 100f, engine.velocityMag * 0.005f);
                cam.m_Lens.FieldOfView = Mathf.SmoothDamp(cam.m_Lens.FieldOfView, fov, ref _camFovVel, 0.5f);
            }
        }

        private void FixedUpdate()
        {
            if (!RaceManager.isRaceStarted)
            {
                if (WaypointGroup.instance == null)
                    return;
                // race not started, make sure to keep boat fairly aligned.
                var target = WaypointGroup.instance.startingPositions[playerIndex];
                Vector3 targetPosition = target.GetColumn(3);
                Vector3 targetForward = target.GetColumn(2);
                var t = transform;
                var currentPosition = t.position;
                var currentForward = t.forward;

                targetPosition.y = currentPosition.y;
                engine.rigidbody.AddForce((currentPosition - targetPosition) * 0.25f);
                engine.rigidbody.MoveRotation(Quaternion.LookRotation(Vector3.Slerp(currentForward, targetForward,
                    0.1f * Time.fixedDeltaTime)));
            }
        }

        private void UpdateLaps()
        {
            if (WaypointGroup.instance == null)
                return;

            lapPercentage = WaypointGroup.instance.GetPercentageAroundTrack(transform.position);
            var lowPercentage = WaypointGroup.instance.GetWaypoint(lastCheckpoint).normalizedDistance;
            var highPercentage = WaypointGroup.instance.GetWaypoint(lastCheckpoint + 1).normalizedDistance;
            ;
            lapPercentage = Mathf.Clamp(lapPercentage, lowPercentage, highPercentage <= 0.001f ? 1f : highPercentage);

            if (raceUi)
            {
                raceUi.UpdateLapCounter(lapCount);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("NOS1") || other.gameObject.CompareTag("NOS2"))
            {
                gasIndex += other.gameObject.CompareTag("NOS1") ? 0.4f : 0.2f;
                other.gameObject.SetActive(false);
                if (gasIndex > 1)
                {
                    gasIndex = 1;
                }

                if (_isHuman)
                {
                    if (!isAcc)
                    {
                        raceUi.SetGasFillAmount(gasIndex);
                    }
                    else
                    {
                        raceUi.GasFillAmountAdd(other.gameObject.CompareTag("NOS1") ? 0.4f : 0.2f);
                    }
                }

                if (_isHuman == false && 4.9f - place < gasIndex * 5f)
                {
                    StartAccelerate(0.19f);
                }
            }

            if (WaypointGroup.instance == null)
                return;

            if (!other.CompareTag("waypoint") || matchComplete) return;

            var wp = WaypointGroup.instance.GetTriggersWaypoint(other as BoxCollider);
            var wpIndex = WaypointGroup.instance.GetWaypointIndex(wp);
            var next = WaypointGroup.instance.GetNextWaypoint(wp);
            _nextwp = WaypointGroup.instance.GetWaypointIndex(next);
            if (wp.isCheckpoint || wpIndex == 0)
            {
                lastCheckpoint = wpIndex;

                if (_isHuman)
                {
                    WaypointGroup.instance.UpdateRenderer(wpIndex);
                }
            }


            EnteredWaypoint(wpIndex, wp.isCheckpoint);
        }

        private void EnteredWaypoint(int index, bool checkpoint)
        {
            if (WaypointGroup.instance == null)
                return;

            var count = WaypointGroup.instance.waypoints.Count;
            var nextWp = (int)Mathf.Repeat(_wpCount + 1, count);

            if ((index - nextWp) < 0 || (index - nextWp) > 2)
                return;
            _wpCount = nextWp;
            if (index != 0)
                return;
            lapCount++;
            splitTimes.Add(RaceManager.raceTime);

            if (lapCount <= RaceManager.GetLapCount())
                return;

            Debug.Log(
                $"Boat {name} finished {RaceUI.OrdinalNumber(place)} with time:{RaceUI.FormatRaceTime(splitTimes.Last())}");
            RaceManager.BoatFinished(playerIndex);
            matchComplete = true;
        }

        [ContextMenu("Randomize")]
        private void ColorizeInvoke()
        {
            Colorize(Color.black, Color.black, true);
        }

        public void SetRenderLayer(int layernum)
        {
            transform.Find("BoatHull").gameObject.layer = layernum;
            var renderobjs = transform.Find("BoatHull").gameObject.GetAllChildren();
            foreach (var item in renderobjs)
            {
                item.layer = layernum;
            }

            Invoke("ResetBoatLayer", 3);
        }

        private void ResetBoatLayer()
        {
            transform.Find("BoatHull").gameObject.layer = 11;
            var renderobjs = transform.Find("BoatHull").gameObject.GetAllChildren();
            foreach (var item in renderobjs)
            {
                item.layer = 11;
            }
        }

        public virtual void StartAccelerate(float gasthreshold)
        {
            if (gasIndex > gasthreshold && isAcc == false)
            {
                isAcc = true;
                Invoke(nameof(CancleAccelerate), gasIndex * 10f);
                if (_isHuman)
                {
                    StartCoroutine(raceUi.GasFillTween());
                }

                gasIndex = 0f;
            }
        }

        public virtual void CancleAccelerate()
        {
            if (gasIndex > 0.01f)
            {
                Invoke(nameof(CancleAccelerate), gasIndex * 10f);
                gasIndex = 0f;
            }
            else
            {
                isAcc = false;
                gasIndex = 0f;
            }
        }

        private void Colorize(Color primaryColor, Color trimColor, bool random = false)
        {
            var livery = new BoatLivery
            {
                primaryColor = random ? ConstantData.GetRandomPaletteColor : primaryColor,
                trimColor = random ? ConstantData.GetRandomPaletteColor : trimColor
            };
            Colorize(livery);
        }

        /// <summary>
        /// This sets both the primary and secondary colour and assigns via a MPB
        /// </summary>
        private void Colorize(BoatLivery livery)
        {
            boatRenderer?.material?.SetColor(_liveryPrimary, livery.primaryColor);
            engineRenderer?.material?.SetColor(_liveryPrimary, livery.primaryColor);
            boatRenderer?.material?.SetColor(_liveryTrim, livery.trimColor);
            engineRenderer?.material?.SetColor(_liveryTrim, livery.trimColor);
        }

        public void ResetPosition()
        {
            if (WaypointGroup.instance)
            {
                var resetMatrix = WaypointGroup.instance.GetWaypointForReset(lastCheckpoint, transform.position);
                var resetPoint = resetMatrix.GetColumn(3);
                resetPoint.y = _spawnHeight + 2;

                SetBoatPosition(resetPoint, resetMatrix.rotation);

                AiController bc = _controller as AiController;
                if (bc != null)
                    bc.AssignWp(WaypointGroup.instance.GetWaypoint(lastCheckpoint + 2).point);
            }
        }

        public void SetBoatPosition(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;

            engine.rigidbody.velocity = Vector3.zero;
            engine.rigidbody.angularVelocity = Vector3.zero;
            engine.rigidbody.position = position;
            engine.rigidbody.rotation = rotation;
        }
    }

    [Serializable]
    public class BoatData
    {
        public string boatName;
        public string boatType;
        public int boatABIndex;
        public BoatLivery livery;
        public bool human;
        [NonSerialized] public Boat boat;
        [NonSerialized] public GameObject boatObject;

        public void SetController(GameObject boat, Boat controller)
        {
            boatObject = boat;
            this.boat = controller;
        }
    }

    [Serializable]
    public struct BoatLivery
    {
        [ColorUsage(false)] public Color primaryColor;
        [ColorUsage(false)] public Color trimColor;
    }
}