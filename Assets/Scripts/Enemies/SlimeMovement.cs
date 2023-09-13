using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeMovement : MonoBehaviour, IEnemyMovement
{
    public LayerMask platformsLayerMask;
    new private Rigidbody2D rigidbody2D;
    private RaycastHit2D raycastHit2D;
    private CapsuleCollider2D capsuleCollider2D;

    public float airTime;
    private float vx, vy;

    void Awake()
    {
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void HandleMovement(Transform destination)
    {
        if(IsGrounded())
        {
            vx = (destination.position.x - transform.position.x) / airTime;
            vy = Mathf.Abs(Physics.gravity.y * airTime);

            rigidbody2D.velocity = new Vector3(vx, vy, 0);
        }
    }

    public bool IsGrounded()
    {
        raycastHit2D = Physics2D.BoxCast(capsuleCollider2D.bounds.center, capsuleCollider2D.bounds.size, 0f, Vector2.down, .1f, platformsLayerMask);
        return raycastHit2D.collider != null;
    }
}