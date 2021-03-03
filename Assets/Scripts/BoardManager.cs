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
    public void ChangeState(PlayerInput input)
    {
        playerInput = input;
    }

    #endregion

    List<EnemyUnit> enemyAttackingUnits = new List<EnemyUnit>();

    void Update()
    {
        switch (currentState)
        {
            case States.WaitingForPlayerInput:
                {
                    ReadForInputs();
                    switch (playerInput)
                    {
                        case PlayerInput.PrimaryAttack:
                            HandlePlayerAttack();
                            break;
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
                        case PlayerInput.DiagonalMode:
                            HandleDiagonalMode();
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
        if (!PlayerUnit.instance.isAttacking)
        {
            ChangeState(States.CalculatingMovements);
            MoveEnemyUnits();
        }
    }

    private void HandlePlayerAttack()
    {
        currentState = States.WaitingForPlayerAttackEnd;
        PlayerUnit.instance.isAttacking = true;
        PlayerUnit.instance.Attack();
    }

    private void HandlePlayerMovement()
    {
        Node currentPlayerNode = PlayerUnit.instance.GetCurrentNode();
        Node desiredNodeLocation = Grid.instance.grid[
            currentPlayerNode.gridX + Mathf.RoundToInt(GetInputVector().x),
            currentPlayerNode.gridY + Mathf.RoundToInt(GetInputVector().z)
            ];

        if (desiredNodeLocation.walkable)
        {
            ChangeState(States.CalculatingMovements);
            PlayerUnit.instance.SetDesiredNode(desiredNodeLocation);
            MoveEnemyUnits();
        }
        else
        {
            PlayerUnit.instance.RotateTowards(desiredNodeLocation.GetWorldPoint());
            GameEvents.instance.TurnPass();
        }
    }

    private void HandleDiagonalMode()
    {
        //Diagonal only desired nodes
        Node currentPlayerNode = PlayerUnit.instance.GetCurrentNode();

        Node desiredNodeLocation = Grid.instance.grid[
            currentPlayerNode.gridX + Mathf.RoundToInt(GetInputVector().x),
            currentPlayerNode.gridY + Mathf.RoundToInt(GetInputVector().z)
            ];

        if (desiredNodeLocation.gridX - currentPlayerNode.gridX == 0 || desiredNodeLocation.gridY - currentPlayerNode.gridY == 0)
        {
            return;
        }



        if (desiredNodeLocation.walkable)
        {
            ChangeState(States.CalculatingMovements);
            PlayerUnit.instance.SetDesiredNode(desiredNodeLocation);
            MoveEnemyUnits();
        }
        else
        {
            PlayerUnit.instance.RotateTowards(desiredNodeLocation.GetWorldPoint());
            GameEvents.instance.TurnPass();
        }
    }

    private void HandleLooting()
    {

    }

    private void HandleResting()
    {
        ChangeState(States.CalculatingMovements);
        MoveEnemyUnits();
    }

    private void HandleRotateMode()
    {
        if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
            return;

        //Todo: auto-rotate towards an enemy if around 3x3
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
            unit.MoveUnit(unit.GetDesiredNode().GetWorldPoint());
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
            Grid.instance.FindPath(unit.GetCurrentNode(), PlayerUnit.instance.GetComponent<Unit>().GetCurrentNode());

            //Save desired node if it's not a dupe
            if (Grid.instance.path != null)
            {
                if (Grid.instance.path.Count != 0)
                {
                    if (!IsDuplicateDesiredNode(Grid.instance.path[0]))
                    {
                        unit.SetDesiredNode(Grid.instance.path[0]);
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
            List<Node> surroundingNodes = Grid.instance.GetNeighbours(Grid.instance.NodeFromWorldPoint(unit.transform.position)).Where(x => x.walkable).ToList();

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
       // arrow8Dir.SetTrigger("DisableArrows");
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
       //arrow8Dir.SetTrigger("DisableArrows");
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