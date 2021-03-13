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
        GameEvents.instance.pushDungeonData += PushDungeonData;

    }
    #endregion

    Animator animator;
    List<Unit> attackTargets = new List<Unit>();
    float blendValue;

    private void Start()
    {
        GameEvents.instance.doDamage += DoDamage;
        animator = GetComponent<Animator>();
    }

    private void DoDamage(Unit attacking, Unit defending, int amount)
    {
        //Weird stuff with C# events and gameobjects
        if (this == null) return;

        //Damage display
        if (defending.gameObject == this.gameObject)
        {
            animator.SetTrigger("Hurt");
            GameObject damageIndicator = Instantiate(GameObject.Find("DamageIndicator"), new Vector3(GetCurrentNode().gridX, 0, GetCurrentNode().gridY), Quaternion.identity);
            damageIndicator.GetComponent<TMPro.TextMeshPro>().text = amount.ToString();
        }
    }

    void PushDungeonData(DungeonData data)
    {
        SetCurrentNode(Grid.instance.NodeFromWorldPoint(data.playerSpawnRoom.V3Center));

        transform.position = data.playerSpawnRoom.V3Center;
    }

    private void Update()
    {
        if (moving)
        {
            blendValue += 4 * Time.deltaTime;
        }
        else
        {
            blendValue -= 4 * Time.deltaTime;
        }

        blendValue = Mathf.Clamp(blendValue, 0, 1);
        animator.SetFloat("Blend", blendValue);
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

            if (unit.GetCurrentNode().GetWorldPoint() == Grid.instance.NodeFromWorldPoint(tileToCheck).GetWorldPoint())
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