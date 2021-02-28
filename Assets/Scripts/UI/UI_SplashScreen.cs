using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SplashScreen : MonoBehaviour
{
    public Text dungeonName, floorNumber;
    private void Start()
    {
        GameEvents.instance.pushDungeonData += PushDungeonData;
    }

    private void PushDungeonData(DungeonData data)
    {
        dungeonName.text = data.dungeonName;
        floorNumber.text = $"Floor {data.floorNumber}";
    }
}
