using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int maxInventorySize;
    public List<Inventory_Slot> items;

    private void Start()
    {
        items = new List<Inventory_Slot>(maxInventorySize);
        GameEvents.instance.addItem += AddItem;
        GameEvents.instance.removeItem += RemoveItem;
        GameEvents.instance.useItem += UseItem;
    }

    private void UseItem(Item toUse)
    {

    }

    private void RemoveItem(Item toRemove, int amount)
    {

    }

    private void AddItem(Item toAdd, int amount)
    {

    }
}