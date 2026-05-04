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
    private float previousRotation;

    public override void OnInitialize()
    {
        if (rotationObject == null)
        {
            Debug.LogError("Has not rotation object");
            EntityState = EntityState.Disabled;
            return;
        }
        currentStartTimer = rotateToStartTimer;
        currentToFinishTimer = timeFromStartToFinished;
        previousRotation = rotationObject.localEulerAngles.y;
        rotateDirection = Utils.Math.MyMath.ClampVector(rotateDirection, Utils.Math.MyMath.ClampMode.ZeroToOne);
    }

    public override void OnFixedUpdate()
    {
        if (currentStartTimer <= 0f)
        {
            if (currentToFinishTimer >= 0f)
            {
                rotationObject.localEulerAngles += rotateDirection * (rotateAmount / timeFromStartToFinished * Time.fixedDeltaTime * GameManager.Instance.ObjectsGameSpeed);
                currentToFinishTimer -= Time.fixedDeltaTime * GameManager.Instance.ObjectsGameSpeed;
            }
            else
            {
                Vector3 eulerAngles = rotationObject.localEulerAngles; 
                eulerAngles.y = previousRotation + rotateAmount;
                rotationObject.localEulerAngles = eulerAngles;
                previousRotation = rotationObject.localEulerAngles.y;
                currentStartTimer = rotateToStartTimer;
                currentToFinishTimer = timeFromStartToFinished;
            }
        }
        else
        {
            currentStartTimer -= Time.fixedDeltaTime * GameManager.Instance.ObjectsGameSpeed;
        }
    }


}
