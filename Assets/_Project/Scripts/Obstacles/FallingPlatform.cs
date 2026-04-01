using UnityEngine;
using Managers;

public class FallingPlatform : MovingObject
{
    enum MoveState
    {
        None,
        PlayerEntered,
        Falling,
        Stagnant,
        Reset
    }

    private MoveState moveState = MoveState.None;
    private Vector3 defaultRotation;
    [Space(15)]
    [SerializeField] private float fallCountdown = 4f;
    private float currentFallCountdown;

    [SerializeField] private float resetTimer = 3f;
    private float currentResetTimer;

    [SerializeField] private float bootedFallSpeed = 2f;
    private float currentBootedFallSpeed = 1f;

    protected override float realMaxVelocity => maxVelocity * currentBootedFallSpeed * GameManager.Instance.ObjectsGameSpeed;


    public override void OnInitialize()
    {
        base.OnInitialize();
        currentResetTimer = resetTimer;
        currentFallCountdown = fallCountdown;
        moveState = MoveState.None;
        defaultRotation = transform.eulerAngles;
    }

    private void NextState()
    {
        int index = (int)moveState;
        index++;
        moveState = (MoveState)index;
    }

    public override void OnFixedUpdate()
    {
        switch (moveState)
        {
            case MoveState.PlayerEntered:

                if (currentFallCountdown < 0)
                {
                    currentBootedFallSpeed = bootedFallSpeed;
                    transform.eulerAngles = defaultRotation;
                    NextState();
                }
                else
                {
                    Vector3 eulorAngels = transform.eulerAngles;

                    eulorAngels.x -= Random.Range(-0.99f, 1f);
                    eulorAngels.z -= Random.Range(-0.99f, 1f);
                    eulorAngels.y -= Random.Range(-0.99f, 1f);

                    eulorAngels = new(Clamp(eulorAngels.x), Clamp(eulorAngels.y), Clamp(eulorAngels.z));
                    transform.eulerAngles = eulorAngels;
                    
                    currentFallCountdown -= Time.fixedDeltaTime * GameManager.Instance.ObjectsGameSpeed;
                }

                break;
            case MoveState.Falling:

                if (MoveToCompletion())
                {
                    NextState();
                }

                break;
            case MoveState.Stagnant:

                if (currentResetTimer < 0)
                {
                    currentBootedFallSpeed = 1f;
                    NextState();
                }
                else
                {
                    currentResetTimer -= Time.fixedDeltaTime * GameManager.Instance.ObjectsGameSpeed;
                }

                break;
            case MoveState.Reset:

                if (MoveToCompletion())
                {
                    currentResetTimer = resetTimer;
                    currentFallCountdown = fallCountdown;
                    moveState = MoveState.None;
                }

                break;
        }
    }

    private bool MoveToCompletion()
    {
        if (currentVelocity < realMaxVelocity)
        {
            currentVelocity += (moveSpeed * currentBootedFallSpeed) * Time.fixedDeltaTime * GameManager.Instance.ObjectsGameSpeed;
        }

        float dist = Vector3.Distance(transform.position, desiredMovePoint);

        if (dist > 0.15f)
        {
            Debug.Log(transform.position);
            transform.position += currentVelocity * Time.fixedDeltaTime * currentMoveDirection;
            return false;
        }
        else
        {
            currentMoveDirection = -currentMoveDirection;
            desiredMovePoint = NewDesiredMovePoint();
            currentVelocity = 0;
            return true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (moveState == MoveState.None)
            {
                moveState = MoveState.PlayerEntered;
            }
        }
    }
}
