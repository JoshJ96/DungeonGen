using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Singleton
    public static PlayerController instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    public float test = 3;
    bool canMove = true;

    private void Start()
    {
        GameEvents.instance.playerTurnStart += PlayerTurnStart;
        GameEvents.instance.enemyTurnStart += EnemyTurnStart;
        GameEvents.instance.boardStep += BoardStep;
    }

    private void BoardStep(Vector3 destination)
    {
        canMove = false;
        StartCoroutine(MovePlayer(destination));
        print("BoardStep ran from PlayerController.cs");
    }

    IEnumerator MovePlayer(Vector3 destination)
    {
        while (transform.position != destination)
        {
            transform.localPosition = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * 5.00f);
            yield return null;
        }
        canMove = true;
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
            float inputZ = Input.GetAxisRaw("Vertical");

            if (Input.GetAxisRaw("Horizontal") != 0)
            {
                Vector3 destination = new Vector3(transform.position.x + Mathf.Sign(inputX), transform.position.y, transform.position.z);
                GameEvents.instance.BoardStep(destination);
            }
            else if (Input.GetAxisRaw("Vertical") != 0)
            {
                Vector3 destination = new Vector3(transform.position.x, transform.position.y, transform.position.z + Mathf.Sign(inputZ));
                GameEvents.instance.BoardStep(destination);
            }
        }
    }
}
