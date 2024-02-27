using UnityEngine.Profiling;

namespace UnityEngine.Experimental.Rendering
{
    public class MiniProfiler : MonoBehaviour
    {
        private bool _enable = true;
        private const float _averageStatDuration = 1.0f;
        private int _frameCount;
        private float _accDeltaTime;
        private string _statsLabel;
        private GUIStyle _style;

        private float[] _frameTimes = new float[5000];
        private int _totalFrames = 0;
        private float _minFrameTime = 1000f;
        private float _maxFrameTime = 0f;

        internal class RecorderEntry
        {
            public string name;
            public int callCount;
            public float accTime;
            public Recorder recorder;
        };

        private RecorderEntry[] _recordersList =
        {
            // Warning: Keep that list in the exact same order than SRPBMarkers enum
            new RecorderEntry()
            {
                name = "UnityEngine.CoreModule.dll!UnityEngine.Rendering::RenderPipelineManager.DoRenderLoop_Internal()"
            },
            new RecorderEntry() { name = "CullScriptable" },
            new RecorderEntry() { name = "Shadows.ExecuteDrawShadows" },
            new RecorderEntry() { name = "RenderLoop.ScheduleDraw" },
            new RecorderEntry() { name = "Render PostProcessing Effects" },
        };

        enum Markers
        {
            kRenderloop,
            kCulling,
            kShadows,
            kDraw,
            kPost,
        };

        void Awake()
        {
            for (int i = 0; i < _recordersList.Length; i++)
            {
                var sampler = Sampler.Get(_recordersList[i].name);
                if (sampler.isValid)
                    _recordersList[i].recorder = sampler.GetRecorder();
            }

            _style = new GUIStyle();
            _style.fontSize = 30;
            _style.normal.textColor = Color.white;

            ResetStats();
        }

        void RazCounters()
        {
            _accDeltaTime = 0.0f;
            _frameCount = 0;
            for (int i = 0; i < _recordersList.Length; i++)
            {
                _recordersList[i].accTime = 0.0f;
                _recordersList[i].callCount = 0;
            }
        }

        void ResetStats()
        {
            _statsLabel = "Gathering data...";
            RazCounters();
        }

        void Update()
        {
            if (_enable)
            {
                _accDeltaTime += Time.unscaledDeltaTime;
                _frameCount++;
                _frameTimes[(int)Mathf.Repeat(_totalFrames, 5000)] = Time.unscaledDeltaTime;
                int frameFactor = Mathf.Clamp(_totalFrames, 0, 5000);
                float m_averageFrameTime = 0f;

                for (int i = 0; i < frameFactor; i++)
                {
                    m_averageFrameTime += _frameTimes[i];
                }

                if (_frameCount > 10)
                {
                    _minFrameTime = Time.unscaledDeltaTime < _minFrameTime ? Time.unscaledDeltaTime : _minFrameTime;
                    _maxFrameTime = Time.unscaledDeltaTime > _maxFrameTime ? Time.unscaledDeltaTime : _maxFrameTime;
                }

                // get timing & update average accumulators
                for (int i = 0; i < _recordersList.Length; i++)
                {
                    if (_recordersList[i].recorder != null)
                    {
                        // acc time in ms
                        _recordersList[i].accTime += _recordersList[i].recorder.elapsedNanoseconds / 1000000.0f;
                        _recordersList[i].callCount += _recordersList[i].recorder.sampleBlockCount;
                    }
                }

                if (_accDeltaTime >= _averageStatDuration)
                {
                    float ooFrameCount = 1.0f / (float)_frameCount;
                    float avgLoop = _recordersList[(int)Markers.kRenderloop].accTime * ooFrameCount;
                    float avgCulling = _recordersList[(int)Markers.kCulling].accTime * ooFrameCount;
                    float avgShadow = _recordersList[(int)Markers.kShadows].accTime * ooFrameCount;
                    float avgDraw = _recordersList[(int)Markers.kDraw].accTime * ooFrameCount;
                    float avgPost = _recordersList[(int)Markers.kPost].accTime * ooFrameCount;

                    _statsLabel = $"Rendering Loop Main Thread:{avgLoop:N}ms\n";
                    _statsLabel += $"    Culling:{avgCulling:N}ms\n";
                    _statsLabel += $"    Shadows:{avgShadow:N}ms\n";
                    _statsLabel += $"    Draws:{avgDraw:F2}ms\n";
                    _statsLabel += $"    PostProcessing:{avgPost:F2}ms\n";
                    _statsLabel +=
                        $"Total: {(_accDeltaTime * 1000.0f * ooFrameCount):F2}ms ({(int)(((float)_frameCount) / _accDeltaTime)} FPS)\n";

                    float frameMulti = 1f / frameFactor;
                    _statsLabel += $"Average:{(m_averageFrameTime * 1000f * frameMulti):F2}ms\n";
                    _statsLabel += $"Minimum:{_minFrameTime * 1000f:F2}ms\n";
                    _statsLabel += $"Maximum:{_maxFrameTime * 1000f:F2}ms\n";

                    RazCounters();
                }
            }

            _totalFrames++;
        }

        void OnGUI()
        {
            if (_enable)
            {
                bool SRPBatcher = UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset.useSRPBatcher;
                GUI.color = new Color(1, 1, 1, 1);
                float w = 1000, h = 356;
                if (SRPBatcher)
                    GUILayout.BeginArea(new Rect(32, 50, w, h), "(SRP batcher ON)", GUI.skin.window);
                else
                    GUILayout.BeginArea(new Rect(32, 50, w, h), "(SRP batcher OFF)", GUI.skin.window);
                GUILayout.Label(_statsLabel, _style);
                GUILayout.EndArea();
            }
        }
    }
}