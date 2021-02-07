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

    public Animator animator;

    private void Start()
    {
        GameEvents.instance.moveUnit += MoveUnit;
    }

    private void Update()
    {
        animator.SetFloat("Blend", GetInputVector().normalized.magnitude);
    }
    
    private Vector3 GetInputVector()
    {
        return new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
    }
}