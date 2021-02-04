using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public int attackRange;
    public int aggroRange;

    private void Start()
    {
        GameEvents.instance.moveUnit += MoveUnit;
    }

    private void MoveUnit(Unit unit, Vector3 destination)
    {
        if (unit.gameObject == this.gameObject)
        {
            StartCoroutine(Move(destination));
        }
    }

    /*private void OnDrawGizmos()
    {
        //Draw attack range
        foreach (var tile in CurrentAttackRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + tile, Vector3.one);
        } 
    }*/
}