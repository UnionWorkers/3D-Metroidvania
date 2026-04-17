using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;


public enum AnimationState : byte
{
    Grounded,
    InAir,
}

[System.Serializable]
public class PlayerAnimationController
{
    private AnimationState animationState;
    private bool canRun = true;

    [SerializeField] private float walkCooldown = 0.5f;
    private float currentWalkTimer = 0;
    private bool isWalkCooldownActive = false;

    [NonSerialized] public Animator CharacterAnimator;
    public PlayerAnimationController(Animator inCharacterAnimator = null)
    {
        CharacterAnimator = inCharacterAnimator;
    }

    public AnimationState AnimationState
    {
        get => animationState;
        set
        {
            if (animationState == value) { return; }

            // On exit state 
            switch (animationState)
            {
                case AnimationState.Grounded:
                    canRun = false;
                    break;
                case AnimationState.InAir:
                    break;

            }

            // On enter state 
            switch (value)
            {
                case AnimationState.Grounded:
                    AudioManager.Instance.PlaySFX("Land " + Random.Range(1, 3));
                    canRun = true;
                    break;
                case AnimationState.InAir:
                    break;
            }
            animationState = value;
        }
    }

    public void IsFalling(float fallSpeed)
    {
        if (fallSpeed > 0)
        {
            CharacterAnimator.SetFloat("FallSpeed", fallSpeed);
            return;
        }
        AnimationState = AnimationState.InAir;
    }

    public void SetRunAnimation(float velocity, MonoBehaviour inMonoBehaviour)
    {
        if (!canRun) { return; }

        CharacterAnimator.SetFloat("RunVelocity", velocity);

        if (velocity > 0.5 && !isWalkCooldownActive)
        {
            AudioManager.Instance.PlaySFX("Run " + Random.Range(1, 5));
            inMonoBehaviour.StartCoroutine(WalkCooldown());
        }
    }

    private IEnumerator WalkCooldown()
    {
        isWalkCooldownActive = true;
        while (currentWalkTimer < walkCooldown)
        {
            currentWalkTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        isWalkCooldownActive = false;
        currentWalkTimer = 0;
    }

    public void TriggerJumpAnimation(bool isNormalJump)
    {

        CharacterAnimator.SetTrigger("Jump");

        if (isNormalJump)
        {
            AudioManager.Instance.PlaySFX("Jump " + Random.Range(1, 5));

        }
        else
        {
            AudioManager.Instance.PlaySFX("DoubleJump " + Random.Range(1, 3));
        }
    }

    public void TriggerAttackAnimation()
    {
        CharacterAnimator.SetTrigger("Attack");
        AudioManager.Instance.PlaySFX("Attack " + Random.Range(1, 3));
    }


    public void TriggerHitAnimation()
    {
        CharacterAnimator.SetTrigger("Hit");
        AudioManager.Instance.PlaySFX("TakeDamage " + Random.Range(1, 3));
    }


}
