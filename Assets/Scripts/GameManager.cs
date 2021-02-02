using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Turn
{
    Player,
    Enemy
}

public class GameManager : MonoBehaviour
{
    public Turn currentTurn = Turn.Player;
}