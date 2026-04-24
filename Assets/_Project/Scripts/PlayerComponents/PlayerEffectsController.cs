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
public class PlayerEffectsController
{
    private AnimationState animationState;
    private bool canRun = true;

    [SerializeField] private float walkCooldown = 0.5f;
    private float currentWalkTimer = 0;
    private bool isWalkCooldownActive = false;

    [NonSerialized] public Animator CharacterAnimator;
    public PlayerEffectsController(Animator inCharacterAnimator = null)
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
                    PlayerSFX("Land " + Random.Range(1, 3));
                    canRun = true;
                    break;
                case AnimationState.InAir:
                    break;
            }
            animationState = value;
        }
    }

    public void PlayerSFX(string audioName)
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager is not available cant play audio");
            return;
        }
        
        AudioManager.Instance.PlaySFX(audioName);
    }

    public void PlayerMusic(string audioName)
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager is not available cant play audio");
            return;
        }

        AudioManager.Instance.PlayMusic(audioName);
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
            PlayerSFX("Run " + Random.Range(1, 5));
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
            PlayerSFX("Jump " + Random.Range(1, 5));

        }
        else
        {
            PlayerSFX("DoubleJump " + Random.Range(1, 3));
        }
    }

    public void TriggerAttackAnimation()
    {
        CharacterAnimator.SetTrigger("Attack");
        PlayerSFX("Attack " + Random.Range(1, 3));
    }


    public void TriggerHitAnimation()
    {
        CharacterAnimator.SetTrigger("Hit");
        PlayerSFX("TakeDamage " + Random.Range(1, 3));
    }


}
