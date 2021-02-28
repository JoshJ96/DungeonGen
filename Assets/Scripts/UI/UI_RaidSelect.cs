using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_RaidSelect : MonoBehaviour
{
    public void StartForestCave()
    {
        SceneManager.LoadScene("Dungeon1_01", LoadSceneMode.Single);
    }
}
