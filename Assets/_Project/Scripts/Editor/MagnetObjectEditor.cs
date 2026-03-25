using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(MagnetObjectInteractable), true)]
public class MagnetObjectEditor : Editor
{
    private void OnSceneGUI()
    {
        MagnetObjectInteractable magnetObject = target as MagnetObjectInteractable;

        Vector3 startPos = magnetObject.StartPoint;
        Vector3 endPos = magnetObject.EndPoint;


        magnetObject.StartPoint = Handles.PositionHandle(startPos, Quaternion.identity);
        magnetObject.EndPoint = Handles.PositionHandle(endPos, Quaternion.identity);

        Handles.color = new Color(0.0f, 0.1f, 1.0f);

        Handles.Label(startPos + (Vector3.up * 0.3f), $"Start Point");
        Handles.Label(endPos + (Vector3.up * 0.3f), $"End Point");

        Handles.color = new Color(0.3f, 1.0f, 0.3f);
        Handles.DrawLine(startPos, endPos, 5.0f);

        magnetObject.Validate();

    }


}

