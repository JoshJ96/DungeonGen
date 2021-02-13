using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public partial class BoardManager : MonoBehaviour
{
    #region Board State Machine Setup

    private States currentState = States.WaitingForPlayerInput;
    public enum States
    {
        WaitingForPlayerInput,
        CalculatingMovements,
        WaitingForEnemyMovementEnd,
        EnemyAttackPhase,
        Looting,
        WaitingForPlayerAttackEnd
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
        Loot,
        PrimaryAttack,
        RotateMode,
        DiagonalMode
    }
    public void ChangeState(PlayerInput input) => playerInput = input;

    #endregion

    Grid worldGrid;
    Pathfinding pathfinding;
    List<EnemyUnit> enemyAttackingUnits = new List<EnemyUnit>();
    bool canInput = true;
    public GameObject damageIndicatorObject;

    private void Start()
    {
        worldGrid = GetComponent<Grid>();
        pathfinding = GetComponent<Pathfinding>();
        GameEvents.instance.turnPass += TurnPass;
        GameEvents.instance.doDamage += DoDamage;
    }

    private void DoDamage(Unit doingDamage, Unit takingDamage, int amount)
    {
        GameObject damageIndicator = Instantiate(damageIndicatorObject, takingDamage.transform.position + Vector3.up, Quaternion.identity);
        damageIndicator.GetComponent<TMPro.TextMeshPro>().text = $"{amount}";
    }

    void Update()
    {
        switch (currentState)
        {
            case States.WaitingForPlayerInput:
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
                        case PlayerInput.RotateMode:
                            HandleRotateMode();
                            break;
                        case PlayerInput.PrimaryAttack:
                            HandlePlayerAttack();
                            break;
                        default:
                            break;
                    }
                }
                break;
            case States.WaitingForEnemyMovementEnd:
                HandleWaitingForEnemyMovementEnd();
                break;
            case States.EnemyAttackPhase:
                HandleEnemyAttack();
                break;
            case States.WaitingForPlayerAttackEnd:
                HandleWaitingForPlayerAttackEnd();
                break;
            default:
                break;
        }
    }

    /*************************
    * Player Action Handlers *
    **************************/

    private void HandleWaitingForPlayerAttackEnd()
    {
        canInput = false;
        if (!PlayerUnit.instance.isAttacking)
        {
            ChangeState(States.CalculatingMovements);
            MoveEnemyUnits();
        }
    }

    private void HandlePlayerAttack()
    {
        canInput = false;
        currentState = States.WaitingForPlayerAttackEnd;
        PlayerUnit.instance.isAttacking = true;
        PlayerUnit.instance.Attack();
    }

    private void HandlePlayerMovement()
    {
        canInput = false;
        Node currentPlayerNode = PlayerUnit.instance.GetCurrentNode();
        Node desiredNodeLocation = worldGrid.grid[
            currentPlayerNode.gridX + Mathf.RoundToInt(GetInputVector().x),
            currentPlayerNode.gridY + Mathf.RoundToInt(GetInputVector().z)
            ];

        //Node checkNodeAtDesiredLocation = worldGrid.NodeFromWorldPoint(desiredPlayerLocation);

        if (desiredNodeLocation.walkable)
        {
            ChangeState(States.CalculatingMovements);
            PlayerUnit.instance.SetDesiredNode(desiredNodeLocation);
            MoveEnemyUnits();
        }
        else
        {
            PlayerUnit.instance.RotateTowards(desiredNodeLocation.worldPosition);
            canInput = true;
            GameEvents.instance.TurnPass();
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

    private void HandleRotateMode()
    {
        //Todo: auto-rotate towards an enemy if around 3x3
        if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
        {
            return;
        }
        Vector3 rotateTowards = new Vector3(
            PlayerUnit.instance.transform.position.x + Mathf.RoundToInt(GetInputVector().x),
            0,
            PlayerUnit.instance.transform.position.z + Mathf.RoundToInt(GetInputVector().z));

        PlayerUnit.instance.RotateTowards(rotateTowards);
    }

    /************************
    * Enemy Action Handlers *
    *************************/
    private void MoveEnemyUnits()
    {
        //All enemy units will scan for player and change their states accordingly
        foreach (EnemyUnit unit in FindObjectsOfType<EnemyUnit>())
        {
            unit.ScanForPlayerInAggroRange();
        }

        //Calculate the desired nodes for aggro units
        SetAggroUnitDesiredNodes();

        //Calculate the desired nodes for patrol units
        SetPatrolUnitDesiredNodes();

        //Build list of units ready to be moved
        List<Unit> toMove = FindObjectsOfType<Unit>().Where(x => x.GetDesiredNode() != null).ToList();

        //Move all units
        foreach (var unit in toMove)
        {
            unit.MoveUnit(unit.GetDesiredNode().worldPosition);
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
            foreach (EnemyUnit unit in FindObjectsOfType<EnemyUnit>())
            {
                unit.ScanForPlayerInAttackRange();
            }

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