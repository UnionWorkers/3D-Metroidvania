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

    [SerializeField] private Transform doubleJumpPlatform;
    [SerializeField] private float doubleJumpCooldown = 0.3f;
    private float currentDoubleJumpTimer = 0;

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
                    CharacterAnimator.SetBool("InAir", false);
                    break;

            }

            // On enter state 
            switch (value)
            {
                case AnimationState.Grounded:
                    CharacterAnimator.SetTrigger("Landing");
                    PlayerSFX("Land " + Random.Range(1, 3));
                    canRun = true;
                    break;
                case AnimationState.InAir:
                    CharacterAnimator.SetBool("InAir", true);
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
            return;
        }
        AnimationState = AnimationState.InAir;
    }

    public void SetRunAnimation(float velocity, MonoBehaviour inMonoBehaviour)
    {
        if (!canRun) { return; }

        CharacterAnimator.SetFloat("WalkSpeed", velocity);

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

    public void TriggerJumpAnimation(bool isNormalJump, MonoBehaviour inMonoBehaviour)
    {

        if (isNormalJump)
        {
            CharacterAnimator.SetTrigger("Jump");
            PlayerSFX("Jump " + Random.Range(1, 5));
        }
        else
        {
            if (doubleJumpPlatform != null)
            {
                doubleJumpPlatform.gameObject.SetActive(true);
                inMonoBehaviour.StartCoroutine(DoubleJumpPlatformTurnOff());
            }

            CharacterAnimator.Play("Jump", -1, 0f);
            PlayerSFX("DoubleJump " + Random.Range(1, 3));
        }
    }

    private IEnumerator DoubleJumpPlatformTurnOff()
    {
        while (currentDoubleJumpTimer < doubleJumpCooldown)
        {
            currentDoubleJumpTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        doubleJumpPlatform.gameObject.SetActive(false);
        currentDoubleJumpTimer = 0;
    }

    public void TriggerAttackAnimation()
    {
        // CharacterAnimator.SetTrigger("Attack");
        PlayerSFX("Attack " + Random.Range(1, 3));
    }


    public void TriggerHitAnimation()
    {
        // CharacterAnimator.SetTrigger("Hit");
        PlayerSFX("TakeDamage " + Random.Range(1, 3));
    }


}
