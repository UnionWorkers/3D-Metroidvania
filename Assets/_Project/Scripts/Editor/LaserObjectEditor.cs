using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LaserPoleController), true)]
public class LaserObjectEditor : Editor
{
    private void OnSceneGUI()
    {
        LaserPoleController laserObject = target as LaserPoleController;

        Vector3 startPos = laserObject.StartPoint;
        Vector3 endPos = laserObject.EndPoint;

        laserObject.StartPoint = Handles.PositionHandle(startPos, Quaternion.identity);
        laserObject.EndPoint = Handles.PositionHandle(endPos, Quaternion.identity);

        Handles.Label(startPos + (Vector3.up * 0.5f), $"Start Point");
        Handles.Label(endPos + (Vector3.up * 0.5f), $"End Point");

        Handles.color = new Color(0.3f, 1.0f, 0.3f);
        Handles.DrawLine(startPos, endPos, 5.0f);

    }

    public override void OnInspectorGUI()
    {
        LaserPoleController laserController = target as LaserPoleController;

        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();

        GUILayout.Space(10);
        if (GUILayout.Button("Spawn lasers"))
        {
            laserController.Validate();
        }
    }


}
