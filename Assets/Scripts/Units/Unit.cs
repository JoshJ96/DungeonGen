using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Range(0, 10)]
    public int attackRange;
    [Range(0, 10)]
    public int aggroRange;

    public bool moving = false;
    private Node desiredNode;
    public void SetDesiredNode(Node node)
    {
        desiredNode = node;
    }
    public Node GetDesiredNode()
    {
        return desiredNode;
    }

    //Build a range of nodes from the distance given (used for aggro/attack ranges)
    public List<Vector3> GetRange(int distance)
    {
        //Initialize List
        List<Vector3> range = new List<Vector3>();

        //Build Vector3s based on distance
        for (int i = -distance; i <= distance; i++)
        {
            for (int j = -distance; j <= distance; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                range.Add(new Vector3(transform.position.x + i, 0, transform.position.z + j));
            }
        }
        return range;
    }

    //Draws aggro range
    private void OnDrawGizmosSelected()
    {
        //Draw attack range
        foreach (var tile in GetRange(aggroRange))
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(tile, Vector3.one);
        }
        //Draws attack range
        foreach (var tile in GetRange(attackRange))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(tile, Vector3.one);
        }
    }
}