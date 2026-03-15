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
            Destroy(gameObject);
        }
    }
    void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag("Magic"))
        {
            Health -= Damage;
            Debug.Log("hit"); 
        }
    }
}
