using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public bool moving = false;
    private Node desiredNode;

    public IEnumerator Move(Vector3 destination)
    {
        moving = true;
        while (transform.position != destination)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * 5.00f);
            yield return null;
        }
        moving = false;
    }
    public void SetDesiredNode(Node node)
    {
        desiredNode = node;
    }
    public Node GetDesiredNode()
    {
        return desiredNode;
    }

}