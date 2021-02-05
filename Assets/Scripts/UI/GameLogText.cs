using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameLogText : MonoBehaviour
{
    Text log;
    void Start()
    {
        GameEvents.instance.doDamage += DoDamage;
        log = GetComponent<Text>();
    }

    private void DoDamage(Unit dealingDamage, Unit takingDamage, int amount)
    {
        log.text += $"<color='Yellow'>{dealingDamage.name}</color> deals <color='red'>{amount}</color> damage to {takingDamage.name}\n";
    }

}
