using Entities.Controller;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUiHandler : MonoBehaviour
{
    [SerializeField] private Transform sliderTransform = null;
    private Slider healthBar = null;

    public void OnInitialize(PlayerController inPlayerController)
    {
        healthBar = sliderTransform.GetComponent<Slider>();

        // init player health bar 
        healthBar.minValue = 0;
        healthBar.maxValue = inPlayerController.HealthComponent.MaxHealth;
        healthBar.value = inPlayerController.HealthComponent.CurrentHealth;
        inPlayerController.OnHealthChanged += PlayerHealthUpdate;

        gameObject.SetActive(true);
    }

    private void PlayerHealthUpdate(int currentHealth)
    {
        healthBar.value = currentHealth;
    }
}
