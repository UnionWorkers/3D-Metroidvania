using Entities;
using Managers;
using UnityEngine;

public class CyclingPlatform : BaseEntity
{
    [SerializeField] private Transform rotationObject;
    [SerializeField] private float rotateToStartTimer = 3f;
    [SerializeField] private float timeFromStartToFinished = 3f;
    [SerializeField] private float rotateAmount = 90f;
    [SerializeField] private Vector3 rotateDirection;

    private float currentStartTimer;
    private float currentToFinishTimer;

    public override void OnInitialize()
    {
        currentStartTimer = rotateToStartTimer;
        currentToFinishTimer = timeFromStartToFinished;
        rotateDirection = Utils.Math.MyMath.ClampVector(rotateDirection, Utils.Math.MyMath.ClampMode.ZeroToOne);
    }

    public override void OnUpdate()
    {
        if (currentStartTimer <= 0f)
        {
            if (currentToFinishTimer >= 0f)
            {
                rotationObject.Rotate(rotateDirection * (rotateAmount / timeFromStartToFinished * Time.deltaTime * GameManager.Instance.ObjectsGameSpeed));
                currentToFinishTimer -= Time.deltaTime * GameManager.Instance.ObjectsGameSpeed;
            }
            else
            {
                currentStartTimer = rotateToStartTimer;
                currentToFinishTimer = timeFromStartToFinished;
            }
        }
        else
        {
            currentStartTimer -= Time.deltaTime * GameManager.Instance.ObjectsGameSpeed;
        }
    }


}
