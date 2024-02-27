using System;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using WaterSystem;

namespace BoatAttack
{
    public class Engine : MonoBehaviour
    {
        // The rigid body attatched to the boat
        [NonSerialized] public Rigidbody rigidbody;

        // Boats velocity
        [NonSerialized] public float velocityMag;

        //engine stats
        public float steeringTorque = 5f;

        // CSH change  give more power to human controll , if player has watch the ads.
        public float horsepower = 30f;

        // engine submerged check
        private NativeArray<float3> _point;
        private float3[] _heights = new float3[1];
        private float3[] _normals = new float3[1];
        private int _guid;
        private float _yHeight;
        public Vector3 enginePosition;
        private float _turnVel;
        private float _currentAngle;

        private void Awake()
        {
            // Get the engines GUID for the buoyancy system
            _guid = GetInstanceID();
            _point = new NativeArray<float3>(1, Allocator.Persistent);
        }

        private void FixedUpdate()
        {
            velocityMag = rigidbody.velocity.sqrMagnitude; // get the sqr mag

            if (_point.IsCreated)
            {
                // Get the water level from the engines position and store it
                _point[0] = transform.TransformPoint(enginePosition);
                GerstnerWavesJobs.UpdateSamplePoints(ref _point, _guid);
                GerstnerWavesJobs.GetData(_guid, ref _heights, ref _normals);
                _yHeight = _heights[0].y - _point[0].y;
            }
        }

        private void OnDisable()
        {
            if (_point.IsCreated)
                _point.Dispose();
        }

        /// <summary>
        /// Controls the acceleration of the boat
        /// </summary>
        /// <param name="modifier">Acceleration modifier, adds force in the 0-1 range</param>
        public void Accelerate(float modifier)
        {
            if (_yHeight > -0.1f)
            {
                modifier = Mathf.Clamp(modifier, 0f, 2f);
                var forward = rigidbody.transform.forward;
                forward.y = 0f;
                forward.Normalize();
                rigidbody.AddForce(horsepower * modifier * forward, ForceMode.Acceleration);
                rigidbody.AddRelativeTorque(-Vector3.right * modifier, ForceMode.Acceleration);
            }
        }

        /// <summary>
        /// Controls the turning of the boat
        /// </summary>
        /// <param name="modifier">Steering modifier, positive for right, negative for negative</param>
        public void Turn(float modifier)
        {
            if (_yHeight > -0.1f)
            {
                modifier = Mathf.Clamp(modifier, -1f, 1f);
                rigidbody.AddRelativeTorque(new Vector3(0f, steeringTorque, -steeringTorque * 0.5f) * modifier,
                    ForceMode.Acceleration); // add torque based on input and torque amount
            }

            _currentAngle = Mathf.SmoothDampAngle(_currentAngle,
                60f * -modifier,
                ref _turnVel,
                0.5f,
                10f,
                Time.fixedTime);
            transform.localEulerAngles = new Vector3(0f, _currentAngle, 0f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            // Draw teh engine position with sphere
            Gizmos.DrawCube(enginePosition, new Vector3(0.1f, 0.2f, 0.3f));
        }
    }
}