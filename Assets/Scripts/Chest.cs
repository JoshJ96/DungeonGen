using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class Chest : MonoBehaviour
{
    public List<Vector3> interactTiles = new List<Vector3> { };
    public GameObject lootText;

    private void Update()
    {
        foreach (Vector3 tile in interactTiles)
        {
            if (PlayerUnit.instance.GetCurrentNode().worldPosition == Grid.instance.NodeFromWorldPoint(transform.position + tile).worldPosition)
            {
                if (PlayerUnit.instance.GetDirection() == Unit.Direction.North)
                {
                    lootText.SetActive(true);
                }
                else
                {
                    lootText.SetActive(false);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (interactTiles.Count != 0)
        {
            //Draws attack range
            foreach (var tile in interactTiles)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position + tile, Vector3.one);
            }
        }
    }
}
