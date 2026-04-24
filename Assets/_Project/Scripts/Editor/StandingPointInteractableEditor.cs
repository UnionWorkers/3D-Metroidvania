using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StandingPointInteractable), true)]
public class StandingPointInteractableEditor : Editor
{
    private void OnSceneGUI()
    {
        StandingPointInteractable magnetObject = target as StandingPointInteractable;

        Vector3 StandingPoint = magnetObject.StandingPoint;

        EditorGUI.BeginChangeCheck();

        Vector3 newStandingPoint = Handles.PositionHandle(StandingPoint, Quaternion.identity);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed StandingPoint Values");
            magnetObject.StandingPoint = newStandingPoint;
    
            magnetObject.Validate();
        }

        StandingPoint += magnetObject.OffsetPlayer;

        Handles.color = new Color(0.0f, 1.0f, 0.0f);
        Handles.SphereHandleCap(0, StandingPoint, Quaternion.identity, 0.2f, EventType.Repaint);

        Handles.color = new Color(0.0f, 0.1f, 1.0f);
        Handles.Label(StandingPoint + (Vector3.up * 0.3f), $"Player Point");


    }
}
