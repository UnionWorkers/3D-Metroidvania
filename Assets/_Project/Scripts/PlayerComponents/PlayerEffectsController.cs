using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;


public enum AnimationState : byte
{
    Grounded,
    InAir,
}

public enum GlideState : byte
{
    None,
    Active
}

public enum DashState : byte
{
    None,
    Active
}

[System.Serializable]
public class PlayerEffectsController
{
    private AnimationState animationState;
    private GlideState glideState;
    private DashState dashState;

    private bool canRun = true;

    [SerializeField] private float walkCooldown = 0.5f;
    private float currentWalkTimer = 0;
    private bool isWalkCooldownActive = false;

    [Space(15)]
    [SerializeField] private Transform doubleJumpPlatform;
    [SerializeField] private float doubleJumpCooldown = 0.3f;
    private float currentDoubleJumpTimer = 0;


    [Space(15)]
    [SerializeField] private ParticleSystem glideParticles;
    [SerializeField] private ParticleSystem landParticles;
    [SerializeField] private ParticleSystem dashParticles;


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
                    if (landParticles != null)
                    {
                        landParticles.Play();
                    }
                    canRun = true;
                    break;
                case AnimationState.InAir:
                    CharacterAnimator.SetBool("InAir", true);
                    break;
            }
            animationState = value;
        }
    }

    public GlideState GlideState
    {
        get => glideState;
        set
        {
            if (glideState == value || glideParticles == null) { return; }
            glideState = value;

            switch (glideState)
            {
                case GlideState.None:
                    glideParticles.Stop();
                    break;
                case GlideState.Active:
                    glideParticles.Play();
                    break;
            }

        }
    }

    public DashState DashState
    {
        get => dashState;
        set
        {
            if (dashState == value || dashParticles == null) { return; }
            dashState = value;

            switch (dashState)
            {
                case DashState.None:
                    dashParticles.Stop();
                    break;
                case DashState.Active:
                    dashParticles.Play();
                    break;
            }

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
