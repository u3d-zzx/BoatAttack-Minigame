using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class DefaultVolume : MonoBehaviour
{
    public static DefaultVolume instance;
    public Volume volBaseComponent;
    public Volume volQualityComponent;
    public GameObject[] qualityVolumes;

    private void Start()
    {
        if (!instance)
        {
            instance = this;
            if (Application.isPlaying)
                DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            if (UniversalRenderPipeline.asset.debugLevel != PipelineDebugLevel.Disabled)
                Debug.Log($"Extra Volume Manager cleaned up. GUID:{gameObject.GetInstanceID()}");
#if UNITY_EDITOR
            DestroyImmediate(gameObject);
            return;
#else
            Destroy(gameObject);
            return;
#endif
        }
    }
}

#if UNITY_EDITOR
[InitializeOnLoad]
public class StartupVolume
{
    private static GameObject _vol;

    static StartupVolume()
    {
        EditorApplication.delayCall += () =>
        {
            var obj =
                AssetDatabase.LoadAssetAtPath("Assets/objects/misc/DefaultVolume.prefab", typeof(GameObject)) as
                    GameObject;
            if (obj == null) return;
            if (UniversalRenderPipeline.asset.debugLevel != PipelineDebugLevel.Disabled)
                Debug.Log($"Creating Volume Manager");
            _vol = Object.Instantiate(obj);
            _vol.hideFlags = HideFlags.HideAndDontSave;
        };
    }
}
#endif