using System.IO;
using CustomCharacterController;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerCharacterController), true)]
public class PlayerControllerDataSaverEditor : Editor
{

    private string path = "Assets/_Project/Scripts/CharacterControllers/PlayerMovePresets/";
    private string defaultName = "PlayerMovementPreset";
    public override void OnInspectorGUI()
    {
        PlayerCharacterController playerCharacterController = target as PlayerCharacterController;

        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();

        GUILayout.Space(10);
        GUILayout.Label("Makes a scriptable object of the players move stats", EditorStyles.boldLabel);
        if (GUILayout.Button("Save Movement Data"))
        {
            bool bathExist = Directory.Exists(path);

            if (bathExist)
            {
                SOPlayerMoveData newAsset = ScriptableObject.CreateInstance<SOPlayerMoveData>();
                newAsset.MoveStats = playerCharacterController.MoveStats;

                if (newAsset != null)
                {
                    string assetPath = path + defaultName + ".asset";
                    assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

                    AssetDatabase.CreateAsset(newAsset, assetPath);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    EditorGUIUtility.PingObject(newAsset);
                }
            }
            else
            {
                Debug.LogError($"Cant find path: {path}");
            }
        }

        
        GUILayout.Space(15);
        
        GUILayout.Label("Read Before USING! 🧐", EditorStyles.boldLabel);
        GUILayout.Label("Will override the player move stats with the preset", EditorStyles.boldLabel);
        if (GUILayout.Button("Copy Preset"))
        {
            if (playerCharacterController.MovePreset != null)
            {
                playerCharacterController.MoveStats = playerCharacterController.MovePreset.MoveStats;
            }
            else
            {
                Debug.LogWarning("Preset is null, cant do operation");
            }
        }

        GUILayout.Space(15);
        
        GUILayout.Label("Read Before USING! 🧐", EditorStyles.boldLabel);
        GUILayout.Label("Will override the preset with the players move stats", EditorStyles.boldLabel);
        if (GUILayout.Button("Override Preset"))
        {
            if (playerCharacterController.MovePreset != null)
            {
                playerCharacterController.MovePreset.MoveStats = playerCharacterController.MoveStats;
            }
            else
            {
                Debug.LogWarning("Preset is null, cant do operation");
            }
        }

    }


}
