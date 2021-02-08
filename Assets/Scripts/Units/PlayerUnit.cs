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
}