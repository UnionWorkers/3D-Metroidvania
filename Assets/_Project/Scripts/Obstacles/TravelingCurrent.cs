using Entities;
using Managers;
using UnityEngine;
using UnityEngine.Splines;

public class TravelingCurrent : BaseEntity
{

    [SerializeField] private DamageStruct damageStruct;
    [SerializeField] private SplineAnimate splineAnimate = null;
    [SerializeField] private float maxSpeed = 4f;
    private float previousGameSpeed = 1f;
    private float previousNormalizedSpeed = 1f;


    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            IHealth playerHealth = collision.GetComponent<IHealth>();
            playerHealth.TakeDamage(new(damageStruct.DamageAmount, transform));
        }
    }


    public override void OnInitialize()
    {
        splineAnimate = GetComponent<SplineAnimate>();

        previousGameSpeed = GameManager.Instance.ObjectsGameSpeed;
        previousNormalizedSpeed = splineAnimate.NormalizedTime;
        splineAnimate.MaxSpeed = maxSpeed * previousGameSpeed;

        GameManager.Instance.OnGameStateChanged += GameStateChanged;

    }

    private void GameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Running:
                if (!splineAnimate.IsPlaying)
                {
                    splineAnimate.Play();
                }
                break;
            case GameState.Paused:
                if (splineAnimate.IsPlaying)
                {
                    splineAnimate.Pause();
                }
                break;
        }
    }
    public override void OnUpdate()
    {
        if (previousGameSpeed != GameManager.Instance.ObjectsGameSpeed)
        {
            previousNormalizedSpeed = splineAnimate.NormalizedTime;
            previousGameSpeed = GameManager.Instance.ObjectsGameSpeed;
            splineAnimate.MaxSpeed = maxSpeed * previousGameSpeed;
            splineAnimate.NormalizedTime = previousNormalizedSpeed;
        }
    }
}
