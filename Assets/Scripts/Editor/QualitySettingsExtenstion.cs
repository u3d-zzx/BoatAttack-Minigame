using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

static class UniversalSettings
{
    private static class Styles
    {
        public static GUIStyle frameBox = new GUIStyle(EditorStyles.helpBox);
        public static GUIStyle header = new GUIStyle(EditorStyles.boldLabel);
        public static GUIContent menuIcon = new GUIContent((EditorGUIUtility.Load("pane options") as Texture2D));
        public static GUIContent menuItems = new GUIContent("Hello");
    }

    private static int qualityAssetIndex = QualitySettings.GetQualityLevel();
    private static int displayAssetIndex = qualityAssetIndex + 1;

    [SettingsProvider]
    public static SettingsProvider CreateUniversalSettings()
    {
        // First parameter is the path in the Settings window.
        // Second parameter is the scope of this setting: it only appears in the Project Settings window.
        var provider = new SettingsProvider("Project/Quality/Universal RP Settings", SettingsScope.Project)
        {
            // By default the last token of the path is used as display name if no label is provided.
            label = "Universal RP Settings",
            // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
            guiHandler = (searchContext) =>
            {
                Styles.header.fontSize = 16;
                var asset = GraphicsSettings.renderPipelineAsset;

                // No Pipeline asset in Graphics Settings
                if (asset == null)
                    return;

                string[] names = QualitySettings.names;
                for (int i = 0; i < names.Length; i++)
                {
                    if (QualitySettings.GetRenderPipelineAssetAt(i) == null)
                        names[i] += " (Default Fallback)";
                }

                names.CopyTo(names = new string[names.Length + 1], 1);
                names[0] = "Default Asset";

                EditorGUILayout.BeginVertical(Styles.frameBox);
                GUILayout.Label($"Quality Level: {names[displayAssetIndex]}", Styles.header);
                EditorGUI.BeginChangeCheck();
                displayAssetIndex = GUILayout.Toolbar(displayAssetIndex, names);
                if (EditorGUI.EndChangeCheck() && displayAssetIndex != 0)
                {
                    QualitySettings.SetQualityLevel(displayAssetIndex - 1);
                }

                if (displayAssetIndex != 0)
                {
                    var qualityAsset = QualitySettings.GetRenderPipelineAssetAt(displayAssetIndex - 1);
                    if (qualityAsset != null) asset = qualityAsset;
                }

                var assetPath = AssetDatabase.GetAssetPath(asset);

                EditorGUILayout.Separator();
                if (GUILayout.Button($"Path: {assetPath}"))
                {
                    EditorGUIUtility.PingObject(asset);
                }

                EditorGUILayout.Separator();

                DoRenderPipelineSettings(asset);

                EditorGUILayout.Separator();

                EditorGUILayout.EndVertical();
            },

            // Populate the search keywords to enable smart search filtering and label highlighting:
            keywords = new HashSet<string>(new[] { "Number", "Some String" })
        };

        return provider;
    }

    private static void DoRenderPipelineSettings(RenderPipelineAsset asset)
    {
        EditorGUILayout.BeginVertical(Styles.frameBox);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Render Pipeline Asset", Styles.header);
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Hello"), false, null);
        menu.AddItem(new GUIContent("Goodbye"), false, null);
        if (GUILayout.Button(Styles.menuIcon))
        {
            menu.ShowAsContext();
        }

        EditorGUILayout.EndHorizontal();

        Editor assetSettings = Editor.CreateEditor(asset);
        assetSettings.OnInspectorGUI();

        EditorGUILayout.EndVertical();
    }
}