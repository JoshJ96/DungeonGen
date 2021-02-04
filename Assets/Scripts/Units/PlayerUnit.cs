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
            StartCoroutine(Move(destination));
        }
    }
}