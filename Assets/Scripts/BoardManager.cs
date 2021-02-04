using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    Grid worldGrid;
    PlayerUnit playerUnit;

    #region State Machine Setup

    private States currentState;
    public enum States
    {
        WaitingForPlayerInput,
        CalculateMovements,
        MovingUnits,
        EnemyAttackPhase,
        ExtraPhase
    }
    public void ChangeState(States state) => currentState = state;

    #endregion

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
                if (GetInputVector() != Vector3.zero)
                {
                    //Desired player location
                    Vector3 desiredLocation = new Vector3(
                        playerUnit.transform.position.x + GetInputVector().x,
                        0,
                        playerUnit.transform.position.z + GetInputVector().z);

                    //Check the node at desiredLocation world point
                    Node toCheck = worldGrid.NodeFromWorldPoint(desiredLocation);

                    //If the node is walkable
                    if (toCheck.walkable)
                    {
                        //Disable player input and set their desired node
                        ChangeState(States.CalculateMovements);
                        playerUnit.SetDesiredNode(toCheck);

                        //Grab list of enemy units in the "patrol" state
                        List<EnemyUnit> enemyPatrolUnits = FindObjectsOfType<EnemyUnit>().Where(x => x.currentState == State.Patrol).ToList();

                        //Loop through each patrol unit and find random desired tiles
                        foreach (EnemyUnit unit in enemyPatrolUnits)
                        {
                            //Get surrounding nodes (walkable only)
                            List<Node> surroundingNodes = worldGrid.GetNeighbours(worldGrid.NodeFromWorldPoint(unit.transform.position)).Where(x => x.walkable).ToList();

                            //Pick random nodes until an unclaimed one is found. If none is ever found, that's ok. The unit will be skipped
                            while (surroundingNodes.Count != 0)
                            {
                                int randomIndex = UnityEngine.Random.Range(0, surroundingNodes.Count);
                                Node randomNode = surroundingNodes[randomIndex];

                                //Save desired node
                                if (!IsDuplicateDesiredNode(randomNode))
                                {
                                    unit.SetDesiredNode(randomNode);
                                    break;
                                }
                                //If it can't be walked to, remove it from the list and continue the while loop
                                else
                                {
                                    surroundingNodes.RemoveAt(randomIndex);
                                }
                            }
                        }

                        //Build list of units ready to be moved
                        List<Unit> toMove = FindObjectsOfType<Unit>().Where(x => x.GetDesiredNode() != null).ToList();

                        //Move all units
                        foreach (var unit in toMove)
                        {
                            GameEvents.instance.MoveUnit(unit, unit.GetDesiredNode().worldPosition);
                        }

                        //Exit the state
                        ChangeState(States.MovingUnits);
                    }
                }
                break;
            case States.CalculateMovements:
                break;
            case States.MovingUnits:
                //Once all units are done with their move, the next phase can begin
                if (!AnyUnitsMoving())
                    currentState = States.WaitingForPlayerInput;
                break;
            case States.EnemyAttackPhase:
                break;
            case States.ExtraPhase:
                break;
            default:
                break;
        }
    }

    /*******************************************************
    ██╗░░██╗███████╗██╗░░░░░██████╗░███████╗██████╗░░██████╗
    ██║░░██║██╔════╝██║░░░░░██╔══██╗██╔════╝██╔══██╗██╔════╝
    ███████║█████╗░░██║░░░░░██████╔╝█████╗░░██████╔╝╚█████╗░
    ██╔══██║██╔══╝░░██║░░░░░██╔═══╝░██╔══╝░░██╔══██╗░╚═══██╗
    ██║░░██║███████╗███████╗██║░░░░░███████╗██║░░██║██████╔╝
    ╚═╝░░╚═╝╚══════╝╚══════╝╚═╝░░░░░╚══════╝╚═╝░░╚═╝╚═════╝░
     *******************************************************/

    //Builds vector3 based on horizontal and vertical inputs
    private Vector3 GetInputVector()
    {
        return new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
    }

    //Scans all units in the room and checks if they're moving
    private bool AnyUnitsMoving()
    {
        List<Unit> units = FindObjectsOfType<Unit>().ToList();
        foreach (var unit in units)
        {
            if (unit.moving)
            {
                return true;
            }
        }
        return false;
    }

    //Scans all units in the room and checks if any desired nodes collide
    private bool IsDuplicateDesiredNode(Node toCheck)
    {
        List<Unit> units = GameObject.FindObjectsOfType<Unit>().Where(x => x.GetDesiredNode() != null).ToList();
        foreach (var item in units)
        {
            if (toCheck.worldPosition == item.GetDesiredNode().worldPosition)
            {
                return true;
            }
        }
        return false;
    }

}
