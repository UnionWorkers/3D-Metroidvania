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
        if (conveyorBeltMesh == null)
        {
            conveyorBeltMesh = GetComponent<MeshRenderer>();
            if (conveyorBeltMesh == null)
            {
                Debug.LogError("Conveyor Belt has no assigned Mesh Renderer");
                EntityState = EntityState.Disabled;
                return;
            }
        }

        previousGameSpeed = GameManager.Instance.GameSpeed;
        currentVectorOffset = conveyorBeltMesh.material.GetVector("_Offset");
    }

    public override void OnFixedUpdate(float gameSpeed)
    {
        if (previousGameSpeed != gameSpeed)
        {
            previousGameSpeed = gameSpeed;
        }

        currentVectorOffset += new Vector2(0, 1) * (CurrentVelocity * visualSpeedOffset * Time.fixedDeltaTime);
        conveyorBeltMesh.material.SetVector("_Offset", currentVectorOffset);
    }
}
