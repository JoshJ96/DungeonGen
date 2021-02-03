using System;
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

    //Move a singular unit
    public event Action<Unit, Vector3> moveUnit;
    public void MoveUnit(Unit unit, Vector3 point) => moveUnit?.Invoke(unit, point);

    //Move all "patrol" units
    public event Action movePatrolUnits;
    public void MovePatrolUnits() => movePatrolUnits?.Invoke();
}