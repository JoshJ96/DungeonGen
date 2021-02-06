using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item")]
public class Item : ScriptableObject
{
    public Sprite itemIcon;
    public string itemName;
    public string itemDescription;
    public int itemActionID;
    public bool stackable;
    public int maxStack;
}