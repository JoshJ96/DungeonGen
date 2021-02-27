using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager instance;
    public GameData gameData;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        instance = this;
    }

    public void LoadData(string json)
    {
        gameData = JsonUtility.FromJson<GameData>(json);
    }

    public void SaveData()
    {

        string hi = JsonUtility.ToJson(gameData);

        File.WriteAllText(Application.persistentDataPath + "/gamedata.json", hi);
    }
}