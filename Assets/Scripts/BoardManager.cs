using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    #region State Machine Setup

    private States currentState;
    public enum States
    {
        WaitingForPlayerInput,
        CalculateMovements,
        MoveUnits,
        EnemyAttackPhase
    }
    public void ChangeState(States state) => currentState = state;

    #endregion

    Grid worldGrid;
    Pathfinding pathfinding;
    List<EnemyUnit> enemyAttackingUnits = new List<EnemyUnit>();
    bool canInput = true;

    private void Start()
    {
        worldGrid = GetComponent<Grid>();
        pathfinding = GetComponent<Pathfinding>();
        GameEvents.instance.turnPass += TurnPass;
    }

    void Update()
    {
        switch (currentState)
        {
            case States.WaitingForPlayerInput:
                if (GetInputVector() != Vector3.zero)
                {
                    canInput = false;
                    //Desired player location
                    Vector3 desiredLocation = new Vector3(
                        PlayerUnit.instance.transform.position.x + GetInputVector().x,
                        0,
                        PlayerUnit.instance.transform.position.z + GetInputVector().z);

                    //Check the node at desiredLocation world point
                    Node toCheck = worldGrid.NodeFromWorldPoint(desiredLocation);

                    //If the node is walkable
                    if (toCheck.walkable)
                    {
                        //Disable player input and set their desired node
                        ChangeState(States.CalculateMovements);
                        PlayerUnit.instance.SetDesiredNode(toCheck);
                        InitializeTurn();
                    }
                    else
                    {
                        PlayerUnit.instance.RotateTowards(desiredLocation);
                        canInput = true;
                    }
                }
                //Press B to "rest" a turn
                else if (canInput)
                {
                    if (Input.GetKey(KeyCode.JoystickButton1))
                    {
                        canInput = false;
                        InitializeTurn();
                    }
                }
                else if (canInput && Input.GetKeyDown(KeyCode.JoystickButton3))
                {
                    if (true)
                    {
                        if (objectAtWorldPoint(PlayerUnit.instance.transform.position + Vector3.forward, "Chest"))
                        {
                            if (PlayerUnit.instance.GetDirection() == Unit.Direction.North)
                            {

                            }
                        }
                    }
                }

                break;
            case States.CalculateMovements:
                break;
            case States.MoveUnits:
                //Once all units are done with their move, the next phase can begin
                if (!AnyUnitsMoving())
                {
                    //Get ready and move to the attack phase
                    GameEvents.instance.ScanForPlayerInAttackRange();

                    //Grab list of enemy units in the "attackRange" state
                    enemyAttackingUnits = FindObjectsOfType<EnemyUnit>().Where(x => x.GetState() == EnemyUnit.EnemyStates.PlayerInAttackRange).ToList();
                    //todo: Sort by speeds and stuff could go here

                    //If there's no enemies to attack the player, this phase is done
                    if (enemyAttackingUnits.Count == 0)
                    {
                        //All units are done moving
                        GameEvents.instance.TurnPass();
                        currentState = States.WaitingForPlayerInput;
                    }
                    //If there is, it's time to order the turns of them
                    else
                    {
                        enemyAttackingUnits[0].Attack(PlayerUnit.instance);
                        currentState = States.EnemyAttackPhase;
                    }
                }
                break;
            case States.EnemyAttackPhase:
                if (!enemyAttackingUnits[0].isAttacking)
                {
                    enemyAttackingUnits.RemoveAt(0);
                    if (enemyAttackingUnits.Count != 0)
                    {
                        enemyAttackingUnits[0].Attack(PlayerUnit.instance);
                    }
                    //If there's no enemies to attack the player, this phase is done
                    else
                    {
                        //All units are done moving
                        GameEvents.instance.TurnPass();
                        currentState = States.WaitingForPlayerInput;
                    }
                }
                break;
            default:
                break;
        }
    }

    private void SetAggroUnitDesiredNodes()
    {
        //Grab list of enemy units in the "aggro" state
        List<EnemyUnit> enemyPatrolUnits = FindObjectsOfType<EnemyUnit>().Where(x => x.GetState() == EnemyUnit.EnemyStates.TargetingPlayer).ToList();

        //Loop through each patrol unit and find the first element of it's A* path
        foreach (EnemyUnit unit in enemyPatrolUnits)
        {
            pathfinding.FindPath(unit.transform.position, PlayerUnit.instance.transform.position);

            //Save desired node if it's not a dupe
            if (worldGrid.path != null)
            {
                if (worldGrid.path.Count != 0)
                {
                    if (!IsDuplicateDesiredNode(worldGrid.path[0]))
                    {
                        unit.SetDesiredNode(worldGrid.path[0]);
                    }
                }
            }
        }
    }

    void InitializeTurn()
    {
        //All enemy units will scan for player and change their states accordingly
        GameEvents.instance.ScanForPlayerInAggroRange();

        //Calculate the desired nodes for aggro units
        SetAggroUnitDesiredNodes();

        //Calculate the desired nodes for patrol units
        SetPatrolUnitDesiredNodes();

        //Build list of units ready to be moved
        List<Unit> toMove = FindObjectsOfType<Unit>().Where(x => x.GetDesiredNode() != null).ToList();

        //Move all units
        foreach (var unit in toMove)
        {
            GameEvents.instance.MoveUnit(unit, unit.GetDesiredNode().worldPosition);
        }

        //Exit the state
        ChangeState(States.MoveUnits);
    }

    //Calculate the desired nodes for patrol units
    private void SetPatrolUnitDesiredNodes()
    {
        //Grab list of enemy units in the "patrol" state
        List<EnemyUnit> enemyPatrolUnits = FindObjectsOfType<EnemyUnit>().Where(x => x.GetState() == EnemyUnit.EnemyStates.Patrol).ToList();

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

                //Save desired node if it's not a dupe
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

    //Upon turn passing, allow inputs again
    private void TurnPass()
    {
        canInput = true;
    }

    bool objectAtWorldPoint(Vector3 location, string tag)
    {
        Collider[] hitColliders = Physics.OverlapSphere(location, 0.1f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.CompareTag(tag))
            {
                return true;
            }
        }
        return false;
    }
}