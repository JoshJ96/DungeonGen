using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State
{
    Patrol,
    TargetingPlayer,
    PlayerInAttackRange
}

public class EnemyUnit : Unit
{
    public State currentState;
    public List<Vector3> CurrentAttackRange = new List<Vector3> { Vector3.zero };

    private void OnDrawGizmos()
    {
        //Draw attack range
        foreach (var tile in CurrentAttackRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + tile, Vector3.one);
        }
    }
}