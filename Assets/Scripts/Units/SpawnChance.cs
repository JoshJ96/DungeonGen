using UnityEngine;

[CreateAssetMenu(fileName = "SpawnChance", menuName = "SpawnChance")]
public class SpawnChance : ScriptableObject
{
    public GameObject unit;
    [Range(0,100)]
    public int percentChance;
}
