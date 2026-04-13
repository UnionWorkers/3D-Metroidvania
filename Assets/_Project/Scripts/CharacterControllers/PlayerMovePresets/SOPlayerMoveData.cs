using CustomCharacterController;
using UnityEngine;

[CreateAssetMenu(fileName = "SOPlayerMoveData", menuName = "Scriptable Objects/SOPlayerMoveData")]
public class SOPlayerMoveData : ScriptableObject
{
    [SerializeField] private MoveStats moveStats;
    public MoveStats MoveStats
    {
        get => moveStats;
        set => moveStats = value;
    }
}
