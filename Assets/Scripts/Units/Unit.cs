using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Unit : MonoBehaviour
{
    #region Direction States
    public enum Direction
    {
        North,
        Northeast,
        East,
        Southeast,
        South,
        Southwest,
        West,
        Northwest
    }

    public Dictionary<Direction, Vector3> directions = new Dictionary<Direction, Vector3>
    {
        { Direction.North,      new Vector3(0,0,1)   },
        { Direction.Northeast,  new Vector3(1,0,1)   },
        { Direction.East,       new Vector3(1,0,0)   },
        { Direction.Southeast,  new Vector3(1,0,-1)  },
        { Direction.South,      new Vector3(0,0,-1)  },
        { Direction.Southwest,  new Vector3(-1,0,-1) },
        { Direction.West,       new Vector3(-1,0,0)  },
        { Direction.Northwest,  new Vector3(-1,0,1)  }
    };

    public Direction facingDirection;

    public Direction GetDirection()
    {
        return facingDirection;
    }
    #endregion

    public int maxHitpoints;
    public int currentHitpoints;

    public int GetCurrentHitpoints() => currentHitpoints;
    public void SetCurrentHitpoints(int hp) => currentHitpoints = hp;

    public int walkSpeed = 7;

    //Desired Node (used in Board Manager)
    private Node desiredNode;
        public void SetDesiredNode(Node node) => desiredNode = node;
        public Node GetDesiredNode() => desiredNode;

    private Node currentNode;
        public void SetCurrentNode(Node node) => currentNode = node;
        public Node GetCurrentNode() => currentNode;

    public int displacementAngle;

    //Attack and aggro ranges
    [Range(0, 10)]
    public int attackRange;
    [Range(0, 10)]
    public int aggroRange;

    //Control (used in Board Manager)
    public bool moving = false;
    public bool isAttacking = false;

    //Build a range of nodes from the distance given (used for aggro/attack ranges)
    public List<Node> GetRange(int distance)
    {
        //Initialize List
        List<Node> range = new List<Node>();

        //Build Vector3s based on distance
        for (int i = -distance; i <= distance; i++)
        {
            for (int j = -distance; j <= distance; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                range.Add(Grid.instance.grid[GetCurrentNode().gridX + i, GetCurrentNode().gridY + j]);
            }
        }
        return range;
    }

    public virtual void MoveUnit(Vector3 destination)
    {
        destination = new Vector3(destination.x, transform.position.y, destination.z);
        SetCurrentNode(Grid.instance.NodeFromWorldPoint(destination));
        StartCoroutine(Move(destination));
    }

    public IEnumerator Move(Vector3 destination)
    {
        RotateTowards(destination);

        moving = true;
        while (transform.position != destination)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * walkSpeed);
            yield return null;
        }
        moving = false;
        SetCurrentNode(Grid.instance.NodeFromWorldPoint(transform.position));
    }

    public void RotateTowards(Vector3 destination)
    {
        //No movement? Don't change rotation
        if (transform.position - destination == Vector3.zero)
        {
            return;
        }

        //Get directional vector towards destination
        Vector3 targetVector = new Vector3(destination.x - transform.position.x, 0, destination.z - transform.position.z);

        //Change the enumeration
        foreach (var item in directions)
        {
            if (item.Value == targetVector)
            {
                facingDirection = item.Key;
            }
        }

        //Calculate movement angle and rotate
        float targetAngle = Mathf.Atan2(targetVector.x, targetVector.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, targetAngle + displacementAngle, 0f);
    }
}