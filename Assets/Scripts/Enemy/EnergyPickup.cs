using UnityEngine;

public class EnergyPickup : MonoBehaviour
{
    public float energyAmount = 10f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            PlayerEnergy.Instance?.AddEnergy(energyAmount, "Pickup");
            PlayerHealth.Instance?.Heal(10);
            Destroy(gameObject);
        }

        
    }
}