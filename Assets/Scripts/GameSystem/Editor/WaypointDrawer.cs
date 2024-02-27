using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using static BoatAttack.WaypointGroup;

[CustomPropertyDrawer(typeof(Waypoint))]
public class WaypointDrawer : PropertyDrawer
{
    private SerializedProperty _posProp;
    private SerializedProperty _rotProp;
    private SerializedProperty _numProp;
    private SerializedProperty _widthProp;
    private SerializedProperty _checkProp;

    private readonly float _vertLine = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
    private readonly float _vertHeight = EditorGUIUtility.singleLineHeight;

    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(rect, GUIContent.none, property);
        var labelWidth = EditorGUIUtility.labelWidth;

        _posProp = property.FindPropertyRelative("point");
        _rotProp = property.FindPropertyRelative("rotation");
        _numProp = property.FindPropertyRelative("index");
        _widthProp = property.FindPropertyRelative("width");
        _checkProp = property.FindPropertyRelative("isCheckpoint");

        var firstLine = rect.y + EditorGUIUtility.standardVerticalSpacing;
        var secondLine = firstLine + _vertLine;
        Rect numRect = new Rect(rect.x, firstLine, 25, _vertHeight);
        var dynamicWidth = rect.width - numRect.width;
        Rect posRect = new Rect(rect.x + numRect.width + 4, firstLine, dynamicWidth * 0.666f - 4, _vertHeight);
        Rect rotRect = new Rect(posRect.x + posRect.width + 4, firstLine, dynamicWidth * 0.333f - 4, _vertHeight);
        Rect widthRect = new Rect(rect.x, secondLine, rect.width * 0.5f, _vertHeight);
        Rect checkRect = new Rect(rect.x + widthRect.width + 10, secondLine, rect.width * 0.5f - 10, _vertHeight);

        // get array number
        GUI.Button(numRect, label.text.Split(' ')[1]);

        float3 rawPos = _posProp.vector3Value;
        EditorGUI.BeginChangeCheck();
        rawPos.xz = EditorGUI.Vector2Field(posRect, GUIContent.none, rawPos.xz);
        if (EditorGUI.EndChangeCheck())
        {
            _posProp.vector3Value = math.round(rawPos * 100f) / 100f;
            ;
        }

        Quaternion rawRot = _rotProp.quaternionValue;
        var rot = rawRot.eulerAngles;
        EditorGUI.BeginChangeCheck();
        EditorGUIUtility.labelWidth = 12;
        rot.y = EditorGUI.FloatField(rotRect, "H", rot.y);
        if (EditorGUI.EndChangeCheck())
        {
            _rotProp.quaternionValue = Quaternion.Euler(0f, math.round(rot.y), 0f);
        }

        EditorGUIUtility.labelWidth = 60;
        EditorGUI.PropertyField(widthRect, _widthProp,
            new GUIContent("Width",
                "Width of this waypoint, AI boats will keep within this and checkpoint gates will scale to this width."));
        _checkProp.boolValue = EditorGUI.ToggleLeft(checkRect,
            new GUIContent("Checkpoint", "Is this waypoint a checkpoint in game."), _checkProp.boolValue);

        EditorGUIUtility.labelWidth = labelWidth;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return _vertLine * 2f + EditorGUIUtility.standardVerticalSpacing;
    }
}