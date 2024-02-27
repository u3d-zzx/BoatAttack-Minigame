using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;

namespace BoatAttack
{
    [CustomEditor(typeof(WaypointGroup))]
    public class WaypointGroupEditor : Editor
    {
        private WaypointGroup _wpGroup;
        private SerializedProperty _waypoints;
        private ReorderableList _waypointList;
        private int _selectedWp = -1;
        private bool _wpHeaderBool;
        private const int _airWallLength = 10;

        private void OnEnable()
        {
            if (_wpGroup == null)
                _wpGroup = (WaypointGroup)target;

            _waypoints = serializedObject.FindProperty("waypoints");
            _waypointList = new ReorderableList(serializedObject, _waypoints)
            {
                drawElementCallback = DrawElementCallback,
                drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Waypoints"); },
                onSelectCallback = list => { _selectedWp = list.index; },
                elementHeightCallback = index => EditorGUI.GetPropertyHeight(_waypoints.GetArrayElementAtIndex(index))
            };
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var prop = _waypointList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, prop);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "waypoints");

            EditorGUI.BeginChangeCheck();
            _wpHeaderBool = EditorGUILayout.BeginFoldoutHeaderGroup(_wpHeaderBool, "Waypoint List");
            if (_wpHeaderBool)
            {
                EditorGUILayout.Space();
                _waypointList.DoLayoutList();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            if (EditorGUI.EndChangeCheck())
            {
                var len = _wpGroup.CalculateTrackDistance();
                _wpGroup.length = len;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (_wpGroup == null)
                _wpGroup = (WaypointGroup)target;

            for (var i = 0; i < _wpGroup.waypoints.Count; i++)
            {
                var wp = _wpGroup.waypoints[i];
                Handles.color = _wpGroup.waypointColour;

                #region Control

                if (_selectedWp == i)
                {
                    if (Tools.current == Tool.Move)
                    {
                        // Control handle
                        EditorGUI.BeginChangeCheck();
                        var pos = Handles.PositionHandle(wp.point, wp.rotation);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(_wpGroup, "Moved Waypoint");
                            pos = new Vector3((float)Math.Round(pos.x, 2), (float)Math.Round(pos.y, 2),
                                (float)Math.Round(pos.z, 2));
                            wp.point = pos;
                        }
                    }
                    else if (Tools.current == Tool.Rotate)
                    {
                        // Control handle
                        EditorGUI.BeginChangeCheck();
                        var rot = Handles.RotationHandle(wp.rotation, wp.point);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(_wpGroup, "Rotated Waypoint");
                            wp.rotation = Quaternion.Euler(0f, Mathf.Round(rot.eulerAngles.y), 0f);
                        }
                    }
                    else if (Tools.current == Tool.Scale)
                    {
                        // Control handle
                        EditorGUI.BeginChangeCheck();
                        var scale = Handles.ScaleSlider(wp.width, wp.point, (wp.rotation * Vector3.right),
                            wp.rotation, wp.width, 0.1f);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(_wpGroup, "Scaled Waypoint");
                            wp.width = scale;
                        }
                    }
                }

                #endregion

                #region Visualization

                // Draw lines
                var a = wp;
                var b = i != _wpGroup.waypoints.Count - 1 ? _wpGroup.waypoints[i + 1] : _wpGroup.waypoints[0];


                var aMatrix = Matrix4x4.Rotate(a.rotation);
                Vector3 a0 = a.point;
                Vector3 b0 = b.point;
                float length = Vector3.Distance(a0, b0);
                float weight = a.width / (a.width + b.width);
                Vector3 a1 = aMatrix * Vector3.right * a.width;
                Vector3 a2 = aMatrix * Vector3.left * a.width;
                ;
                Vector3 af = aMatrix * Vector3.forward * Vector3.Distance(a0, b0) * weight;

                a1 = a1 + a0;
                a2 = a2 + a0;

                var bMatrix = Matrix4x4.Rotate(b.rotation);
                Vector3 b1 = bMatrix * Vector3.right * b.width;
                Vector3 b2 = bMatrix * Vector3.left * b.width;
                Vector3 bf = bMatrix * Vector3.forward * Vector3.Distance(a0, b0) * (1 - weight);
                b1 = b1 + b0;
                b2 = b2 + b0;

                Vector3 a1f = aMatrix * Vector3.forward * Vector3.Distance(a1, b1) * weight;
                Vector3 a2f = aMatrix * Vector3.forward * Vector3.Distance(a2, b2) * weight;
                Vector3 b1f = bMatrix * Vector3.forward * Vector3.Distance(a1, b1) * (1 - weight);
                Vector3 b2f = bMatrix * Vector3.forward * Vector3.Distance(a2, b2) * (1 - weight);

                Vector3 x0 = a0;
                Vector3 x1 = a1;
                Vector3 x2 = a2;

                var col = _wpGroup.waypointColour;
                col.a = 0.05f;

                for (int j = 0; j < _wpGroup.waypointstep + 1; ++j)
                {
                    float invt = j * (1.0f / _wpGroup.waypointstep);
                    float t = 1 - invt;
                    Vector3 y0 = a0 * invt * invt * invt + (a0 + af) * 3 * t * invt * invt +
                                 (b0 - bf) * 3 * t * t * invt + b0 * t * t * t;
                    Vector3 y1 = a1 * invt * invt * invt + (a1 + a1f) * 3 * t * invt * invt +
                                 (b1 - b1f) * 3 * t * t * invt + b1 * t * t * t;
                    Vector3 y2 = a2 * invt * invt * invt + (a2 + a2f) * 3 * t * invt * invt +
                                 (b2 - b2f) * 3 * t * t * invt + b2 * t * t * t;
                    if (j > 0)
                    {
                        Handles.color = col;
                        Handles.DrawDottedLine(x0, y0, 4f);
                        Handles.DrawLine(x1, y1);
                        Handles.DrawLine(x2, y2);
                        Handles.color = col;
                        Handles.DrawAAConvexPolygon(x1,
                            x2,
                            y2,
                            y1);
                    }

                    x0 = y0;
                    x1 = y1;
                    x2 = y2;
                }


