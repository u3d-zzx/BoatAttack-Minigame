using UnityEngine;
using WaterSystem;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections.Generic;

namespace BoatAttack
{
    /// <summary>
    /// This controls the logic for the wind surfer
    /// </summary>
    public class WindsurferManager : MonoBehaviour
    {
        public List<Transform> surfers;

        // point to sample wave height
        private NativeArray<float3> _points;

        // height sameple from water system
        private float3[] _heights;

        // height sameple from water system
        private float3[] _normals;

        private Vector3[] _smoothPositions;

        // the objects GUID for wave height lookup
        private int _guid;
        public GameObject windSurface;

        private void Start()
        {
            _guid = gameObject.GetInstanceID();
            Init();
        }


        private void Init()
        {
            _heights = new float3[surfers.Count];
            _normals = new float3[surfers.Count];
            _smoothPositions = new Vector3[surfers.Count];

            for (var i = 0; i < surfers.Count; i++)
            {
                _smoothPositions[i] = surfers[i].position;
            }

            _points = new NativeArray<float3>(surfers.Count, Allocator.Persistent);
        }

        private void OnDisable()
        {
            _points.Dispose();
        }

        private void Update()
        {
            GerstnerWavesJobs.UpdateSamplePoints(ref _points, _guid);
            GerstnerWavesJobs.GetData(_guid, ref _heights, ref _normals);

            for (int i = 0; i < surfers.Count; i++)
            {
                _smoothPositions[i] = surfers[i].position;
                // Sample the water height at the current position
                _points[0] = _smoothPositions[i];
                if (_heights[0].y > _smoothPositions[i].y)
                    _smoothPositions[i].y += Time.deltaTime;
                else
                    _smoothPositions[i].y -= Time.deltaTime * 0.25f;
                surfers[i].position = _smoothPositions[i];
            }
        }

        public void AddWindsurfer(Cinemachine.CinemachinePathBase path, float pos)
        {
            var obj = Instantiate(windSurface, transform);
            Cinemachine.CinemachineDollyCart dolly = obj.GetComponent<Cinemachine.CinemachineDollyCart>();
            dolly.m_Speed = 0.01f;
            dolly.m_Position = pos;

            surfers.Add(obj.transform.Find("Geo_Props_WindSurfer"));
        }
    }
}