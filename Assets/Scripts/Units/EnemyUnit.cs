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

    public int maxHitpoints;
    private int currentHitpoints;
    public int GetCurrentHitpoints() => currentHitpoints;
    public void SetCurrentHitpoints(int hp) => currentHitpoints = hp;

    private void Start()
    {
        SetCurrentNode(Grid.instance.NodeFromWorldPoint(transform.position));
        currentHitpoints = maxHitpoints;
        GameEvents.instance.scanForPlayerInAggroRange += ScanForPlayerInAggroRange;
        GameEvents.instance.scanForPlayerInAttackRange += ScanForPlayerInAttackRange;
        animator = GetComponent<Animator>();
    }

    private void LateUpdate()
    {
        if (currentHitpoints <= 0)
        {
            GameEvents.instance.EnemyDestruction(this);
            Destroy(gameObject);
        }
    }

    //If a player is within aggro range, change the current state
    public void ScanForPlayerInAggroRange()
    {
        foreach (Vector3 position in GetRange(aggroRange))
        {
            if (   PlayerUnit.instance.transform.position.x == position.x
                && PlayerUnit.instance.transform.position.z == position.z)
            {
                SetState(EnemyStates.TargetingPlayer);
                return;
            }
        }
        SetState(EnemyStates.Patrol);
    }

    public void ScanForPlayerInAttackRange()
    {
        foreach (Vector3 position in GetRange(attackRange))
        {
            if (   PlayerUnit.instance.transform.position.x == position.x
                && PlayerUnit.instance.transform.position.z == position.z)
            {
                SetState(EnemyStates.PlayerInAttackRange);
                return;
            }
        }
        ScanForPlayerInAggroRange();
    }

    public void Attack(Unit toAttack)
    {
        RotateTowards(toAttack.transform.position);
        int randomDamage = UnityEngine.Random.Range(1, 10);
        GameEvents.instance.DoDamage(this, toAttack, randomDamage);
        StartCoroutine(PerformAttack());
    }

    IEnumerator PerformAttack()
    {
        animator.SetBool("Attacking", true);
        isAttacking = true;
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
        animator.SetBool("Attacking", false);
    }
}