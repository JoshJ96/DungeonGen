using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_NewGame : MonoBehaviour
{
    Button btn;

    private void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
    }

    private void TaskOnClick()
    {
        string jsonString = @"{""level"": 1}";

        File.WriteAllText(Application.persistentDataPath + "/gamedata.json", jsonString);
        GameDataManager.instance.LoadData(jsonString);
        SceneManager.LoadScene("Overworld", LoadSceneMode.Single);
    }
}
