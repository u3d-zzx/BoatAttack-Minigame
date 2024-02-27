using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ABManager : MonoBehaviour
{
    public static ABManager _instance = null;
    public static string manifestABRoot;
    public static Action manifestLoaded;
    public AssetBundleManifest assetBundleManifest;
    private string _streamingAssetPath = Application.streamingAssetsPath;

#if UNITY_EDITOR
    public static string localABRoot;
#endif

    public static ABManager instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject(nameof(ABManager));
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<ABManager>();
            }

            return _instance;
        }
    }

    void Awake()
    {
#if UNITY_EDITOR
        localABRoot = Path.Combine(Application.dataPath, "../AssetBundles/", EditorUserBuildSettings.activeBuildTarget.ToString());
        manifestABRoot = Path.Combine(localABRoot, EditorUserBuildSettings.activeBuildTarget.ToString());
        if (!File.Exists(manifestABRoot))
            return;

#elif UNITY_WEBGL

#if UNITY_WEIXINMINIGAME
        manifestABRoot = AutoStreaming.CustomCloudAssetsRoot + "WeixinMiniGame";
#else
        manifestABRoot = AutoStreaming.CustomCloudAssetsRoot + "WebGL";
#endif
#elif UNITY_ANDROID
        manifestABRoot = AutoStreaming.CustomCloudAssetsRoot + "Android";
#endif
        StartCoroutine(GetABManifest());
    }

    IEnumerator GetABManifest()
    {
        bool manifestNeedRedownload = false;
        int retryLoadCount = 3;
        do
        {
            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(manifestABRoot, 0))
            {
                yield return uwr.SendWebRequest();
                if (!string.IsNullOrEmpty(uwr.error))
                {
                    manifestNeedRedownload = true;
                    Debug.LogError(uwr.error);
                }
                else
                {
                    AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    assetBundleManifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                    if (assetBundleManifest == null)
                        Debug.LogError("Failed to load AssetBundleManifest from AssetBundle");

                    bundle.Unload(false);
                }
            }
        } while (manifestNeedRedownload && --retryLoadCount > 0);

        var dds = ABFactory.instance;
        manifestLoaded?.Invoke();
    }

    public Hash128 GetBundleHash(string bundlename)
    {
        if (assetBundleManifest == null)
            Debug.LogError($"Try to GetBundleHash {bundlename}, but assetBundleManifest is null");

        return assetBundleManifest.GetAssetBundleHash(bundlename);
    }

    public string[] GetBundleDependency(string bundlename)
    {
        var dependencies = assetBundleManifest.GetAllDependencies(bundlename);
        if (dependencies == null)
        {
            Debug.LogError($"Try to GetAllDependencies with {bundlename}, but its dependencies are null");
            return new string[2] { "boatshare", "fonts" };
        }

        return dependencies;
    }

    public string GetBoatABNameFromIndex(int index)
    {
        if (index == 0)
            return "Interceptor";

        if (index == 1)
            return "Renegade";

        return "Boat" + (index - 1).ToString();
    }

    public string GetBoatPrefabNameFromIndex(int index, bool inRace = false)
    {
        if (!inRace)
            return GetBoatABNameFromIndex(index) + "_show";

        return "_" + GetBoatABNameFromIndex(index);
    }
}