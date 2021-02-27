using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Continue : MonoBehaviour
{
    Button btn;

    private void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
    }

    private void TaskOnClick()
    {
        if (File.Exists(Application.persistentDataPath + "/gamedata.json"))
        {
            print("File exists");
            string fileContents = File.ReadAllText(Application.persistentDataPath + "/gamedata.json");
            GameDataManager.instance.LoadData(fileContents);
            SceneManager.LoadScene("Overworld", LoadSceneMode.Single);
        }
        else
        {
            print("File doesn't exist");
        }
    }
}
