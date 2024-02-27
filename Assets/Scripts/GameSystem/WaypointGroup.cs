using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace BoatAttack
{
    public class WaypointGroup : MonoBehaviour
    {
        public static WaypointGroup instance;
        public int waypointGroupId = 0;
        public Color waypointColour = Color.yellow;
        public bool loop = true;
        public int waypointstep = 10;
        public bool raceStarted = false;
        [NonSerialized] public bool reverse = false;
        [NonSerialized] public Matrix4x4[] startingPositions = new Matrix4x4[4];
        [SerializeField] public List<Waypoint> waypoints = new List<Waypoint>();
        public float length;
        private readonly Dictionary<BoxCollider, Waypoint> _triggerPairs = new Dictionary<BoxCollider, Waypoint>();
        private readonly SortedDictionary<int, Waypoint> _checkpointPairs = new SortedDictionary<int, Waypoint>();
        private readonly Dictionary<Waypoint, MeshRenderer> _rendererPairs = new Dictionary<Waypoint, MeshRenderer>();
        private readonly string _checkPointName = "checkpoint";
        private BoxCollider[] _triggers;

        private void Awake()
        {
            instance = this;
            CalculateTrackDistance();
        }

        public void Setup(bool reversed)
        {
            reverse = reversed;

            if (reverse)
            {
                waypoints.Reverse();
                waypoints.Insert(0, waypoints[waypoints.Count - 1]);
                waypoints.RemoveAt(waypoints.Count - 1);
            }

            var i = 0;
            _triggers = new BoxCollider[waypoints.Count];
            foreach (var wp in waypoints)
            {
                var obj = new GameObject($"wp{i}_trigger", typeof(BoxCollider))
                {
                    tag = gameObject.tag
                };
                obj.transform.SetPositionAndRotation(wp.point, wp.rotation);
                obj.TryGetComponent(out _triggers[i]);
                _triggers[i].isTrigger = true;
                _triggers[i].size = new Vector3(wp.width * 4f, 50f, 1f);
                _triggerPairs.Add(_triggers[i], wp);
                wp.trigger = _triggers[i];
                if (wp.isCheckpoint || i == 0)
                {
                    _checkpointPairs.Add(i, wp);
                    StartCoroutine(CreateCheckpoint(wp, i == 0));
                }

                i++;
            }

            Invoke(nameof(RecheckTrigger), 1);
            GetStartPositions();
        }

        void RecheckTrigger()
        {
            foreach (var wp in waypoints)
            {
                wp.trigger.isTrigger = true;
            }
        }

        IEnumerator CreateCheckpoint(Waypoint wp, bool start)
        {
            GameObject objresult = null;
            yield return ABFactory.instance.CreateFromABAsync(_checkPointName, _checkPointName,
                (checkpointfromAB) => { objresult = checkpointfromAB; });
            if (objresult != null)
            {
                objresult.name += wp.index.ToString();
                objresult.transform.position = wp.point;
                objresult.transform.rotation = wp.rotation;
                objresult.transform.localScale = Vector3.one * ((wp.width + 1) / 6);

                if (!start)
                    objresult.GetComponent<MeshRenderer>().enabled = false;

                _rendererPairs.Add(wp, objresult.GetComponent<MeshRenderer>());

                if (!start) yield break;

                if (objresult.TryGetComponent<MeshRenderer>(out var renderer))
                    renderer.material.SetColor("_Color", Color.red);
            }
        }

        [Serializable]
        public class Waypoint
        {
            public Vector3 point;
            [FormerlySerializedAs("WPwidth")] public float width;
            public Quaternion rotation = Quaternion.identity;
            public int index;
            public bool isCheckpoint;
            [NonSerialized] public BoxCollider trigger;
            public float normalizedDistance;

            public Waypoint(Vector3 position, float radius)
            {
                point = position;
                width = radius;
            }

            public Waypoint(Waypoint wp)
            {
                point = wp.point;
                index = wp.index;
                rotation = wp.rotation;
                isCheckpoint = wp.isCheckpoint;
                trigger = wp.trigger;
                normalizedDistance = wp.normalizedDistance;
            }
        }

        public Vector3 GetWaypointDestination(int index)
        {
            var wp = GetWaypoint(index);
            return wp.point + (Random.insideUnitSphere * wp.width);
        }

        public Waypoint GetWaypoint(int index)
        {
            return waypoints[(int)Mathf.Repeat(index, waypoints.Count)];
        }

        public int GetWaypointIndex(Waypoint wp)
        {
            return waypoints.IndexOf(wp);
        }

        public Waypoint GetTriggersWaypoint(BoxCollider trigger)
        {
            return _triggerPairs.TryGetValue(trigger, out var wp) ? wp : null;
        }

        public MeshRenderer GetRenderersWaypoint(Waypoint wp)
        {
            return _rendererPairs.TryGetValue(wp, out var meshrenderer) ? meshrenderer : null;
        }

        public Waypoint GetNextWaypoint(Waypoint wp)
        {
            return GetWaypoint(waypoints.IndexOf(wp) + 1);
        }

        public Waypoint GetPreviousWaypoint(Waypoint wp)
        {
            return GetWaypoint(waypoints.IndexOf(wp) - 1);
        }

        public Waypoint GetNextCheckpoint(Waypoint wp)
        {
            var startingPoint = waypoints.IndexOf(wp);
            return GetNextCheckpoint(startingPoint);
        }

        public Waypoint GetNextCheckpoint(int index)
        {
            return _checkpointPairs.FirstOrDefault(x => x.Key > index).Value ?? waypoints[0];
        }

        // public Waypoint GetClosestWaypoint(Vector3 point)
        // {
        //     return waypoints.OrderBy(wp => Vector3.Distance(point, wp.point)).ToArray()[0];
        // }

        // To circumvent garbage collection, the above function cannot be employed, as "system.thread" is not permitted in WebGL. Instead, we opt for an alternative method.
        public Waypoint GetClosestWaypoint(Vector3 point)
        {
            Waypoint closest = null;
            float minDistance = float.MaxValue;

            foreach (var waypoint in waypoints)
            {
                float distance = Vector3.Distance(point, waypoint.point);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = waypoint;
                }
            }

            return closest;
        }

        public void UpdateRenderer(int index)
        {
            Waypoint currentWp = GetWaypoint(index);
            Waypoint nextFirstWp = GetNextCheckpoint(index);
            Waypoint nextSecondWp = GetNextCheckpoint(nextFirstWp);
            Waypoint nextThirdWp = GetNextCheckpoint(nextSecondWp);
            Waypoint nextFourthWp = GetNextCheckpoint(nextThirdWp);
            MeshRenderer currentMr = _rendererPairs.TryGetValue(currentWp, out MeshRenderer curmr) ? curmr : null;
            if (currentMr.transform.Find("particalSystem"))
            {
                ParticleSystem currentrmps = currentMr.transform.Find("particalSystem").GetComponent<ParticleSystem>();
                currentrmps.Play();
            }

            if (currentMr.transform.Find("AnimaCheckpoint"))
            {
                Transform anima_transform = currentMr.transform.Find("AnimaCheckpoint");
                anima_transform.gameObject.SetActive(true);
            }

            currentMr.enabled = false;
            MeshRenderer nextFirstMr = _rendererPairs.TryGetValue(nextFirstWp, out MeshRenderer mr) ? mr : null;
            MeshRenderer nextSecondMr = _rendererPairs.TryGetValue(nextSecondWp, out MeshRenderer mr2) ? mr2 : null;
            MeshRenderer nextThirdMr = _rendererPairs.TryGetValue(nextThirdWp, out MeshRenderer mr3) ? mr3 : null;
            MeshRenderer nextFourthMr = _rendererPairs.TryGetValue(nextFourthWp, out MeshRenderer mr4) ? mr4 : null;
            nextFirstMr.enabled = true;
            nextSecondMr.enabled = true;
            nextThirdMr.enabled = true;
            nextFourthMr.enabled = true;
        }

        // public float GetPercentageAroundTrack(Vector3 point)
        // {
        //     var closestPoint = GetClosestPointOnPath(point, out Tuple<Waypoint, Waypoint> wps);
        //     var looped = wps.Item2.normalizedDistance <= 0;
        //     var segmentPercentage = (looped ? 1f : wps.Item2.normalizedDistance) - wps.Item1.normalizedDistance;
        //     var segmentDistance = length * segmentPercentage;
        //     var positionSegmentPercentage = Vector3.Distance(closestPoint, wps.Item1.point) / segmentDistance;
        //     return Mathf.Lerp(wps.Item1.normalizedDistance, (looped ? 1f : wps.Item2.normalizedDistance),
        //         positionSegmentPercentage);
        // }

        public Vector3 GetClosestPointOnPath(Vector3 point, out Tuple<Waypoint, Waypoint> wpIndices)
        {
            var closest = GetClosestWaypoint(point);
            var next = GetNextWaypoint(closest);
            var previous = GetPreviousWaypoint(closest);

            var nextLine = FindNearestPointOnLine(closest.point, next.point, point);
            var prevLine = FindNearestPointOnLine(closest.point, previous.point, point);

            var nextIsClosest = Vector3.Distance(point, nextLine) < Vector3.Distance(point, prevLine);

            wpIndices = new Tuple<Waypoint, Waypoint>(nextIsClosest ? closest : previous,
                nextIsClosest ? next : closest);

            return nextIsClosest ? nextLine : prevLine;
        }
        
        public float GetPercentageAroundTrack(Vector3 point)
        {
            var closestPoint = GetClosestPointOnPath(point, out (Waypoint, Waypoint) wps);
            var looped = wps.Item2.normalizedDistance <= 0;
            var segmentPercentage = (looped ? 1f : wps.Item2.normalizedDistance) - wps.Item1.normalizedDistance;
            var segmentDistance = length * segmentPercentage;
            var positionSegmentPercentage = Vector3.Distance(closestPoint, wps.Item1.point) / segmentDistance;
            return Mathf.Lerp(wps.Item1.normalizedDistance, (looped ? 1f : wps.Item2.normalizedDistance),
                positionSegmentPercentage);
        }
        
        public Vector3 GetClosestPointOnPath(Vector3 point, out (Waypoint, Waypoint) wpIndices)
        {
            var closest = GetClosestWaypoint(point);
            var next = GetNextWaypoint(closest);
            var previous = GetPreviousWaypoint(closest);

            Vector3 nextLine = Vector3.zero, prevLine = Vector3.zero;

            CalculateNearestPoints(point, closest.point, next.point, previous.point, out nextLine, out prevLine);

            var nextIsClosest = Vector3.Distance(point, nextLine) < Vector3.Distance(point, prevLine);

            wpIndices = (nextIsClosest ? closest : previous, nextIsClosest ? next : closest);

            return nextIsClosest ? nextLine : prevLine;
        }

        private void CalculateNearestPoints(Vector3 point, Vector3 start, Vector3 next, Vector3 previous, out Vector3 nextLine, out Vector3 prevLine)
        {
            nextLine = FindNearestPointOnLine(start, next, point);
            prevLine = FindNearestPointOnLine(start, previous, point);
        }

        public Matrix4x4 GetClosestPointOnWaypoint(Vector3 point, bool special)
        {
            var sortedWPs = waypoints.OrderBy(wp => Vector3.Distance(point, wp.point)).ToArray();

            var wpA = sortedWPs[0];
            var wpB = sortedWPs[1];

            if (Mathf.Abs(wpA.index - wpB.index) > 1)
                wpB = waypoints[(int)Mathf.Repeat(wpA.index + 2, waypoints.Count)];

            var respawnPoint = FindNearestPointOnLine(wpA.point, wpB.point, point);
            respawnPoint.y = 0f;

            Vector3 lookVec;
            lookVec = wpA.point - wpB.point;
            int indexA = GetWaypointIndex(wpA);
            int indexB = GetWaypointIndex(wpB);

            if (Mathf.Abs(indexA - indexB) == 2)
            {
                if (indexA > indexB)
                {
                    wpB = GetNextWaypoint(wpB);
                    indexB = GetWaypointIndex(wpB);
                }
                else
                {
                    wpB = GetPreviousWaypoint(wpB);
                    indexB = GetWaypointIndex(wpB);
                }
            }

            respawnPoint = FindNearestPointOnLine(wpA.point, wpB.point, point);
            respawnPoint.y = 0f;

            if (indexA > indexB)
            {
                if (indexA == 14 && indexB == 0)
                {
                    lookVec = wpB.point - wpA.point;
                }
                else if (indexA == 10 && special)
                {
                    lookVec = GetNextWaypoint(wpA).point - wpB.point;
                }
                else
                {
                    lookVec = wpA.point - wpB.point;
                }
            }
            else
            {
                if (indexB == 14 && indexA == 0)
                {
                    lookVec = wpA.point - wpB.point;
                }
                else
                {
                    lookVec = wpB.point - wpA.point;
                }
            }

            if ((wpA.index == 0 && wpB.index == waypoints.Count - 1) ||
                (wpB.index == 0 &&
                 wpA.index == waypoints.Count - 1)) // if at the loop point we need to reverse the lookVec
                lookVec = -lookVec;

            Quaternion facing = Quaternion.LookRotation(Vector3.Normalize(lookVec * (reverse ? -1f : 1f)), Vector3.up);

            Matrix4x4 matrix = Matrix4x4.TRS(respawnPoint, facing, Vector3.one);

            return matrix;
        }

        public Matrix4x4 GetWaypointForReset(int index, Vector3 pos)
        {
            var wp_a = waypoints[index];
            var wp_b = waypoints[(index + 1) % waypoints.Count];
            float dis0 = Vector3.Distance(wp_a.point, pos);
            float dis1 = Vector3.Distance(wp_b.point, pos);
            float temp = dis0 / (dis0 + dis1);
            float invtemp = 1 - temp;

            Vector3 a0 = wp_a.point;
            Vector3 b0 = wp_b.point;

            float length = Vector3.Distance(a0, b0);
            float weight = wp_a.width / (wp_a.width + wp_b.width);

            var aMatrix = Matrix4x4.Rotate(wp_a.rotation);
            Vector3 af = aMatrix * Vector3.forward * length * weight;
            var bMatrix = Matrix4x4.Rotate(wp_b.rotation);
            Vector3 bf = bMatrix * Vector3.forward * length * (1 - weight);
            Vector3 y0 = a0 * invtemp * invtemp * invtemp + (a0 + af) * 3 * temp * invtemp * invtemp +
                         (b0 - bf) * 3 * temp * temp * invtemp + b0 * temp * temp * temp;

            Matrix4x4 matrix = Matrix4x4.TRS(y0, Quaternion.Lerp(wp_a.rotation, wp_b.rotation, temp), Vector3.one);
            return matrix;
        }

        private Matrix4x4[] GetStartPositions()
        {
            var position = waypoints[0].point + Vector3.up;
            var rotation = waypoints[0].rotation;
            if (reverse)
                rotation *= Quaternion.AngleAxis(180f, Vector3.up);

            for (int i = 0; i < startingPositions.Length; i++)
            {
                var pos = new Vector3(i % 2 == 0 ? 3f : -3f, 0f, i * 5f + 4f);
                pos.z = -pos.z;

                startingPositions[i].SetTRS(position, rotation, Vector3.one);
                startingPositions[i] *= Matrix4x4.Translate(pos);
            }

            return startingPositions;
        }

        public float CalculateTrackDistance()
        {
            var distance = 0f;
            for (var i = 1; i < waypoints.Count; i++)
            {
                distance += Vector3.Distance(waypoints[i - 1].point, waypoints[i].point);
            }

            // if the track is a loop then add the last>first distance
            if (loop)
            {
                distance += Vector3.Distance(waypoints[0].point, waypoints[waypoints.Count - 1].point);
            }

            var percentage = 0f;
            for (var i = 1; i < waypoints.Count; i++)
            {
                var segment = Vector3.Distance(waypoints[i - 1].point, waypoints[i].point);
                percentage += segment / distance;
                waypoints[i].normalizedDistance = percentage;
            }

            return distance;
        }

        private static Vector3 FindNearestPointOnLine(Vector3 start, Vector3 end, Vector3 point)
        {
            var line = (end - start);
            var len = line.magnitude;
            line.Normalize();

            var v = point - start;
            var d = Vector3.Dot(v, line);
            d = Mathf.Clamp(d, 0f, len);
            return start + line * d;
        }

        private void OnDrawGizmos()
        {
            startingPositions = GetStartPositions();

            var c = reverse ? Color.red : Color.green;
            var startBox = new Vector3(2f, 0.1f, 6f);
            foreach (var startPos in startingPositions)
            {
                Gizmos.matrix = startPos;
                c.a = 0.5f;
                Gizmos.color = c;
                Gizmos.DrawCube(Vector3.zero, startBox);
                c.a = 1f;
                Gizmos.color = c;
                Gizmos.DrawWireCube(Vector3.zero, startBox);
            }
        }

        public int DoWaypointCheck(Vector3 pos, int oldindex)
        {
            int index = oldindex;
            if (IsPassed(index, pos) == true && IsPassed(index + 1, pos) == true)
                index = (index + 1) % waypoints.Count;
            else if (IsPassed(index, pos) == false)
                index = Mathf.Max(0, index - 1);
            return index;
        }

        private bool IsPassed(int index, Vector3 pos)
        {
            Waypoint wp = waypoints[index % waypoints.Count];
            Vector3 wforward = wp.rotation * Vector3.forward;
            Vector3 wdir = pos - wp.point;
            float ispass = Vector3.Dot(wforward, wdir);
            return ispass > 0;
        }
    }
}