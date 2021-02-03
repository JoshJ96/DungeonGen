using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{
    public List<Vector3> CurrentAttackRange = new List<Vector3> { Vector3.zero };

    private void Start()
    {
        GameEvents.instance.moveUnit += MoveUnit;
    }

    private void MoveUnit(Unit unit, Vector3 destination)
    {
        if (unit.gameObject == this.gameObject)
        {
            StartCoroutine(MovePlayer(destination));
        }
    }

    IEnumerator MovePlayer(Vector3 destination)
    {
        moving = true;
        while (transform.position != destination)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * 5.00f);
            yield return null;
        }
        moving = false;
    }

    private void OnDrawGizmos()
    {
        //Draw attack range
        foreach (var tile in CurrentAttackRange)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position + tile, Vector3.one);
        }
    }
}