using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMeleeAttack : MonoBehaviour, IAttack
{
    public Transform attackPoint;
    public LayerMask enemyLayers;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private int attackPower;
    private float attackRadius;

    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        attackPower = GetComponent<PlayerController>().attackPower;
        attackRadius = GetComponent<PlayerController>().attackRadius;
    }

    public void HandleAttack()
    {
        // Update attack point to the left
        if (spriteRenderer.flipX == true && attackPoint.localPosition.x > 0)
        {
            attackPoint.localPosition = new Vector3(attackPoint.localPosition.x * -1, attackPoint.localPosition.y, 0);
        }

        // Update attack point to the right
        if (spriteRenderer.flipX == false && attackPoint.localPosition.x < 0)
        {
            attackPoint.localPosition = new Vector3(attackPoint.localPosition.x * -1, attackPoint.localPosition.y, 0);
        }

        // Detect enemies in range of attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayers);

        // Damage enemies
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Enemy>().TakeDamage(attackPower);
        }

    }
}
