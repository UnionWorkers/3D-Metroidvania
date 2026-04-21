using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ClimbableInteractable), true)]
public class ClimbableInteractableEditor : Editor
{
    private void OnSceneGUI()
    {
        ClimbableInteractable magnetObject = target as ClimbableInteractable;

        Vector3 startPos = magnetObject.StartPoint;
        Vector3 endPoint = magnetObject.EndPoint;

        magnetObject.StartPoint = Handles.PositionHandle(startPos, magnetObject.transform.rotation);
        magnetObject.EndPoint = Handles.PositionHandle(endPoint, magnetObject.transform.rotation);

        Handles.color = new Color(0.0f, 0.1f, 1.0f);
        Handles.Label(startPos + (Vector3.up * 0.3f), $"Start Point");
        Handles.SphereHandleCap(0, startPos, Quaternion.identity, 0.2f, EventType.Repaint);


        Handles.color = new Color(0.0f, 1.0f, 0.0f);
        Handles.Label(endPoint + (Vector3.up * 0.3f), $"End Point");
        Handles.SphereHandleCap(0, endPoint, Quaternion.identity, 0.2f, EventType.Repaint);

        Handles.color = new Color(0.3f, 1.0f, 0.3f);
        Handles.DrawLine(startPos, endPoint, 5.0f);

    }
}
