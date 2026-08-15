using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int Health = 30;
    public int Damage = 10;

    [Header("Drop Box Settings")]
    [Range(0f, 1f)]
    public float dropChance = 0.05f;      // 5% chance by default
    public GameObject box3DPrefab;        // Optional custom 3D Box prefab
    public float boxDespawnTime = 2.0f;   // 2 seconds before despawning

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

        // Check drop chance for Charm Box
        if (Random.value <= dropChance)
        {
            if (Charms.CharmManager.Instance != null)
            {
                Charms.CharmManager.Instance.AddRunBox(1);
            }

            // Spawn 3D box object at enemy death position for 2 seconds
            SpawnDroppedBox3D(transform.position);
        }

        Destroy(gameObject);
    }

    private void SpawnDroppedBox3D(Vector3 spawnPosition)
    {
        GameObject spawnedBox = null;

        if (box3DPrefab != null)
        {
            spawnedBox = Instantiate(box3DPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            // Procedural 3D box object (Cube) if no custom prefab assigned
            spawnedBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spawnedBox.name = "DroppedCharmBox_3D";
            spawnedBox.transform.position = spawnPosition + Vector3.up * 0.5f;
            spawnedBox.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

            // Make collider trigger so physics won't block player or enemies
            Collider col = spawnedBox.GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            // Set box color
            Renderer ren = spawnedBox.GetComponent<Renderer>();
            if (ren != null)
            {
                ren.material.color = new Color(0.9f, 0.65f, 0.2f); // Wood/Gold box color
            }
        }

        if (spawnedBox != null)
        {
            spawnedBox.AddComponent<DroppedBoxVisualEffect>();
            Destroy(spawnedBox, boxDespawnTime); // Disappears in 2 seconds
        }
    }
}
