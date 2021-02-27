using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap_Drawer : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    public Node thisNode => Grid.instance.NodeFromWorldPoint(transform.position);

    public Node playerNode => PlayerUnit.instance.GetCurrentNode();

    public Node[,] map => Grid.instance.grid;


    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        GameEvents.instance.turnPass += TurnPass;
    }

    int drawRange = 5;
    private void TurnPass()
    {
        if (playerNode == thisNode)
        {
            for (int x = playerNode.gridX - drawRange; x < playerNode.gridX + drawRange; x++)
            {
                for (int y = playerNode.gridY - drawRange; y < playerNode.gridY + drawRange; y++)
                {
                    if (thisNode.roomPartOf.NodesWithinRoom(map).Contains(map[x, y]) && (thisNode.roomPartOf.type == Room.Type.Room))
                    {
                        map[x, y].visibleOnMap = true;
                        continue;
                    }
                    if (thisNode.roomPartOf.NodesWithinRoom(map).Contains(map[x, y]) && (thisNode.roomPartOf.type == Room.Type.Hallway))
                    {
                        map[x, y].visibleOnMap = true;
                        continue;
                    }
                }
            }
        }


        if (thisNode.visibleOnMap)
        {
            spriteRenderer.enabled = true;
        }
    }
}
