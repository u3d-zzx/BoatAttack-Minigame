using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using Cinemachine;

namespace BoatAttack
{
    /// <summary>
    /// This is an overall camera manager for the demo(mainly for testing/debugging purposes) it hooks up to teh UI interface
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        public GameObject UI;
        public CameraModes camModes;
        public PlayableDirector cutsceneDirector;
        public List<CinemachineVirtualCamera> cutsceneCameras = new List<CinemachineVirtualCamera>();
        public CinemachineVirtualCamera droneCamera;
        public CinemachineVirtualCamera raceCamera;
        public CinemachineClearShot replayShots;
        public Text staticCamText;
        private int _curStaticCam = 0;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (camModes == CameraModes.Cutscene)
                    StaticCams();
                else
                    PlayCutscene();
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
                NextStaticCam();

            if (Input.GetKeyDown(KeyCode.RightArrow))
                PrevStaticCam();

            if (Input.GetKeyDown(KeyCode.H) || (Input.touchCount > 0 && Input.touches[0].tapCount == 2))
                UI.SetActive(!UI.activeSelf);
        }

        public void PlayCutscene()
        {
            camModes = CameraModes.Cutscene;
            // Lower other camera priorities
            droneCamera.Priority = 5;
            raceCamera.Priority = 5;
            replayShots.Priority = 5;
            // activate cutscene
            cutsceneDirector.enabled = true;
            cutsceneDirector.Stop();
            cutsceneDirector.Play();
        }

        void DisableCutscene()
        {
            cutsceneDirector.enabled = false;
            cutsceneDirector.Stop();
        }

        public void DroneCam()
        {
            camModes = CameraModes.Drone;
            // Lower other camera priorities
            DisableCutscene();
            raceCamera.Priority = 5;
            replayShots.Priority = 5;
            // activate drone
            droneCamera.Priority = 15;
        }

        public void RaceCam()
        {
            camModes = CameraModes.Race;
            // Lower other camera priorities
            DisableCutscene();
            droneCamera.Priority = 5;
            replayShots.Priority = 5;
            // activate drone
            raceCamera.Priority = 15;
        }

        public void ReplayCam()
        {
            camModes = CameraModes.Replay;
            // Lower other camera priorities
            DisableCutscene();
            droneCamera.Priority = 5;
            raceCamera.Priority = 5;
            // activate drone
            replayShots.Priority = 15;
        }

        public void StaticCams()
        {
            camModes = CameraModes.Static;
            // Lower other camera priorities
            DisableCutscene();
            droneCamera.Priority = 5;
            raceCamera.Priority = 5;
            replayShots.Priority = 5;
            SetStaticCam(_curStaticCam);
        }

        public void NextStaticCam()
        {
            _curStaticCam++;
            if (_curStaticCam == cutsceneCameras.Count)
                _curStaticCam = 0;
            SetStaticCam(_curStaticCam);
        }

        public void PrevStaticCam()
        {
            _curStaticCam--;
            if (_curStaticCam < 0)
                _curStaticCam = cutsceneCameras.Count - 1;
            SetStaticCam(_curStaticCam);
        }

        void SetStaticCam(int cameraIndex)
        {
            for (var i = 0; i < cutsceneCameras.Count; i++)
            {
                if (i != cameraIndex)
                {
                    cutsceneCameras[i].Priority = 5;
                }
                else
                {
                    cutsceneCameras[i].Priority = 11;
                    staticCamText.text = cutsceneCameras[i].gameObject.name.Substring(9);
                }
            }
        }

        public enum CameraModes
        {
            Cutscene,
            Race,
            Drone,
            Replay,
            Static
        }
    }
}