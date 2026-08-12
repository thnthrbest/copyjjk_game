using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int Health = 30;
    public int Damage = 10;

    private bool isDead = false;

    void Update()
    {
        if (Health <= 0 && !isDead)
        {
            Die();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Magic"))
        {
            float baseAtkMult = Charms.CharmManager.Instance != null ? Charms.CharmManager.Instance.GetBaseAttackMultiplier() : 1.0f;
            int actualDamage = Mathf.RoundToInt(Damage * baseAtkMult);
            Health -= actualDamage;
            Debug.Log($"Hit! Damage dealt: {actualDamage}"); 
        }

        if (other.CompareTag("dog") && !isDead)
        {
            Debug.Log("โดนหมาแล้วตาย");
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        PlayerEnergy.Instance?.OnKillEnemy();

        // 5% Chance to drop Charm Box for End-Game unboxing
        if (Random.value <= 0.05f)
        {
            if (Charms.CharmManager.Instance != null)
            {
                Charms.CharmManager.Instance.AddRunBox(1);
            }
        }

        Destroy(gameObject);
    }
}
