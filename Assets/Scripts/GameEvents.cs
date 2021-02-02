using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    #region Singleton
    public static GameEvents instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    //Event fired upon player's turn starting
    public event Action playerTurnStart;
    public void PlayerTurnStart() => playerTurnStart?.Invoke();

    //Event fired upon enemy's turn starting
    public event Action enemyTurnStart;
    public void EnemyTurnStart() => enemyTurnStart?.Invoke();

    //Event fired upon a board step occuring
    public event Action<Vector3> boardStep;
    public void BoardStep(Vector3 destination) => boardStep?.Invoke(destination);

}