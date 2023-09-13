using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyMovement
{
    void HandleMovement(Transform dest);

    bool IsGrounded();
}
