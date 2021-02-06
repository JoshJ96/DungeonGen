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

    public bool isAttacking = false;
    Animator animator;


    private void Start()
    {
        GameEvents.instance.scanForPlayerInAggroRange += ScanForPlayerInAggroRange;
        GameEvents.instance.scanForPlayerInAttackRange += ScanForPlayerInAttackRange;
        GameEvents.instance.moveUnit += MoveUnit;
        animator = GetComponent<Animator>();
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
        RotateTowards(destination);

        moving = true;
        while (transform.position != destination)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * 6.00f);
            yield return null;
        }
        moving = false;
    }

    private void RotateTowards(Vector3 destination)
    {
        //Get directional vector towards destination
        Vector3 targetVector = new Vector3(destination.x - transform.position.x, 0, destination.z - transform.position.z);
        //Calculate movement angle
        float targetAngle = Mathf.Atan2(targetVector.x, targetVector.z) * Mathf.Rad2Deg;
        //float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, 0.1f);
        transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
    }

    //If a player is within aggro range, change the current state
    private void ScanForPlayerInAggroRange()
    {
        foreach (Vector3 position in GetRange(aggroRange))
        {
            if (PlayerUnit.instance.transform.position == position)
            {
                SetState(EnemyStates.TargetingPlayer);
                return;
            }
        }
        SetState(EnemyStates.Patrol);
    }

    private void ScanForPlayerInAttackRange()
    {
        foreach (Vector3 position in GetRange(attackRange))
        {
            if (PlayerUnit.instance.transform.position == position)
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
        int randomDamage = UnityEngine.Random.Range(0, 10);
        GameEvents.instance.DoDamage(this, toAttack, randomDamage);
        StartCoroutine(PerformAttack());
    }

    IEnumerator PerformAttack()
    {
        animator.SetBool("Attacking", true);
        isAttacking = true;
        yield return new WaitForSeconds(1.00f);
        isAttacking = false;
        animator.SetBool("Attacking", false);
    }
}