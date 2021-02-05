using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyUnit : Unit
{

    #region State Machine Setup
    public enum EnemyStates
    {
        Patrol,
        TargetingPlayer,
        PlayerInAttackRange
    }
    public EnemyStates currentState = EnemyStates.Patrol;

    public void SetState(EnemyStates state)
    {
        currentState = state;
    }
    public EnemyStates GetState()
    {
        return currentState;
    }

    #endregion

    private void Start()
    {
        GameEvents.instance.scanForPlayerInAggroRange += ScanForPlayerInAggroRange;
        GameEvents.instance.scanForPlayerInAttackRange += ScanForPlayerInAttackRange;
        GameEvents.instance.moveUnit += MoveUnit;
    }



    public void MoveUnit(Unit unit, Vector3 destination)
    {
        if (unit.gameObject == gameObject)
        {
            StartCoroutine(Move(destination));
        }
    }

    public IEnumerator Move(Vector3 destination)
    {
        moving = true;
        while (transform.position != destination)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * 6.00f);
            yield return null;
        }
        moving = false;
    }

    //If a player is within aggro range, change the current state
    private void ScanForPlayerInAggroRange()
    {
        foreach (Vector3 position in GetRange(aggroRange))
        {
            if (PlayerUnit.instance.transform.position == position)
            {
                SetState(EnemyStates.TargetingPlayer);
            }
        }
    }

    private void ScanForPlayerInAttackRange()
    {
        foreach (Vector3 position in GetRange(attackRange))
        {
            if (PlayerUnit.instance.transform.position == position)
            {
                SetState(EnemyStates.PlayerInAttackRange);
            }
        }
    }
}