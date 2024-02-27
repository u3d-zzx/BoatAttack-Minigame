using System.IO;
using UnityEditor;
using UnityEngine;

public class DevelopUtil : EditorWindow
{

    [MenuItem("Utilities/Clean PersistentDataPath")]
    static void ClearPersistentData()
    {
        var folderPath = Application.persistentDataPath;
        foreach (var directory in (new DirectoryInfo(folderPath).GetDirectories()))
        {

            directory.Delete(true);
        }

        foreach (var file in (new DirectoryInfo(folderPath).GetFiles()))
        {
            file.Attributes = FileAttributes.Normal;
            file.Delete();
        }

        AssetDatabase.Refresh();
    }

    [MenuItem("Utilities/Clean StreamingAssetsPath")]
    static void ClearStreamingAssetData()
    {
        var folderPath = Application.streamingAssetsPath;
        foreach (var directory in (new DirectoryInfo(folderPath).GetDirectories()))
        {
            directory.Delete(true);
        }

        foreach (var file in (new DirectoryInfo(folderPath).GetFiles()))
        {
            file.Attributes = FileAttributes.Normal;
            file.Delete();
        }

        AssetDatabase.Refresh();
    }

    [MenuItem("Utilities/Clean PlayerPrefData")]
    static void ClearPlayerPrefsData()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("Utilities/Update CustomCloudAssetPath")]
    static void CopyABToCustomCloudAssetPath()
    {
        string abDirectoryPath= Path.GetFullPath("AssetBundles");
        string abCloudAssetsPath= Path.GetFullPath("CustomCloudAssets");

        Directory.CreateDirectory(abCloudAssetsPath);

        foreach (var abDirectory in (Directory.GetDirectories(abDirectoryPath)))
        {
            foreach (var abFile in (Directory.GetFiles(abDirectory)))
            {
                string destPath = abFile.Replace(abDirectory, abCloudAssetsPath);
                string destDir = Path.GetDirectoryName(destPath);
                Directory.CreateDirectory(destDir);

                File.Copy(abFile, destPath, true);
            }
        }
    }

    [MenuItem("Utilities/Build WXAssetBundles")]
    static void BuildWXAssetBundles()
    {
        string abDirectoryPath= Path.GetFullPath("AssetBundles");
        if (!Directory.Exists(abDirectoryPath))
            Directory.CreateDirectory(abDirectoryPath);
        var buildTarget = EditorUserBuildSettings.activeBuildTarget;
        var buildPath = Path.Combine(abDirectoryPath, buildTarget.ToString());
        if (!Directory.Exists(buildPath))
            Directory.CreateDirectory(buildPath);

        AssetBundleBuild[] assetBundles = new AssetBundleBuild[12];
        assetBundles[0].assetBundleName = "boatshare";
        assetBundles[0].assetNames = new string[]
        {
            "Assets/Materials/_waterFoam.mat",
            "Assets/Materials/Splash.mat",
            //"Assets/Shaders/UtilityGraphs/RaceBoats.ShaderGraph",
            "Assets/Objects/boats/renegade/Renegade.mat",
            "Assets/Objects/boats/Interceptor/Interceptor.mat"
        };

        assetBundles[1].assetBundleName = "wake";
        assetBundles[1].assetNames = new string[]
        {
            "Assets/Objects/misc/Wake.prefab",
        };

        assetBundles[2].assetBundleName = "renegade";
        assetBundles[2].assetNames = new string[]
        {
            "Assets/Objects/boats/renegade/_Renegade.prefab",
            "Assets/Objects/boats/renegade/Renegade_show.prefab"
        };

        assetBundles[3].assetBundleName = "fonts";
        assetBundles[3].assetNames = new string[]
        {
            "Assets/Fonts/SourceHanSansCN-Medium SDF.asset",
        };

        assetBundles[4].assetBundleName = "interceptor";
        assetBundles[4].assetNames = new string[]
        {
            "Assets/Objects/boats/Interceptor/_Interceptor.prefab",
             "Assets/Objects/boats/Interceptor/Interceptor_show.prefab",
        };

        assetBundles[5].assetBundleName = "checkpoint";
        assetBundles[5].assetNames = new string[]
        {
            "Assets/Objects/misc/checkpoint.prefab",
        };

        assetBundles[6].assetBundleName = "defaultvolume";
        assetBundles[6].assetNames = new string[]
        {
            "Assets/Objects/misc/DefaultVolume.prefab",
        };

        assetBundles[7].assetBundleName = "ui_loadingscreen";
        assetBundles[7].assetNames = new string[]
        {
            "Assets/Objects/UI/prefabs/LoadingScreen.prefab",
        };

        assetBundles[8].assetBundleName = "ui_racestats_player";
        assetBundles[8].assetNames = new string[]
        {
            "Assets/Objects/UI/prefabs/RaceStats_Player.prefab",
        };

        assetBundles[9].assetBundleName = "ui_race_canvas_touch";
        assetBundles[9].assetNames = new string[]
        {
            "Assets/Objects/UI/prefabs/Race_Canvas_touch.prefab",
        };

        assetBundles[10].assetBundleName = "ui_playermarker";
        assetBundles[10].assetNames = new string[]
        {
            "Assets/Objects/UI/prefabs/PlayerMarker.prefab",
        };

        assetBundles[11].assetBundleName = "ui_playermapmarker";
        assetBundles[11].assetNames = new string[]
        {
            "Assets/Objects/UI/prefabs/PlayerMapMarker.prefab",
        };


        var buildManifest = BuildPipeline.BuildAssetBundles(buildPath, assetBundles,
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle,
            buildTarget);
        if (buildManifest != null)
        {
            UnityEngine.Debug.Log($"Build AssetBundle successfully with Bundle Count: {buildManifest.GetAllAssetBundles().Length}");
        }


        //copy all assetbundle files to CustomCloudAssets folder if needed
        if (AutoStreaming.autoStreaming) 
        {
            string customCoudAssetsDir = Path.GetFullPath("CustomCloudAssets");
            if (!Directory.Exists(customCoudAssetsDir))
                Directory.CreateDirectory(customCoudAssetsDir);

            DirectoryInfo dirInfo = new DirectoryInfo(buildPath);
            foreach (var file in dirInfo.EnumerateFiles())
            {
                file.CopyTo(Path.Combine(customCoudAssetsDir,file.Name), true);
            }
        }
    }
    
    [MenuItem("Utilities/Disable ReadWrite for Models")]
    static void DisableReadWriteForAllModels()
    {
        string[] guids = AssetDatabase.FindAssets("t:Model");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ModelImporter modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;

            if (modelImporter != null && modelImporter.isReadable)
            {
                modelImporter.isReadable = false;
                AssetDatabase.ImportAsset(path);
            }
        }

        Debug.Log("Read/Write disabled for eligible models in the project.");
    }
}
