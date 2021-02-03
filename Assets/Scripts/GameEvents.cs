﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    #region Singleton
    public static GameEvents instance;
    void Awake()
    {
        instance = this;
    }
    #endregion

    public event Action<Unit, Vector3> moveUnit;
    public void MoveUnit(Unit unit, Vector3 point) => moveUnit?.Invoke(unit, point);
}