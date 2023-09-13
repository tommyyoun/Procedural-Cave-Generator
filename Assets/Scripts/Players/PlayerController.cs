using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private LayerMask platformsLayerMask;
    new private Rigidbody2D rigidbody2D;
    private CapsuleCollider2D capsuleCollider2D;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Movement modifiers
    public float moveSpeed;
    public float jumpVelocity;
    public float maxHeightMultiplier;

    private int jumpCount = 0;

    // Attack info
    public float attackRadius = 0.5f;
    public int attackPower = 10;

    // Attack Script
    private IAttack _attackScript;
    private IMovement _movementScript;

    private void Awake()
    {
        // Every Player needs RigidBody2D, CapsuleC

        rigidbody2D = transform.GetComponent<Rigidbody2D>();
        capsuleCollider2D = transform.GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        _attackScript = GetComponent<IAttack>();
        _movementScript = GetComponent<IMovement>();
    }

    void Update()
    {
        HandleMovement();

        // Play attack animation which uses the attack script
        if (Input.GetMouseButtonDown(0))
            animator.SetTrigger("Attack");
    }

    private bool IsGrounded()
    {
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(capsuleCollider2D.bounds.center, capsuleCollider2D.bounds.size, 0f, Vector2.down, .1f, platformsLayerMask);
        return raycastHit2D.collider != null;
    }

    private void HandleMovement()
    {
        var jumpInputReleased = Input.GetButtonUp("Jump");

        // Left and Right logic
        if (Input.GetKey(KeyCode.A))
        {
            rigidbody2D.velocity = new Vector2(-moveSpeed, rigidbody2D.velocity.y);
            animator.SetFloat("Speed", moveSpeed);

            // Face left
            if (spriteRenderer.flipX == false)
            {
                spriteRenderer.flipX = true;
            }
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rigidbody2D.velocity = new Vector2(+moveSpeed, rigidbody2D.velocity.y);
            animator.SetFloat("Speed", moveSpeed);

            // Face right
            if (spriteRenderer.flipX == true)
            {
                spriteRenderer.flipX = false;
            }
        }
        else // No keys pressed
        {
            rigidbody2D.velocity = new Vector2(0, rigidbody2D.velocity.y);
            animator.SetFloat("Speed", 0);
        }

        // Jump logic
        if (IsGrounded() && Input.GetKeyDown(KeyCode.Space))
        {
            rigidbody2D.velocity = Vector2.up * jumpVelocity;
            animator.SetBool("IsJumping", true);
            jumpCount++;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && jumpCount < 2)
        {
            rigidbody2D.velocity = Vector2.up * jumpVelocity;
            jumpCount++;
        }

        if (jumpInputReleased && rigidbody2D.velocity.y > 0)
        {
            rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, rigidbody2D.velocity.y / maxHeightMultiplier);
        }

        if (rigidbody2D.velocity.y < 0)
        {
            animator.SetBool("IsFalling", true);
            animator.SetBool("IsJumping", false);
        }
        else
        {
            animator.SetBool("IsFalling", false);
        }

        if (IsGrounded() && jumpCount >= 2)
        {
            jumpCount = 0;
        }
    }

}
