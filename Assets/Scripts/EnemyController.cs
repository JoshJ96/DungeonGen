using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    Patrol,
    Targeting,
    InAttackRange
}

public class EnemyController : MonoBehaviour
{
    public EnemyState currentState = EnemyState.Patrol;
    Pathfinding pathfinding;

    // Start is called before the first frame update
    private void Start()
    {
        GameEvents.instance.playerTurnStart += PlayerTurnStart;
        GameEvents.instance.enemyTurnStart += EnemyTurnStart;
        GameEvents.instance.boardStep += BoardStep;
        pathfinding = GameManager.instance.GetComponent<Pathfinding>();
    }
    /* NOTES for dev:
 * 
 * 1. If patrolling, move in a random direction
 * 
 * 2. If targeting player, A* pathfind towards player
 *      2b. If two enemys were to move to the same tile, pick an enemy at random to move
 *      
 * 3. If within attack range of player, perform an attack (enemy phase)
 *      3b. If multiple enemies are in attack range at the same time, pick an enemy at random to attack first
 */
    private void BoardStep(Vector3 obj)
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                RandomPatrolStep();
                break;
            case EnemyState.Targeting:
                GoTowardsPlayer();
                break;
            case EnemyState.InAttackRange:
                break;
            default:
                break;
        }
    }

    private void GoTowardsPlayer()
    {
        List<Node> path = pathfinding.GetPath(transform.position, PlayerController.instance.transform.position);
        StartCoroutine(MoveUnit(path[0].worldPosition));
    }

    private void EnemyTurnStart()
    {
    }

    private void PlayerTurnStart()
    {
    }

    private void RandomPatrolStep()
    {
        //UnityEngine.Random random = new UnityEngine.Random();
        int randomDirectionIndex = UnityEngine.Random.Range(0, 4);
        Vector3 direction = transform.position;
        if (randomDirectionIndex == 0)
        {
            direction.x = transform.position.x - 1;
        }
        if (randomDirectionIndex == 1)
        {
            direction.x = transform.position.x + 1;
        }
        if (randomDirectionIndex == 2)
        {
            direction.z = transform.position.z - 1;
        }
        if (randomDirectionIndex == 3)
        {
            direction.z = transform.position.z + 1;
        }


        StartCoroutine(MoveUnit(direction));
    }
    IEnumerator MoveUnit(Vector3 destination)
    {
        while (transform.position != destination)
        {
            transform.localPosition = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * 5.00f);
            yield return null;
        }
        yield return null;
    }
}
