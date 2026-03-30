using Entities;
using Managers;
using UnityEngine;

public class SpickGuyController : BaseEntity
{
    [SerializeField] private DamageStruct damageStruct;
    [SerializeField] private Transform playerTransform;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            IHealth healthComponent = collision.GetComponent<IHealth>();
            healthComponent.TakeDamage(damageStruct.DamageAmount);
        }
    }

    public override void OnInitialize()
    {
        playerTransform = GameManager.Instance.PlayerController.GetTransform;
    }

    public override void OnUpdate()
    {
        transform.forward = playerTransform.position - transform.position;
    }
}
