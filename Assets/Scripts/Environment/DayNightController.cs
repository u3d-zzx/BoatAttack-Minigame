using UnityEngine;

namespace BoatAttack
{
    /// <summary>
    /// Simple day/night system
    /// </summary>
    [ExecuteInEditMode]
    public class DayNightController : MonoBehaviour
    {
        private static DayNightController _instance;

        [Range(0, 1)]
        // the global 'time'
        public float time = 0.5f;

        private readonly float[] _presets = { 0.27f, 0.35f, 0.45f, 0.55f, 0.65f, 0.73f };
        private int _currentPreset;
        private const string _presetKey = "BoatAttack.DayNight.TimePreset";
        public bool autoIcrement;
        public float speed = 1f;

        public static float globalTime;

        // previous time
        private float _prevTime;

        [Header("Skybox Settings")] public Material skybox;
        public Gradient skyboxColour;
        public Transform clouds;
        [Range(-180, 180)] public float cloudOffset = 0f;
        public ReflectionProbe[] reflections;

        [Header("Sun Settings")] public Light sun;
        public Gradient sunColour;
        [Range(0, 360)] public float northHeading = 136f;
        [Range(0, 90)] public float tilt = 60f;

        [Header("Ambient Lighting")] public Gradient ambientColour;

        [Header("Fog Settings")] [GradientUsage(true)]
        public Gradient fogColour;

        void Awake()
        {
            _instance = this;
            _currentPreset = 2;
            SetTimeOfDay(_presets[_currentPreset], true);
            _prevTime = time;
        }

        void Update()
        {
            if (autoIcrement)
            {
                var t = Mathf.PingPong(Time.time * speed, 1);
                time = t * 0.5f + 0.25f;
            }

            // if time has changed
            if (time != _prevTime)
            {
                SetTimeOfDay(time);
            }
        }

        /// <summary>
        /// Sets the time of day
        /// </summary>
        /// <param name="time">Time in linear 0-1</param>
        public void SetTimeOfDay(float time, bool reflectionUpdate = false)
        {
            this.time = time;
            _prevTime = time;

            if (reflectionUpdate && _instance.reflections?.Length > 0)
            {
                foreach (var probe in _instance.reflections)
                {
                    probe.RenderProbe();
                }
            }

            globalTime = this.time;

            if (sun)
            {
                sun.transform.forward = Vector3.down;
                sun.transform.rotation *= Quaternion.AngleAxis(northHeading, Vector3.forward);
                sun.transform.rotation *= Quaternion.AngleAxis(tilt, Vector3.up);
                sun.transform.rotation *= Quaternion.AngleAxis((this.time * 360f) - 180f, Vector3.right);

                sun.color = sunColour.Evaluate(TimeToGradient(this.time));
            }

            if (skybox)
            {
                // rotate slightly for cheap moving cloud effect
                skybox.SetFloat("_Rotation", 85 + ((this.time - 0.5f) * 20f));
                skybox.SetColor("_Tint", skyboxColour.Evaluate(TimeToGradient(this.time)));
            }

            if (clouds)
            {
                clouds.eulerAngles = new Vector3(0f, this.time * 22.5f + cloudOffset, 0f);
            }

            Shader.SetGlobalFloat("_NightFade", Mathf.Clamp01(Mathf.Abs(this.time * 2f - 1f) * 3f - 1f));
            RenderSettings.fogColor = fogColour.Evaluate(TimeToGradient(this.time));
            RenderSettings.ambientSkyColor = ambientColour.Evaluate(TimeToGradient(this.time));
        }

        float TimeToGradient(float time)
        {
            return Mathf.Abs(time * 2f - 1f);
        }

        public static void SelectPreset(float input)
        {
            _instance._currentPreset += Mathf.RoundToInt(input);
            _instance._currentPreset = (int)Mathf.Repeat(_instance._currentPreset, _instance._presets.Length);
            PlayerPrefs.SetInt(_presetKey, _instance._currentPreset);
            _instance.SetTimeOfDay(_instance._presets[_instance._currentPreset], true);
        }
    }
}