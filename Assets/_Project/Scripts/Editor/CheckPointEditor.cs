using UnityEditor;
using UnityEngine;
using Utils.Checkpoint;

[CustomEditor(typeof(CheckPoint), true)]
public class CheckPointEditor : Editor
{
    private void OnSceneGUI()
    {
        CheckPoint checkPoint = target as CheckPoint;

        checkPoint.SpawnPoint = Handles.PositionHandle(checkPoint.SpawnPoint + checkPoint.transform.position, Quaternion.identity) - checkPoint.transform.position;
    }
}
