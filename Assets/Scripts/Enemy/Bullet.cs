using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float      damage     = 10f;
    public string     source     = "Enemy Bullet";

    void OnTriggerEnter(Collider other)
    {
        //if (other.CompareTag("Enemy") || other.CompareTag("Bullet")) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth hp = other.GetComponent<PlayerHealth>();
            if (hp != null)
                hp.TakeDamage(damage, source);
            
            Destroy(gameObject);
        }

       
    }
}