using UnityEngine;


public enum AnimationState : byte
{
    Grounded,
    InAir,
}


public class PlayerAnimationController
{
    private AnimationState animationState;
    private bool canRun = true;

    public Animator CharacterAnimator;
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
        AnimationState = AnimationState.InAir;
        CharacterAnimator.SetFloat("FallSpeed", fallSpeed);
    }

    public void SetRunAnimation(float velocity)
    {
        if (!canRun) { return; }

        CharacterAnimator.SetFloat("RunVelocity", velocity);
    }

    public void TriggerJumpAnimation()
    {
        CharacterAnimator.SetTrigger("Jump");
    }

    public void TriggerAttackAnimation()
    {
        CharacterAnimator.SetTrigger("Attack");
    }


    public void TriggerHitAnimation()
    {
        CharacterAnimator.SetTrigger("Hit");
    }


}
