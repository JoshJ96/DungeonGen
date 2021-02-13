using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerUnit : Unit
{
    #region Singleton
    public static PlayerUnit instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    public Animator animator;
    List<Unit> attackTargets = new List<Unit>();

    private void Start()
    {
        SetCurrentNode(Grid.instance.NodeFromWorldPoint(transform.position));
    }

    public void Attack()
    {
        isAttacking = true;
        //Get tile to check
        Vector3 tileToCheck = Vector3.zero;

        //Take the facing direction and send the attack towards that direction
        foreach (var item in directions)
        {
            if (item.Key == this.facingDirection)
            {
                tileToCheck = item.Value + transform.position;
            }
        }
        foreach (var unit in FindObjectsOfType<Unit>())
        {
            var hi = unit.GetCurrentNode();
            var hi2 = Grid.instance.NodeFromWorldPoint(tileToCheck);

            if (unit.GetCurrentNode().worldPosition == Grid.instance.NodeFromWorldPoint(tileToCheck).worldPosition)
            {
                print($"Sending an attack to {unit.gameObject.name}");
                attackTargets.Add(unit);
            }
        }
        //If no enemies in target range
        print($"Tried sending an attack to {tileToCheck}");
        StartCoroutine(NormalAttackEnum());

    }

    IEnumerator NormalAttackEnum()
    {
        animator.SetTrigger("Attacking");
        yield return null;
    }

    /*************************
     * ANIMATION EVENTS ONLY *
     *************************/
    void Anim_EndAttack()
    {
        animator.SetTrigger("Attacking");
        isAttacking = false;
    }
    void Anim_DealDamage()
    {
        if (attackTargets.Count != 0)
        {
            foreach (var target in attackTargets)
            {
                EnemyUnit takingDamage = target.GetComponent<EnemyUnit>();
                int randomDamage = UnityEngine.Random.Range(1, 10);
                GameEvents.instance.DoDamage(this, target, randomDamage);
                takingDamage.SetCurrentHitpoints(takingDamage.GetCurrentHitpoints() - randomDamage);
            }

        }

        attackTargets.Clear();
    }
}