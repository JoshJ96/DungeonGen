using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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

    public void SetState(EnemyStates state) => currentState = state;
    public EnemyStates GetState()
    {
        return currentState;
    }

    #endregion

    Animator animator;



    private void Start()
    {
        SetCurrentNode(Grid.instance.NodeFromWorldPoint(transform.position));
        var hi = GetCurrentNode();

        currentHitpoints = maxHitpoints;
        GameEvents.instance.scanForPlayerInAggroRange += ScanForPlayerInAggroRange;
        GameEvents.instance.scanForPlayerInAttackRange += ScanForPlayerInAttackRange;
        GameEvents.instance.doDamage += DoDamage;

        animator = GetComponent<Animator>();
    }

    private void DoDamage(Unit arg1, Unit arg2, int arg3)
    {
        if (currentHitpoints <= 0)
        {
            GameEvents.instance.EnemyDestruction(this);
            Grid.instance.grid[GetCurrentNode().gridX, GetCurrentNode().gridY].walkable = true;
            Destroy(gameObject);
        }
    }

    public override void MoveUnit(Vector3 destination)
    {
        destination = new Vector3(destination.x, transform.position.y, destination.z);
        SetCurrentNode(Grid.instance.NodeFromWorldPoint(destination));
        Grid.instance.NodeFromWorldPoint(transform.position).walkable = true;
        Grid.instance.NodeFromWorldPoint(destination).walkable = false;
        StartCoroutine(Move(destination));
    }

    //If a player is within aggro range, change the current state
    public void ScanForPlayerInAggroRange()
    {
        foreach (Node node in GetRange(aggroRange))
        {
            if (PlayerUnit.instance.GetCurrentNode().GetWorldPoint() == node.GetWorldPoint())
            {
                SetState(EnemyStates.TargetingPlayer);
                return;
            }
        }
        SetState(EnemyStates.Patrol);
    }

    public void ScanForPlayerInAttackRange()
    {
        foreach (Node node in GetRange(attackRange))
        {
            if (PlayerUnit.instance.GetCurrentNode().GetWorldPoint() == node.GetWorldPoint())
            {
                SetState(EnemyStates.PlayerInAttackRange);
                return;
            }
        }
        ScanForPlayerInAggroRange();
    }

    public void Attack(Unit toAttack)
    {
        isAttacking = true;
        RotateTowards(toAttack.transform.position);
        animator.SetBool("Attacking", true);
    }

    /*************************
     * ANIMATION EVENTS ONLY *
     *************************/
    void Anim_EndAttack()
    {
        animator.SetBool("Attacking", false);
        isAttacking = false;
    }

    void Anim_DealDamage()
    {
        Unit takingDamage = PlayerUnit.instance;
        int randomDamage = UnityEngine.Random.Range(1, 10);
        GameEvents.instance.DoDamage(this, takingDamage, randomDamage);
        takingDamage.SetCurrentHitpoints(takingDamage.GetCurrentHitpoints() - randomDamage);
    }
}