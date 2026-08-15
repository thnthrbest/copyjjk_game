using UnityEngine;

public class EnergyPickup : MonoBehaviour
{
    public float energyAmount = 10f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            float healMult = Charms.CharmManager.Instance != null ? Charms.CharmManager.Instance.GetFireballHealMultiplier() : 1.0f;
            PlayerEnergy.Instance?.AddEnergy(energyAmount, "Pickup");
            PlayerHealth.Instance?.Heal(10f * healMult);
            Destroy(gameObject);
        }

        
    }
}