using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


[CustomEditor(typeof(RopeInteractable), true)]
public class RopeInteractableEditor : Editor
{
    private void OnSceneGUI()
    {
        RopeInteractable magnetObject = target as RopeInteractable;

        Vector3 startPos = magnetObject.StartPoint;
        Vector3 endPos = magnetObject.EndPoint;

        EditorGUI.BeginChangeCheck();


        Vector3 newStartPos = Handles.PositionHandle(startPos, Quaternion.identity);
        Vector3 newEndPos = Handles.PositionHandle(endPos, Quaternion.identity);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed Rope Values");
            magnetObject.StartPoint = newStartPos;
            magnetObject.EndPoint = newEndPos;
        }

        Handles.color = new Color(0.0f, 0.1f, 1.0f);

        Handles.Label(startPos + (Vector3.up * 0.3f), $"Start Point");
        Handles.Label(endPos + (Vector3.up * 0.3f), $"End Point");

        Handles.color = new Color(0.3f, 1.0f, 0.3f);
        Handles.DrawLine(startPos, endPos, 5.0f);

        magnetObject.Validate();
    }


}

