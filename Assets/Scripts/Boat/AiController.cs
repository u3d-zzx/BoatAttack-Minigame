using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace BoatAttack
{
    /// <summary>
    /// AIController is for non-human boats to control the engine of the boat
    /// </summary>
    public class AiController : BaseController
    {
        public NavMeshPath navPath;
        private Vector3[] _pathPoint;
        private Vector3 _targetPos;
        private int _curPoint;
        private bool _foundPath;
        private int _pathPointNum;

        private bool _islevel2;

        //nav from position
        private Vector3 _tempFrom;

        //nav to position
        private Vector3 _tempTo;

        //side of destination, positive on right side, negative on left side
        private float _targetSide;
        private WaypointGroup.Waypoint[] _wPs;
        private bool _letplayerwin;
        private float _addpowervel = 0.03f;
        private float _addpoweracc = 0.02f;
        private string _boatAddLevel;
        private float _accPower;

        private void Start()
        {
            int boatid = gameObject.GetComponent<Boat>().playerIndex;
            _boatAddLevel = RaceManager.raceData.boats[boatid].boatType;
            int playerboatlevel = MenuCtrl.Getlevelfromname(RaceManager.raceData.boats[0].boatType);
            int addindex = MenuCtrl.Getlevelfromname(_boatAddLevel) + SceneManager.GetActiveScene().buildIndex - 1;
            if (addindex > playerboatlevel + SceneManager.GetActiveScene().buildIndex + 2)
                addindex = playerboatlevel + SceneManager.GetActiveScene().buildIndex + 2;
            _accPower = 1.3f * (1f + addindex * _addpoweracc);
            engine.horsepower = engine.horsepower * (1 + addindex * _addpowervel);
            RaceManager.raceStarted += StartRace;
            _islevel2 = (SceneManager.GetActiveScene().name == "level_Island2");
            navPath = new NavMeshPath();
        }

        private void StartRace(bool start)
        {
            AssignWp(WaypointGroup.instance.GetWaypoint(0 + 1).point);
            InvokeRepeating(nameof(CalculatePath), 1f, 1f);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            RaceManager.raceStarted -= StartRace;
        }

        private void LateUpdate()
        {
            if (navPath?.status == NavMeshPathStatus.PathInvalid)
            {
                if (controller.idleTime < 1.0f)
                    CalculatePath();
                else
                {
                    Debug.Log("尝试逃脱卡死");
                    RecalculatePath();
                }
            }

            if (_pathPoint != null && _pathPoint.Length > _curPoint && _foundPath)
            {
                // If we are close to the current point on the path get the next
                if (Vector3.Distance(transform.position, _pathPoint[_curPoint]) < 8)
                {
                    _curPoint++;
                    if (_curPoint >= _pathPoint.Length)
                    {
                        var boat = GetComponent<Boat>();
                        var targetWP = WaypointGroup.instance.GetWaypoint(boat.lastCheckpoint + 2);
                        AssignWp(targetWP.point);
                    }
                }
            }

            if (RaceManager.isRaceStarted)
            {
                // if been idle for 3 seconds assume AI is stuck
                if (controller.idleTime > 3f)
                {
                    Debug.Log($"AI boat {gameObject.name} was stuck, re-spawning.");
                    controller.idleTime = 0f;
                    controller.ResetPosition();
                }

                controller.idleTime = (engine.velocityMag < 5f || transform.up.y < 0)
                    ? controller.idleTime + Time.deltaTime
                    : controller.idleTime = 0f;
            }
        }

        private void FixedUpdate()
        {
            if (_pathPoint == null || _pathPoint.Length <= _curPoint) return;
            // Get angle to the destination and the side
            var normDir = _pathPoint[_curPoint] - transform.position;
            normDir = normDir.normalized;
            var dot = Vector3.Dot(normDir, transform.forward);
            //positive on right side, negative on left side
            _targetSide = Vector3.Cross(transform.forward, normDir).y;

            engine.Turn(Mathf.Clamp(_targetSide, -1.0f, 1.0f));
            engine.Accelerate((dot > 0 ? 1f : 0.25f) * (controller.isAcc == true ? _accPower : 1f) *
                              (_letplayerwin == true ? 0.8f : 1f));
        }

        public void AssignWp(Vector3 targetPos)
        {
            _targetPos = targetPos;
            _targetPos.y = 0f;

            CalculatePath();
        }

        /// <summary>
        /// Calculates a new path to the next waypoint
        /// </summary>
        private void CalculatePath()
        {
           // navPath = new NavMeshPath();
           navPath.ClearCorners();
            NavMesh.CalculatePath(transform.position, _targetPos, 255, navPath);
            if (navPath.status == NavMeshPathStatus.PathComplete)
            {
                _pathPoint = navPath.corners;
                _curPoint = 1;
                _foundPath = true;
                if (4.9f - gameObject.GetComponent<Boat>().place < controller.gasIndex * 5f)
                {
                    controller.StartAccelerate(0.19f);
                }
            }
            else if (navPath == null || navPath.status == NavMeshPathStatus.PathInvalid)
            {
                _foundPath = false;
            }
        }

        private void RecalculatePath()
        {
            //navPath = new NavMeshPath(); // New nav path
            navPath.ClearCorners();
            NavMesh.CalculatePath(transform.position, _targetPos, 255, navPath);
            if (navPath.status == NavMeshPathStatus.PathComplete) // if the path is good(complete) use it
            {
                _pathPoint = navPath.corners;
                _curPoint = 1;
                _foundPath = true;
                if (4.9f - gameObject.GetComponent<Boat>().place < controller.gasIndex * 5f)
                    controller.StartAccelerate(0.19f);
            }
            else if (navPath == null ||
                     navPath.status == NavMeshPathStatus.PathInvalid) // if the path is bad, we haven't found a path
            {
                _foundPath = false;
                Debug.Log("寻路失败");
            }
        }

        private void OnDrawGizmosSelected()
        {
            var c = Color.green;
            c.a = 0.5f;
            Gizmos.color = c;

            if (!_foundPath) return;

            Gizmos.DrawLine(transform.position + (Vector3.up * 0.1f), _targetPos);
            Gizmos.DrawSphere(_targetPos, 1);

            c = Color.red;
            Gizmos.color = c;
            if (_pathPoint[_curPoint] != Vector3.zero)
                Gizmos.DrawLine(transform.position + (Vector3.up * 0.1f), _pathPoint[_curPoint]);
        }

        private void OnDrawGizmos()
        {
            var c = Color.yellow;
            Gizmos.color = c;
            if (_pathPoint == null) return;

            for (var i = 0; i < _pathPoint.Length - 1; i++)
            {
                if (i == _pathPoint.Length - 1)
                    Gizmos.DrawLine(_pathPoint[_pathPoint.Length - 1], _pathPoint[i]);
                else
                    Gizmos.DrawLine(_pathPoint[i], _pathPoint[i + 1]);
            }
        }
    }
}