                // Draw points
                var p = wp.point;
                var r = wp.rotation;
                var w = wp.width;

                if (i == 0)
                {
                    // Draw Start/Finish line
                    Handles.color = new Color(0f, 1f, 0f, 0.5f);
                }
                else if (wp.isCheckpoint)
                {
                    // Draw Checkpoints
                    Handles.color = new Color(0f, 0f, 1f, 0.5f);
                }

                DrawRectangle(p, r, new Vector2(w, 1f));

                #endregion
            }
        }

        private void DrawRectangle(Vector3 center, Quaternion rotation, Vector2 size)
        {
            var m = Matrix4x4.Rotate(rotation);
            Vector3 a = m * new Vector3(size.x, 0f, size.y);
            Vector3 b = m * new Vector3(size.x, 0f, -size.y);
            Handles.DrawAAConvexPolygon(center + a, center + b, center - a, center - b);
        }

        [MenuItem("Utilities/Generate Air Wall")]
        public static void GenAirWalls()
        {
            var wps = FindObjectOfType<WaypointGroup>();
            if (wps != null)
            {
                GameObject airwallprefab =
                    AssetDatabase.LoadAssetAtPath("Assets/Objects/AirWall.prefab", typeof(GameObject)) as GameObject;

                if (airwallprefab == null)
                {
                    Debug.LogError("Assets/Objects/AirWall.prefab 没找到");
                    return;
                }
                else
                    Debug.LogError("找到目标了");

                {
                    int count = wps.transform.childCount;
                    for (int i = 0; i < count; i++)
                    {
                        DestroyImmediate(wps.transform.GetChild(0).gameObject);
                    }
                }
                int airwallcount = 0;
                int step = 1;
                for (var i = 0; i < wps.waypoints.Count; i++)
                {
                    var a = wps.waypoints[i];
                    var b = i != wps.waypoints.Count - 1 ? wps.waypoints[i + 1] : wps.waypoints[0];


                    var aMatrix = Matrix4x4.Rotate(a.rotation);
                    Vector3 a0 = a.point;
                    Vector3 b0 = b.point;
                    float length = Vector3.Distance(a0, b0);
                    float weight = a.width / (a.width + b.width);
                    Vector3 a1 = aMatrix * Vector3.right * a.width;
                    Vector3 a2 = aMatrix * Vector3.left * a.width;
                    ;

                    a1 = a1 + a0;
                    a2 = a2 + a0;

                    var bMatrix = Matrix4x4.Rotate(b.rotation);
                    Vector3 b1 = bMatrix * Vector3.right * b.width;
                    Vector3 b2 = bMatrix * Vector3.left * b.width;
                    b1 = b1 + b0;
                    b2 = b2 + b0;

                    Vector3 a1f = aMatrix * Vector3.forward * Vector3.Distance(a1, b1) * weight;
                    Vector3 a2f = aMatrix * Vector3.forward * Vector3.Distance(a2, b2) * weight;
                    Vector3 b1f = bMatrix * Vector3.forward * Vector3.Distance(a1, b1) * (1 - weight);
                    Vector3 b2f = bMatrix * Vector3.forward * Vector3.Distance(a2, b2) * (1 - weight);

                    Vector3 x1 = a1;
                    Vector3 x2 = a2;

                    step = (int)(Vector3.Distance(a1, b1) / _airWallLength) + 1;
                    for (int j = 0; j < step + 1; ++j)
                    {
                        float invt = j * (1.0f / step);
                        float t = 1 - invt;
                        Vector3 y1 = a1 * invt * invt * invt + (a1 + a1f) * 3 * t * invt * invt +
                                     (b1 - b1f) * 3 * t * t * invt + b1 * t * t * t;
                        if (j > 0)
                        {
                            var obj1 = Instantiate(airwallprefab, wps.transform);
                            float dis = SetPos(obj1, x1, y1);
                            var aw1 = obj1.GetComponent<AirWall>();
                            aw1.Init(dis);
                            obj1.name = "AirWall_" + airwallcount;
                            airwallcount++;
                        }

                        x1 = y1;
                    }

                    step = (int)(Vector3.Distance(a2, b2) / _airWallLength) + 1;
                    for (int j = 0; j < step + 1; ++j)
                    {
                        float invt = j * (1.0f / step);
                        float t = 1 - invt;
                        Vector3 y2 = a2 * invt * invt * invt + (a2 + a2f) * 3 * t * invt * invt +
                                     (b2 - b2f) * 3 * t * t * invt + b2 * t * t * t;
                        if (j > 0)
                        {
                            var obj2 = Instantiate(airwallprefab, wps.transform);
                            float dis = SetPos(obj2, y2, x2);
                            var aw2 = obj2.GetComponent<AirWall>();
                            aw2.Init(dis);
                            obj2.name = "AirWall_" + airwallcount;
                            airwallcount++;
                        }

                        x2 = y2;
                    }
                }

                Debug.LogError("Generate AirWall number:" + airwallcount);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        private static float SetPos(GameObject obj, Vector3 x, Vector3 y)
        {
            var pos = (x + y) * 0.5f;
            pos.y = 0;
            obj.transform.position = pos;
            obj.transform.up = Vector3.up;
            var dir = x - y;
            dir.y = 0;
            obj.transform.right = dir.normalized;
            return dir.magnitude;
        }
    }
}