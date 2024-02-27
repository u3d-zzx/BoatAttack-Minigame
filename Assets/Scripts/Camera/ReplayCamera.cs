using System;
using Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BoatAttack
{
    public class ReplayCamera : MonoBehaviour
    {
        public static ReplayCamera instance;
        public static bool spectatorEnabled;
        public CinemachineClearShot clearShot;
        private static BoatData _focusedBoat;
        private Transform _focusPoint;
        private ICinemachineCamera _currentCam;
        private float _timeSinceCut;

        private void OnEnable()
        {
            instance = this;
            _currentCam = clearShot.LiveChild;
        }

        private void LateUpdate()
        {
            if (spectatorEnabled && _focusedBoat == null)
            {
                SetTarget(0);
            }

            if (_timeSinceCut > 3f)
            {
                _timeSinceCut = 0;
                clearShot.ResetRandomization();
            }

            if (_currentCam != clearShot.LiveChild)
            {
                _currentCam = clearShot.LiveChild;
            }

            _timeSinceCut += Time.deltaTime;
        }

        public void EnableSpectatorMode()
        {
            RaceManager.raceData.boats[0].boat.isAcc = false;
            spectatorEnabled = true;
            SetTarget(0);
        }

        public void DisableSpectatorMode()
        {
        }

        public void SetTarget(int boatIndex)
        {
            _focusedBoat = RaceManager.raceData.boats[boatIndex];
            _focusPoint = _focusedBoat.boatObject.transform;
            SetReplayTarget(_focusPoint);
        }

        private void SetReplayTarget(Transform target)
        {
            if (!clearShot && target) return;
            clearShot.Priority = 100;
            clearShot.Follow = clearShot.LookAt = _focusPoint = target;
        }
    }
}