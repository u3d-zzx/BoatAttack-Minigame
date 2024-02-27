using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEditor;

public class ABFactory : MonoBehaviour
{
    private static ABFactory _instance = null;
    private bool _isloading = false;
    private Dictionary<string, AssetBundle> loadedAssetBundles = new Dictionary<string, AssetBundle>();
    private Dictionary<string, AssetBundle> loadingAssetBundles = new Dictionary<String, AssetBundle>();

#if UNITY_EDITOR && EDITOR_AB_SIMULATE_MODE
    private Dictionary<string, string> prefabPaths = new Dictionary<string, string>();
#endif


    public static ABFactory instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject(nameof(ABFactory));
                _instance = go.AddComponent<ABFactory>();
                DontDestroyOnLoad(go);

#if UNITY_EDITOR && EDITOR_AB_SIMULATE_MODE
                _instance.prefabPaths.Add("Wake", "Assets/Objects/misc/Wake.prefab");
                _instance.prefabPaths.Add("_Renegade", "Assets/Objects/boats/renegade/_Renegade.prefab");
                _instance.prefabPaths.Add("Renegade_show", "Assets/Objects/boats/renegade/Renegade_show.prefab");
                _instance.prefabPaths.Add("_Interceptor", "Assets/Objects/boats/Interceptor/_Interceptor.prefab");
                _instance.prefabPaths.Add("Interceptor_show", "Assets/Objects/boats/Interceptor/Interceptor_show.prefab");
                _instance.prefabPaths.Add("checkpoint", "Assets/Objects/misc/checkpoint.prefab");
                _instance.prefabPaths.Add("DefaultVolume", "Assets/Objects/misc/DefaultVolume.prefab");
                _instance.prefabPaths.Add("LoadingScreen", "Assets/Objects/UI/prefabs/LoadingScreen.prefab");
                _instance.prefabPaths.Add("RaceStats_Player", "Assets/Objects/UI/prefabs/RaceStats_Player.prefab");
                _instance.prefabPaths.Add("Race_Canvas_touch", "Assets/Objects/UI/prefabs/Race_Canvas_touch.prefab");
                _instance.prefabPaths.Add("PlayerMarker", "Assets/Objects/UI/prefabs/PlayerMarker.prefab");
                _instance.prefabPaths.Add("PlayerMapMarker", "Assets/Objects/UI/prefabs/PlayerMapMarker.prefab");
#endif
            }
            return _instance;
        }
    }

    public IEnumerator CreateFromABAsync(string abname, string prefabname, Action<GameObject> loadAssetCallback = null)
    {
        AssetBundle ab;

#if UNITY_EDITOR
#if EDITOR_AB_SIMULATE_MODE
        if (prefabPaths.ContainsKey(prefabname))
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath(prefabPaths[prefabname], typeof(GameObject)) as GameObject;
            GameObject obj = Instantiate(prefab);
            if (loadAssetCallback != null)
                loadAssetCallback(obj);

            yield break;
        }
#endif

        string url = ABManager.localABRoot + "/" + abname.ToLower();
#else
        string url = AutoStreaming.CustomCloudAssetsRoot + abname.ToLower();
#endif

        if (!loadedAssetBundles.TryGetValue(url, out ab))
        {
            List<string> ABDependNames = new List<string>();
            ABDependNames.AddRange(ABManager.instance.GetBundleDependency(abname.ToLower()));
            ABDependNames.Add(abname.ToLower());

            foreach (var abdepnames in ABDependNames)
            {
#if UNITY_EDITOR
                string dependencyurl = ABManager.localABRoot + "/" + abdepnames;
#else
                string dependencyurl = AutoStreaming.CustomCloudAssetsRoot + abdepnames;
#endif
                while (loadingAssetBundles.ContainsKey(dependencyurl)) 
                {
                    yield return null;
                }

                if (!loadedAssetBundles.ContainsKey(dependencyurl))
                {
                    using (UnityWebRequest req = UnityWebRequestAssetBundle.GetAssetBundle(dependencyurl))
                    {
                        loadingAssetBundles.Add(dependencyurl, null);
((DownloadHandlerAssetBundle)req.downloadHandler).autoLoadAssetBundle = true;
                        yield return req.SendWebRequest();

                        if (!string.IsNullOrEmpty(req.error))
                        {
                            Debug.LogError(req.error);
                        }
                        else
                        {
                            loadedAssetBundles.Add(dependencyurl, DownloadHandlerAssetBundle.GetContent(req));
                        }
                        loadingAssetBundles.Remove(dependencyurl);
                    }
                }
            }
        }

        if (!loadedAssetBundles.TryGetValue(url, out ab))
        {
            Debug.LogError("Failed to load AssetBundle: " + url);
            yield break;
        }

        GameObject gameObj = Instantiate(ab.LoadAsset(prefabname) as GameObject);
        if (loadAssetCallback != null)
            loadAssetCallback(gameObj);
    }


    private void OnDestroy()
    {
        foreach (var kv in loadedAssetBundles) 
        {
            kv.Value.Unload(false);
        }
        loadedAssetBundles.Clear();
    }
}