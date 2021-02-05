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

    //Scan for player in aggro range
    public event Action scanForPlayerInAggroRange;
    public void ScanForPlayerInAggroRange() => scanForPlayerInAggroRange?.Invoke();

    //Scan for player in attack range
    public event Action scanForPlayerInAttackRange;
    public void ScanForPlayerInAttackRange() => scanForPlayerInAttackRange?.Invoke();

    //Do damage
    public event Action<Unit, Unit, int> doDamage;
    public void DoDamage(Unit givesDamage, Unit takesDamage, int amount) => doDamage?.Invoke(givesDamage, takesDamage, amount);
}