using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum States
{
    WaitingForPlayerInput,
    MoveAndDeclump,
    EnemyAttackPhase,
    ExtraPhase
}

public class BoardManager : MonoBehaviour
{
    Grid worldGrid;
    public States currentState = States.WaitingForPlayerInput;
    PlayerUnit playerUnit;

    private void Start()
    {
        worldGrid = GetComponent<Grid>();
        playerUnit = FindObjectOfType<PlayerUnit>();
    }

    void Update()
    {
        switch (currentState)
        {
            case States.WaitingForPlayerInput:
                Vector3 inputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

                if (inputVector != Vector3.zero)
                {
                    currentState = States.MoveAndDeclump;

                    //Desired player location
                    Vector3 desiredLocation = new Vector3(
                        playerUnit.transform.localPosition.x + inputVector.x,
                        0,
                        playerUnit.transform.localPosition.z + inputVector.z);

                    //Check the node at desiredLocation world point
                    Node toCheck = worldGrid.NodeFromWorldPoint(desiredLocation);

                    if (toCheck.walkable)
                    {
                        playerUnit.desiredNode = toCheck;

                        List<EnemyUnit> enemyPatrolUnits = FindObjectsOfType<EnemyUnit>().Where(x => x.currentState == State.Patrol).ToList();
                        foreach (EnemyUnit unit in enemyPatrolUnits)
                        {
                            //Get surrounding nodes
                            List<Node> surroundingNodes = Grid.instance.GetNeighbours(Grid.instance.NodeFromWorldPoint(unit.transform.position));

                            //Pick random nodes until an unclaimed one is found
                            while (surroundingNodes.Count != 0)
                            {
                                //Limit nodes to only walkable nodes
                                surroundingNodes = surroundingNodes.Where(x => x.walkable).ToList();

                                //Pick one at random
                                if (surroundingNodes.Count != 0)
                                {
                                    int randomIndex = UnityEngine.Random.Range(0, surroundingNodes.Count);

                                    //Save desired node
                                    if (!isDuplicateDesiredNode(surroundingNodes[randomIndex]))
                                    {
                                        unit.desiredNode = surroundingNodes[randomIndex];
                                        break;
                                    }
                                    else
                                    {
                                        surroundingNodes.RemoveAt(randomIndex);
                                    }
                                }
                            }

                        }//End of (unit in enemyPatrolUnits) loop

                        List<Unit> toMove = FindObjectsOfType<Unit>().Where(x => x.desiredNode != null).ToList();

                        foreach (var unit in toMove)
                        {
                            GameEvents.instance.MoveUnit(unit, unit.desiredNode.worldPosition);
                        }

                        //GameEvents.instance.MovePatrolUnits();
                        //currentState = States.MoveAndDeclump;
                    }
                }
                break;

            case States.MoveAndDeclump:
                //if (!anyUnitsMoving())
                    //currentState = States.WaitingForPlayerInput;
                break;

            case States.EnemyAttackPhase:
                break;

            case States.ExtraPhase:
                break;

            default:
                break;
        }
    }

    //Helper functions
    bool anyUnitsMoving()
    {
        List<Unit> units = GameObject.FindObjectsOfType<Unit>().ToList();
        foreach (var unit in units)
        {
            if (unit.moving)
            {
                return true;
            }
        }
        return false;
    }

    bool isDuplicateDesiredNode(Node toCheck)
    {
        List<Unit> units = GameObject.FindObjectsOfType<Unit>().Where(x => x.desiredNode != null).ToList();
        foreach (var item in units)
        {
            if (toCheck.worldPosition == item.desiredNode.worldPosition)
            {
                return true;
            }
        }
        return false;
    }
}
