using System;
using System.Collections;
using System.Collections.Generic;
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

    private void Start()
    {
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
            transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * 5.00f);
            yield return null;
        }
        moving = false;
    }
}