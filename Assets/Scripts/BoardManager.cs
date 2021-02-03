using System.Collections;
using System.Collections.Generic;
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
    States currentState = States.WaitingForPlayerInput;
    PlayerUnit PlayerUnit;

    private void Start()
    {
        worldGrid = GetComponent<Grid>();
        PlayerUnit = FindObjectOfType<PlayerUnit>();
    }

    void Update()
    {
        switch (currentState)
        {
            case States.WaitingForPlayerInput:
                float inputX = Input.GetAxisRaw("Horizontal");
                float inputZ = Input.GetAxisRaw("Vertical");
                if (inputX != 0)
                {
                    Vector3 desiredLocation = new Vector3(PlayerUnit.transform.localPosition.x + Mathf.Sign(inputX), 0, (PlayerUnit.transform.localPosition.z));
                    Node toCheck = worldGrid.NodeFromWorldPoint(desiredLocation);
                    print(toCheck.walkable);
                    if (toCheck.walkable)
                    {
                        
                    }
                    
                }
                else if (inputZ != 0)
                {
                    Vector3 desiredLocation = new Vector3(PlayerUnit.transform.localPosition.x, 0, (PlayerUnit.transform.localPosition.z) + Mathf.Sign(inputZ));
                    Node toCheck = worldGrid.NodeFromWorldPoint(desiredLocation);
                    print(toCheck.walkable);
                    if (toCheck.walkable)
                    {

                    }
                }
                break;

            case States.MoveAndDeclump:
                break;

            case States.EnemyAttackPhase:
                break;

            case States.ExtraPhase:
                break;

            default:
                break;
        }
    }
}
