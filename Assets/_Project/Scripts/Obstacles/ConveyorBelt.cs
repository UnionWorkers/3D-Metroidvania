using Entities;
using Managers;
using UnityEngine;

public class ConveyorBelt : BaseEntity
{
    [SerializeField] private float conveyorSpeed;
    [SerializeField] private float visualSpeedOffset;
    [SerializeField] private MeshRenderer conveyorBeltMesh;

    private float previousGameSpeed;
    private Vector2 currentVectorOffset;

    public float CurrentVelocity => conveyorSpeed * previousGameSpeed;
    public Vector3 CurrentMoveDirection => transform.forward * Mathf.Clamp(conveyorSpeed, -1, 1);

    public override void OnInitialize()
    {
        previousGameSpeed = GameManager.Instance.ObjectsGameSpeed;
        currentVectorOffset = conveyorBeltMesh.material.GetVector("_Offset");
    }

    public override void OnUpdate()
    {
        if (previousGameSpeed != GameManager.Instance.ObjectsGameSpeed)
        {
            previousGameSpeed = GameManager.Instance.ObjectsGameSpeed;
        }

        currentVectorOffset += new Vector2(0, 1) * (CurrentVelocity * visualSpeedOffset * Time.deltaTime);
        conveyorBeltMesh.material.SetVector("_Offset", currentVectorOffset);
    }
}
