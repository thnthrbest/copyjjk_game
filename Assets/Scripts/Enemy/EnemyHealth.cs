using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int Health = 30;
    public int Damage = 10;

    // Update is called once per frame
    void Update()
    {
        if(Health <= 0)
        {
            PlayerEnergy.Instance?.OnKillEnemy();
            Destroy(gameObject);
        }
    }
    void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag("Magic"))
        {
            float baseAtkMult = Charms.CharmManager.Instance != null ? Charms.CharmManager.Instance.GetBaseAttackMultiplier() : 1.0f;
            int actualDamage = Mathf.RoundToInt(Damage * baseAtkMult);
            Health -= actualDamage;
            Debug.Log($"Hit! Damage dealt: {actualDamage}"); 
        }
         if (other.CompareTag("dog"))
        {
            Destroy(gameObject);
             Debug.Log("โดนหมาแล้วตาย");
        }
    }
}
