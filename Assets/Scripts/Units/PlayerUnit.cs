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

    float turnSmoothVelocity;
    public IEnumerator Move(Vector3 destination)
    {
        Vector3 inputvector = new Vector3(destination.x - transform.position.x, 0, destination.z - transform.position.z);
        //Calculate movement angle
        float targetAngle = Mathf.Atan2(inputvector.x, inputvector.z) * Mathf.Rad2Deg;
        //float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, 0.1f);
        transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);


        moving = true;
        while (transform.position != destination)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * 5.00f);
            yield return null;
        }
        moving = false;
    }
}