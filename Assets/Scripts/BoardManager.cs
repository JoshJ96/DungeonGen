using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class BoardManager : MonoBehaviour
{
    #region Board State Machine Setup

    private States currentState;
    public enum States
    {
        WaitingForPlayerInput,
        CalculatingMovements,
        WaitingForEnemyMovementEnd,
        EnemyAttackPhase,
        Looting
    }
    public void ChangeState(States state) => currentState = state;

    #endregion

    #region Input State Machine Setup

    private PlayerInput playerInput = PlayerInput.None;
    public enum PlayerInput
    {
        None,
        Move,
        Rest,
        Loot
    }
    public void ChangeState(PlayerInput input) => playerInput = input;

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
                HandleWaitingForPlayerInput();
                break;
            case States.WaitingForEnemyMovementEnd:
                HandleWaitingForEnemyMovementEnd();
                break;
            case States.EnemyAttackPhase:
                HandleEnemyAttack();
                break;
            default:
                break;
        }
    }

    /**************************
     * Player Action Handlers *
     **************************/
    private void HandleWaitingForPlayerInput()
    {
        if (canInput)
        {
            ReadForInputs();
            switch (playerInput)
            {
                case PlayerInput.Move:
                    HandlePlayerMovement();
                    break;
                case PlayerInput.Rest:
                    HandleResting();
                    break;
                case PlayerInput.Loot:
                    HandleLooting();
                    break;
                default:
                    break;
            }
        }
    }

    private void HandlePlayerMovement()
    {
        canInput = false;
        Vector3 desiredPlayerLocation = new Vector3(
            PlayerUnit.instance.transform.position.x + Mathf.RoundToInt(GetInputVector().x),
            0,
            PlayerUnit.instance.transform.position.z + Mathf.RoundToInt(GetInputVector().z));

        Node checkNodeAtDesiredLocation = worldGrid.NodeFromWorldPoint(desiredPlayerLocation);

        if (checkNodeAtDesiredLocation.walkable)
        {
            ChangeState(States.CalculatingMovements);
            PlayerUnit.instance.SetDesiredNode(checkNodeAtDesiredLocation);
            MoveEnemyUnits();
        }
        else
        {
            PlayerUnit.instance.RotateTowards(desiredPlayerLocation);
            canInput = true;
        }
    }

    private void HandleLooting()
    {
        if (objectAtWorldPoint(PlayerUnit.instance.transform.position + Vector3.forward, "Chest"))
        {
            if (PlayerUnit.instance.GetDirection() == Unit.Direction.North)
            {
                //Loot stuff
            }
        }
    }

    private void HandleResting()
    {
        ChangeState(States.CalculatingMovements);
        canInput = false;
        MoveEnemyUnits();
    }

    /**************************
    * Enemy Action Handlers *
    **************************/
    private void MoveEnemyUnits()
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
        ChangeState(States.WaitingForEnemyMovementEnd);
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

    private void HandleEnemyAttack()
    {
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
    }

    private void HandleWaitingForEnemyMovementEnd()
    {
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
    }
}