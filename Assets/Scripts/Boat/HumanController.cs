using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;


namespace BoatAttack
{
    /// <summary>
    /// This sends input controls to the boat engine if 'Human'
    /// </summary>
    public class HumanController : BaseController
    {
        public static bool useGYRO;
        public static bool canTurn;
        public Volume camVol;

        private InputControls _controls;
        public float _throttle;
        private float _steering;
        private float _gyro;
        private float _gravity;
        private bool _paused;

        private float _spdEnhanceStrength = 0.03f;
        private float _accEnhanceStrength = 0.02f;
        private float _addMultiStrength = 0.03f;
        private float _addMultiStrengthacc = 0.02f;
        private float _accPower;

        private string _boatSpdEnhLevel;
        private string _boatAccEnhLevel;
        private string _boatAddLevel;

        public bool humanEngineon;

        private void Awake()
        {
            Init();
            camVol = GameObject.Find("Main Camera").GetComponent<Volume>();
            camVol.enabled = false;

            PlayerPrefs.SetInt("CtrlToggle", 0);

            AppSettings.ResetSensor();
        }

        private void Start()
        {
            _boatSpdEnhLevel = string.Format("{0}{1}", RaceManager.raceData.boats[0].boatType, "SpdEnhanceLevel");
            _boatAccEnhLevel = string.Format("{0}{1}", RaceManager.raceData.boats[0].boatType, "AccEnhanceLevel");
            _boatAddLevel = RaceManager.raceData.boats[0].boatType;
            int addindex = MenuCtrl.Getlevelfromname(_boatAddLevel);

            _accPower = (1.3f + (_accEnhanceStrength * PlayerPrefs.GetInt(_boatAccEnhLevel, 0))) *
                        (1 + addindex * _addMultiStrengthacc);
            engine.horsepower = engine.horsepower * (1 + addindex * _addMultiStrength) *
                                (1F + _spdEnhanceStrength * PlayerPrefs.GetInt(_boatSpdEnhLevel, 0));
            AppSettings.ResetSensor();
        }

        public void Init()
        {
            _controls = new InputControls();
            _controls.BoatControls.Trottle.performed += context => _throttle = humanEngineon == true ? 1f : 0f;
            _controls.BoatControls.Gravity.performed += UseGravity;
            _controls.BoatControls.Gravity.canceled += context => { _gravity = 0f; };
            _controls.BoatControls.Steering.performed += UseStick;
            _controls.BoatControls.Steering.canceled += context => { _steering = 0f; };
            _controls.BoatControls.Gyro.performed += context => _gyro = context.ReadValue<Vector3>().y;
            _controls.BoatControls.Gyro.canceled += context => _gyro = 0f;
            _controls.BoatControls.Reset.performed += ResetBoat;
            _controls.BoatControls.Freeze.performed += FreezeBoat;
            _controls.BoatControls.Time.performed += SelectTime;
            _controls.BoatControls.Accelerate.performed += Accelerate;

            camVol = GameObject.Find("Main Camera").GetComponent<Volume>();
            camVol.enabled = false;
            canTurn = false;
        }

        public static void ResetCtrlMode()
        {
            useGYRO = (PlayerPrefs.GetInt("CtrlToggle") == 1);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            _controls.BoatControls.Enable();
            controller.gasIndex = PlayerPrefs.GetFloat("InitGas");
            controller.isAcc = false;
        }

        private void OnDisable()
        {
            if (camVol)
                camVol.enabled = false;
            _controls.BoatControls.Disable();
        }

        private void UseGravity(InputAction.CallbackContext context)
        {
#if !UNITY_EDITOR
            _gravity = context.ReadValue<Vector3>().x;
#endif
#if UNITY_EDITOR
            _steering = context.ReadValue<float>() * 0f;
#endif
        }

        private void UseGyro(InputAction.CallbackContext context)
        {
#if !UNITY_EDITOR
            _gyro = context.ReadValue<Vector3>().y;
#endif
        }

        private void UseStick(InputAction.CallbackContext context)
        {
            _steering = context.ReadValue<float>();
        }

        private void GBCancle(InputAction.CallbackContext context)
        {
            _steering = 0f;
        }

        private void ResetBoat(InputAction.CallbackContext context)
        {
            AppSettings.ResetSensor();
            controller.ResetPosition();
        }

        private void Accelerate(InputAction.CallbackContext context)
        {
            controller.StartAccelerate(0.3f);
        }

        private void FreezeBoat(InputAction.CallbackContext context)
        {
            _paused = !_paused;
            if (_paused)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        public void FillGas()
        {
            controller.gasIndex = 1;
        }

        private void SelectTime(InputAction.CallbackContext context)
        {
            var value = context.ReadValue<float>();
            Debug.Log($"changing day time, input:{value}");
            DayNightController.SelectPreset(value);
        }

        void FixedUpdate()
        {
            if (controller)
                engine.Accelerate(_throttle * (controller.isAcc == true ? _accPower : 1));

            if (canTurn)
                engine.Turn(useGYRO ? (_gravity + _gyro * 2) : _steering);
        }

        private void LateUpdate()
        {
            if (RaceManager.isRaceStarted || ReplayCamera.spectatorEnabled == true)
            {
                if (controller.idleTime > 3f)
                {
                    Debug.Log($"boat {gameObject.name} was stuck, re-spawning.");
                    controller.idleTime = 0f;
                    controller.ResetPosition();
                }

                controller.idleTime = ((engine.velocityMag < 5f && humanEngineon) || transform.up.y < 0)
                    ? controller.idleTime + Time.deltaTime
                    : controller.idleTime = 0;
            }
        }
    }
}