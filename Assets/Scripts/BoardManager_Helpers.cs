using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class BoardManager : MonoBehaviour
{
/*
██╗░░██╗███████╗██╗░░░░░██████╗░███████╗██████╗░░██████╗
██║░░██║██╔════╝██║░░░░░██╔══██╗██╔════╝██╔══██╗██╔════╝
███████║█████╗░░██║░░░░░██████╔╝█████╗░░██████╔╝╚█████╗░
██╔══██║██╔══╝░░██║░░░░░██╔═══╝░██╔══╝░░██╔══██╗░╚═══██╗
██║░░██║███████╗███████╗██║░░░░░███████╗██║░░██║██████╔╝
╚═╝░░╚═╝╚══════╝╚══════╝╚═╝░░░░░╚══════╝╚═╝░░╚═╝╚═════╝░
*/

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

    private void ReadForInputs()
    {
        //Movement key(s) pressed
        if (GetInputVector() != Vector3.zero)
        {
            playerInput = PlayerInput.Move;
        }

        //Rest key pressed
        else if (Input.GetKey(KeyCode.JoystickButton1))
        {
            playerInput = PlayerInput.Rest;
        }

        //Loot key pressed
        else if (Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            playerInput = PlayerInput.Loot;
        }

        //No key pressed
        else
        {
            playerInput = PlayerInput.None;
        }
    }
}
