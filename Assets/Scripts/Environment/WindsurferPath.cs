using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoatAttack
{
    public class WindsurferPath : MonoBehaviour
    {
        private Cinemachine.CinemachinePathBase _pathBase;
        public float[] windsurferPositions;

        private void Awake()
        {
            if (_pathBase == null)
                _pathBase = gameObject.GetComponent<Cinemachine.CinemachinePathBase>();
            if (windsurferPositions.Length > 0)
            {
                WindsurferManager mgr = FindObjectOfType<WindsurferManager>();
                if (mgr)
                {
                    for (int i = 0; i < windsurferPositions.Length; i++)
                    {
                        mgr.AddWindsurfer(_pathBase, windsurferPositions[i]);
                    }
                }
            }
        }
    }
}