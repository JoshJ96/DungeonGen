using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum State
{
    Patrol,
    TargetingPlayer,
    PlayerInAttackRange
}

public class EnemyUnit : Unit
{
    public bool moving = false;
    public State currentState;
    public List<Vector3> CurrentAttackRange = new List<Vector3> { Vector3.zero };

    private void Start()
    {
        GameEvents.instance.movePatrolUnits += MovePatrolUnits;
    }

    private void MovePatrolUnits()
    {
        if (currentState == State.Patrol)
        {
            //Get surrounding nodes
            List<Node> surroundingNodes = Grid.instance.GetNeighbours(Grid.instance.NodeFromWorldPoint(transform.position));

            //Limit nodes to only walkable nodes
            surroundingNodes = surroundingNodes.Where(x => x.walkable).ToList();

            //Pick one at random
            if (surroundingNodes.Count != 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, surroundingNodes.Count);
                //Move to it
                StartCoroutine(MoveUnit(surroundingNodes[randomIndex].worldPosition));
            }
        }
    }

    IEnumerator MoveUnit(Vector3 destination)
    {
        moving = true;
        while (transform.position != destination)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * 5.00f);
            yield return null;
        }
        moving = false;
        GameObject.FindObjectOfType<BoardManager>().currentState = States.WaitingForPlayerInput;
    }

    private void OnDrawGizmos()
    {
        //Draw attack range
        foreach (var tile in CurrentAttackRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + tile, Vector3.one);
        }
    }
}