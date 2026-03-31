using UnityEditor;
using UnityEngine;
using Utils.Checkpoint;

[CustomEditor(typeof(CheckPoint), true)]
public class CheckPointEditor : Editor
{
    private void OnSceneGUI()
    {
        CheckPoint checkPoint = target as CheckPoint;

        Vector3 spawnPos = checkPoint.SpawnPoint + checkPoint.transform.position;

        checkPoint.SpawnPoint = Handles.PositionHandle(spawnPos, Quaternion.identity) - checkPoint.transform.position;

        Handles.color = new Color(0.3f, 1.0f, 0.3f);
        Handles.SphereHandleCap(0, spawnPos, Quaternion.identity, 0.7f, EventType.Repaint);

        Handles.Label(spawnPos + Vector3.up * 1f, "Check point spawn point");



    }
}
