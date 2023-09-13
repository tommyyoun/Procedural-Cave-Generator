using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Animator animator;

    public int maxHealth = 100;
    int currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    void Update()
    {

    }

    public void TakeDamage(int damage)
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt") && animator.GetBool("IsDead") == false)
        {
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                // Freeze body


                // Play die animation
                animator.SetBool("IsDead", true);
            }
            else
            {
                // Play hurt animation
                animator.SetTrigger("Hurt");
            }
            
        }
    }

    void Die()
    {
        // Played at animation method
        Destroy(gameObject);
    }

}
