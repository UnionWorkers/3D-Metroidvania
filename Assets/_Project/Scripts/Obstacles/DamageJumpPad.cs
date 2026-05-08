using System.Collections;
using CustomCharacterController;
using Entities.Controller;
using UnityEngine;

public class DamageJumpPad : JumpPad
{

    [SerializeField] private DamageStruct damageStruct;
    private bool canDoDamage = true;

    protected override void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerCharacterController PlayerRef = other.transform.GetComponent<PlayerCharacterController>();

            if (canDoDamage)
            {
                IHealth health = other.transform.GetComponent<IHealth>();
                health.TakeDamage(new(damageStruct.DamageAmount));
                StartCoroutine(DamageReset());
            }

            PlayerRef.AddForce(Vector3.up * jumpForce, ForceSource.JumpPad);
        }
    }

    private IEnumerator DamageReset()
    {
        float resetTimer = 1;
        float currentDamageTimer = 0;
        canDoDamage = false;

        while (currentDamageTimer < resetTimer)
        {
            currentDamageTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        canDoDamage = true;
    }

}
