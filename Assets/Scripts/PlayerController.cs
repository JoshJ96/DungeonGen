using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    bool canMove = true;

    private void Awake()
    {
        GameEvents.instance.playerTurnStart += PlayerTurnStart;
        GameEvents.instance.enemyTurnStart += EnemyTurnStart;
        GameEvents.instance.boardStep += BoardStep;
    }

    private void BoardStep(Vector3 destination)
    {
        StartCoroutine(MovePlayer(destination));
        print("BoardStep ran from PlayerController.cs");
    }

    IEnumerator MovePlayer(Vector3 destination)
    {
        float totalMovementTime = 5f; //The amount of time for the movement to take
        float currentMovementTime = 0f;//The amount of time that has passed
        while (transform.position != destination)
        {
            currentMovementTime += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(transform.position, destination, currentMovementTime / totalMovementTime);
            yield return null;
        }
        yield return null;
    }

    private void PlayerTurnStart()
    {
        print("PlayerTurnStart ran from PlayerController.cs");
    }
    private void EnemyTurnStart()
    {
        print("EnemyTurnStart ran from PlayerController.cs");
    }

    void Update()
    {
        if (canMove)
        {
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputY = Input.GetAxisRaw("Vertical");

            if (Input.GetAxisRaw("Horizontal") != 0)
            {
                canMove = false;
                GameEvents.instance.BoardStep(new Vector3(inputX, 0, 0));
            }
            else if (Input.GetAxisRaw("Vertical") != 0)
            {
                canMove = false;
                GameEvents.instance.BoardStep(new Vector3(0, 0, inputY));
            }
        }
    }
}
