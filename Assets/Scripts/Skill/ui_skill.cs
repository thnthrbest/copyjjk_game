using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ui_skill : MonoBehaviour
{
    public GameObject player;
    public GameObject timerText;
    //public GameObject ui_skill;
    private PlayerHealth playerHealth;
    private Animator animator;

    void Start()
    {
        playerHealth = player.GetComponent<PlayerHealth>();
        animator = GetComponent<Animator>();
    }

    // public void animateSkill()
    // {
        
    // }

    public void updateTimer()
    {
        timerText.SetActive(true);
        
    }
    
    void Update()
    {
        if (playerHealth.activeDuration > 0f)
        {
            animator.SetBool("use", true);
        }
        else
        {
            animator.SetBool("isActive", false);
            timerText.SetActive(false);
        }
        timerText.GetComponent<TMPro.TextMeshProUGUI>().text = playerHealth.activeDuration.ToString("F2");

    }
}
